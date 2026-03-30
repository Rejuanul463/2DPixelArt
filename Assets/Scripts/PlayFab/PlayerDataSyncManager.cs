using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

namespace Virtuery.PlayFab
{
    public class PlayerDataSyncManager : MonoBehaviour
    {
        public static PlayerDataSyncManager Instance { get; private set; }

        [Header("Sync Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool autoSyncOnLogin = true;
        [SerializeField] private float syncInterval = 300f;
        [SerializeField] private float retryDelay = 5f;
        [SerializeField] private int maxRetryAttempts = 3;

        [Header("Offline Settings")]
        [SerializeField] private bool enableOfflineCache = true;
        [SerializeField] private int maxOfflineQueueSize = 100;

        public event Action<PlayerProfile> OnProfileLoaded;
        public event Action OnProfileSaved;
        public event Action OnSyncComplete;
        public event Action<string> OnSyncError;
        public event Action<bool> OnConnectionStateChanged;

        private PlayerProfile cachedProfile;
        private Queue<OfflineDataOperation> offlineQueue = new Queue<OfflineDataOperation>();
        private bool isOnline = true;
        private bool isSyncing = false;
        private Coroutine syncCoroutine;
        private Coroutine connectionCheckCoroutine;

        private const string OFFLINE_QUEUE_KEY = "OfflineQueue";
        private const string LOCAL_PROFILE_KEY = "LocalProfile";
        private const string LAST_SYNC_KEY = "LastSyncTime";
        private const string PROFILE_IMAGE_KEY = "ProfileImageData";
        private const string LOCAL_PROFILE_USERNAME_KEY = "LocalProfile_UserName";

        public bool IsOnline => isOnline;
        public PlayerProfile CurrentProfile => cachedProfile;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadLocalProfile();

            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess += OnLoginSuccess;
                PlayFabAuthService.Instance.OnLogout += OnLogout;
            }

            if (autoSyncOnLogin && IsAuthenticated())
            {
                SyncFromCloud();
            }

            connectionCheckCoroutine = StartCoroutine(ConnectionCheckRoutine());
        }

        private void OnDestroy()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess -= OnLoginSuccess;
                PlayFabAuthService.Instance.OnLogout -= OnLogout;
            }

            if (syncCoroutine != null)
            {
                StopCoroutine(syncCoroutine);
            }

            if (connectionCheckCoroutine != null)
            {
                StopCoroutine(connectionCheckCoroutine);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private bool IsAuthenticated()
        {
            return PlayFabAuthService.Instance != null && PlayFabAuthService.Instance.IsLoggedIn;
        }

        private void OnLoginSuccess(LoginResult result)
        {
            // Clear cached profile from previous user before syncing new user's data
            cachedProfile = new PlayerProfile();
            PlayerPrefs.DeleteKey(LOCAL_PROFILE_KEY);
            PlayerPrefs.DeleteKey(LOCAL_PROFILE_USERNAME_KEY);
            PlayerPrefs.DeleteKey(PROFILE_IMAGE_KEY);
            PlayerPrefs.Save();
            
            if (autoSyncOnLogin)
            {
                SyncFromCloud();
            }

            if (syncCoroutine == null)
            {
                syncCoroutine = StartCoroutine(PeriodicSyncRoutine());
            }
        }

        private void OnLogout()
        {
            if (syncCoroutine != null)
            {
                StopCoroutine(syncCoroutine);
                syncCoroutine = null;
            }

            // Clear all cached profile data
            cachedProfile = new PlayerProfile();
            PlayerPrefs.DeleteKey(LOCAL_PROFILE_KEY);
            PlayerPrefs.DeleteKey(LOCAL_PROFILE_USERNAME_KEY);
            PlayerPrefs.DeleteKey(PROFILE_IMAGE_KEY);
            PlayerPrefs.Save();
            LogDebug("Cleared all cached profile data on logout");
        }

        #region Cloud Sync Operations

        public void SyncFromCloud()
        {
            if (!IsAuthenticated())
            {
                OnSyncError?.Invoke("Cannot sync: Not authenticated");
                return;
            }

            if (isSyncing) return;

            isSyncing = true;
            LogDebug("Syncing profile from cloud...");

            var request = new GetUserDataRequest();

            PlayFabClientAPI.GetUserData(request,
                result =>
                {
                    isSyncing = false;
                    ProcessCloudData(result.Data);
                    SetOnlineState(true);
                },
                error =>
                {
                    isSyncing = false;
                    SetOnlineState(false);
                    HandleSyncError("Failed to sync from cloud", error);
                });
        }

        public void SyncToCloud()
        {
            if (!IsAuthenticated())
            {
                QueueOfflineOperation(new OfflineDataOperation
                {
                    operationType = OfflineOperationType.FullSync,
                    data = cachedProfile != null ? cachedProfile.ToDictionary() : new Dictionary<string, string>()
                });
                return;
            }

            if (isSyncing) return;

            if (cachedProfile == null)
            {
                LogDebug("No profile to sync");
                return;
            }

            isSyncing = true;
            cachedProfile.updatedAt = DateTime.UtcNow;

            var request = new UpdateUserDataRequest
            {
                Data = cachedProfile.ToDictionary()
            };

            PlayFabClientAPI.UpdateUserData(request,
                result =>
                {
                    isSyncing = false;
                    SetOnlineState(true);
                    SaveLocalProfile();
                    OnProfileSaved?.Invoke();
                    OnSyncComplete?.Invoke();
                    LogDebug("Profile synced to cloud successfully");
                    ProcessOfflineQueue();
                },
                error =>
                {
                    isSyncing = false;
                    SetOnlineState(false);
                    HandleSyncError("Failed to sync to cloud", error);
                });
        }

        public void SyncSpecificData(Dictionary<string, string> data)
        {
            if (!IsAuthenticated())
            {
                QueueOfflineOperation(new OfflineDataOperation
                {
                    operationType = OfflineOperationType.PartialUpdate,
                    data = data
                });
                return;
            }

            var request = new UpdateUserDataRequest
            {
                Data = data
            };

            PlayFabClientAPI.UpdateUserData(request,
                result =>
                {
                    SetOnlineState(true);
                    UpdateCachedData(data);
                    SaveLocalProfile();
                    OnSyncComplete?.Invoke();
                    LogDebug($"Synced {data.Count} data entries to cloud");
                },
                error =>
                {
                    SetOnlineState(false);
                    QueueOfflineOperation(new OfflineDataOperation
                    {
                        operationType = OfflineOperationType.PartialUpdate,
                        data = data
                    });
                    HandleSyncError("Failed to sync specific data", error);
                });
        }

        private void ProcessCloudData(Dictionary<string, UserDataRecord> cloudData)
        {
            if (cloudData == null)
            {
                LogDebug("No cloud data found");
                if (cachedProfile == null)
                {
                    cachedProfile = new PlayerProfile();
                }
                OnProfileLoaded?.Invoke(cachedProfile);
                return;
            }

            var dataDict = new Dictionary<string, string>();
            foreach (var kvp in cloudData)
            {
                dataDict[kvp.Key] = kvp.Value.Value;
            }

            var cloudProfile = PlayerProfile.FromDictionary(dataDict);

            // Always use cloud profile as the source of truth after login
            // (we cleared the cached profile in OnLoginSuccess)
            cachedProfile = cloudProfile;

            // If userName is empty in cloud, try to get it from PlayFabAuthService
            if (string.IsNullOrEmpty(cachedProfile.userName) && PlayFabAuthService.Instance != null)
            {
                string displayName = PlayFabAuthService.Instance.DisplayName;
                if (!string.IsNullOrEmpty(displayName))
                {
                    cachedProfile.userName = displayName;
                }
            }

            SaveLocalProfile();
            PlayerPrefs.SetString(LAST_SYNC_KEY, DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();

            OnProfileLoaded?.Invoke(cachedProfile);
            OnSyncComplete?.Invoke();
            LogDebug("Cloud data processed successfully");
        }

        private PlayerProfile MergeProfiles(PlayerProfile local, PlayerProfile cloud)
        {
            DateTime localUpdated = local.updatedAt;
            DateTime cloudUpdated = cloud.updatedAt;

            if (cloudUpdated > localUpdated)
            {
                LogDebug("Using cloud profile (newer)");
                return cloud;
            }
            else
            {
                LogDebug("Using local profile (newer or same)");
                return local;
            }
        }

        private void UpdateCachedData(Dictionary<string, string> data)
        {
            if (cachedProfile == null)
            {
                cachedProfile = new PlayerProfile();
            }

            foreach (var kvp in data)
            {
                switch (kvp.Key)
                {
                    case PlayerDataKeys.USER_NAME:
                        cachedProfile.userName = kvp.Value;
                        break;
                    case PlayerDataKeys.USER_GENDER:
                        cachedProfile.userGender = kvp.Value;
                        break;
                    case PlayerDataKeys.SELECTED_CHARACTER:
                        cachedProfile.selectedCharacter = kvp.Value;
                        break;
                    case PlayerDataKeys.PERSONALITY_TRAITS:
                        cachedProfile.personalityTraits = kvp.Value;
                        break;
                    case PlayerDataKeys.SUBSCRIPTION_TIER:
                        cachedProfile.subscriptionTier = kvp.Value;
                        break;
                    case PlayerDataKeys.CREDITS:
                        if (int.TryParse(kvp.Value, out int credits))
                            cachedProfile.credits = credits;
                        break;
                    case PlayerDataKeys.MESSAGE_LIMIT:
                        if (int.TryParse(kvp.Value, out int limit))
                            cachedProfile.messageLimit = limit;
                        break;
                    case PlayerDataKeys.SETTINGS_JSON:
                        try
                        {
                            cachedProfile.settings = JsonUtility.FromJson<PlayerSettings>(kvp.Value);
                        }
                        catch { }
                        break;
                }
            }

            cachedProfile.updatedAt = DateTime.UtcNow;
        }

        #endregion

        #region Offline Operations

        private void QueueOfflineOperation(OfflineDataOperation operation)
        {
            if (!enableOfflineCache) return;

            if (offlineQueue.Count >= maxOfflineQueueSize)
            {
                offlineQueue.Dequeue();
            }

            offlineQueue.Enqueue(operation);
            SaveOfflineQueue();
            LogDebug($"Queued offline operation: {operation.operationType}");
        }

        private void SaveOfflineQueue()
        {
            try
            {
                string json = JsonConvert.SerializeObject(new List<OfflineDataOperation>(offlineQueue));
                PlayerPrefs.SetString(OFFLINE_QUEUE_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                LogError($"Failed to save offline queue: {e.Message}");
            }
        }

        private void LoadOfflineQueue()
        {
            try
            {
                string json = PlayerPrefs.GetString(OFFLINE_QUEUE_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    var queue = JsonConvert.DeserializeObject<List<OfflineDataOperation>>(json);
                    offlineQueue = new Queue<OfflineDataOperation>(queue);
                    LogDebug($"Loaded {offlineQueue.Count} offline operations from queue");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to load offline queue: {e.Message}");
                offlineQueue = new Queue<OfflineDataOperation>();
            }
        }

        private void ProcessOfflineQueue()
        {
            if (offlineQueue.Count == 0) return;
            if (!IsAuthenticated()) return;

            LogDebug($"Processing {offlineQueue.Count} offline operations...");

            int processed = 0;
            while (offlineQueue.Count > 0 && processed < 10)
            {
                var operation = offlineQueue.Dequeue();

                if (operation.operationType == OfflineOperationType.FullSync)
                {
                    SyncToCloud();
                }
                else if (operation.operationType == OfflineOperationType.PartialUpdate)
                {
                    SyncSpecificData(operation.data);
                }

                processed++;
            }

            SaveOfflineQueue();
        }

        private void LoadLocalProfile()
        {
            try
            {
                string json = PlayerPrefs.GetString(LOCAL_PROFILE_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    cachedProfile = JsonUtility.FromJson<PlayerProfile>(json);
                    LogDebug("Loaded profile from local cache");
                }
                else
                {
                    cachedProfile = new PlayerProfile();
                }

                LoadOfflineQueue();
            }
            catch (Exception e)
            {
                LogError($"Failed to load local profile: {e.Message}");
                cachedProfile = new PlayerProfile();
            }
        }

        private void SaveLocalProfile()
        {
            try
            {
                if (cachedProfile != null)
                {
                    string json = JsonUtility.ToJson(cachedProfile);
                    PlayerPrefs.SetString(LOCAL_PROFILE_KEY, json);
                    PlayerPrefs.Save();
                    LogDebug("Saved profile to local cache");
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to save local profile: {e.Message}");
            }
        }

        #endregion

        #region Connection Management

        private IEnumerator ConnectionCheckRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(30f);

                if (IsAuthenticated())
                {
                    CheckConnection();
                }
            }
        }

        private void CheckConnection()
        {
            if (!IsAuthenticated()) return;

            var request = new GetUserDataRequest
            {
                Keys = new List<string> { PlayerDataKeys.LAST_LOGIN }
            };

            PlayFabClientAPI.GetUserData(request,
                result => SetOnlineState(true),
                error => SetOnlineState(false));
        }

        private void SetOnlineState(bool online)
        {
            if (isOnline != online)
            {
                isOnline = online;
                OnConnectionStateChanged?.Invoke(online);

                if (online)
                {
                    LogDebug("Connection restored - syncing...");
                    ProcessOfflineQueue();
                    SyncFromCloud();
                }
                else
                {
                    LogDebug("Connection lost - switching to offline mode");
                }
            }
        }

        #endregion

        #region Periodic Sync

        private IEnumerator PeriodicSyncRoutine()
        {
            while (IsAuthenticated())
            {
                yield return new WaitForSeconds(syncInterval);

                if (isOnline && !isSyncing)
                {
                    SyncFromCloud();
                }
            }
        }

        #endregion

        #region Error Handling

        private void HandleSyncError(string context, PlayFabError error)
        {
            string errorMsg = $"{context}: {error.ErrorMessage}";
            LogError(errorMsg);

            if (error.Error == PlayFabErrorCode.ConnectionError)
            {
                SetOnlineState(false);
            }

            OnSyncError?.Invoke(errorMsg);
        }

        #endregion

        #region Public API

        public PlayerProfile GetProfile()
        {
            return cachedProfile ?? (cachedProfile = new PlayerProfile());
        }

        public void UpdateProfile(PlayerProfile profile)
        {
            cachedProfile = profile;
            cachedProfile.updatedAt = DateTime.UtcNow;
            SaveLocalProfile();
            SyncToCloud();
        }

        public void UpdateProfileField(string key, string value)
        {
            SyncSpecificData(new Dictionary<string, string> { { key, value } });
        }

        public void UpdateSettings(PlayerSettings settings)
        {
            if (cachedProfile != null)
            {
                cachedProfile.settings = settings;
                cachedProfile.updatedAt = DateTime.UtcNow;
                SaveLocalProfile();

                SyncSpecificData(new Dictionary<string, string>
                {
                    { PlayerDataKeys.SETTINGS_JSON, JsonUtility.ToJson(settings) }
                });
            }
        }

        public void UpdateSelectedCharacter(string characterId)
        {
            if (cachedProfile != null)
            {
                cachedProfile.selectedCharacter = characterId;
                cachedProfile.updatedAt = DateTime.UtcNow;
                SaveLocalProfile();

                SyncSpecificData(new Dictionary<string, string>
                {
                    { PlayerDataKeys.SELECTED_CHARACTER, characterId }
                });
            }
        }

        public void UpdateSubscriptionTier(string tier)
        {
            if (cachedProfile != null)
            {
                cachedProfile.subscriptionTier = tier;
                cachedProfile.updatedAt = DateTime.UtcNow;
                SaveLocalProfile();

                SyncSpecificData(new Dictionary<string, string>
                {
                    { PlayerDataKeys.SUBSCRIPTION_TIER, tier }
                });
            }
        }

        public void UpdateCredits(int credits)
        {
            if (cachedProfile != null)
            {
                cachedProfile.credits = credits;
                cachedProfile.updatedAt = DateTime.UtcNow;
                SaveLocalProfile();

                SyncSpecificData(new Dictionary<string, string>
                {
                    { PlayerDataKeys.CREDITS, credits.ToString() }
                });
            }
        }

        public void ForceSync()
        {
            if (isOnline)
            {
                SyncToCloud();
            }
            else
            {
                OnSyncError?.Invoke("Cannot force sync: Offline");
            }
        }

        public DateTime GetLastSyncTime()
        {
            string lastSync = PlayerPrefs.GetString(LAST_SYNC_KEY, "");
            if (DateTime.TryParse(lastSync, out DateTime result))
            {
                return result;
            }
            return DateTime.MinValue;
        }

        #endregion

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayerDataSyncManager] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[PlayerDataSyncManager] {message}");
        }
    }

    #region Offline Data Models

    [Serializable]
    public class OfflineDataOperation
    {
        public OfflineOperationType operationType;
        public Dictionary<string, string> data;
        public DateTime timestamp;

        public OfflineDataOperation()
        {
            timestamp = DateTime.UtcNow;
            data = new Dictionary<string, string>();
        }
    }

    public enum OfflineOperationType
    {
        FullSync,
        PartialUpdate
    }

    #endregion
}