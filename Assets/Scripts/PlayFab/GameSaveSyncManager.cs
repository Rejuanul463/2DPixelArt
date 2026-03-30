using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PlayFab;
using PlayFab.ClientModels;

namespace Virtuery.PlayFab
{
    public class GameSaveSyncManager : MonoBehaviour
    {
        public static GameSaveSyncManager Instance { get; private set; }

        [Header("Sync Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float retryDelay = 5f;
        [SerializeField] private int maxRetryAttempts = 3;

        public event Action OnSyncComplete;
        public event Action<string> OnSyncError;
        public event Action OnCloudSaveApplied;

        private bool isSyncing = false;
        private Queue<GameSaveOperation> offlineQueue = new Queue<GameSaveOperation>();
        private const string OFFLINE_GAME_QUEUE_KEY = "OfflineGameSaveQueue";
        private const int MAX_OFFLINE_QUEUE_SIZE = 50;

        private string LocalSavePath =>
#if UNITY_EDITOR
            Application.dataPath + "/save.json";
#else
            Application.persistentDataPath + "/save.json";
#endif

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
            LoadOfflineQueue();

            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess += OnLoginSuccess;
                PlayFabAuthService.Instance.OnLogout += OnLogout;
            }

            if (PlayerDataSyncManager.Instance != null)
            {
                PlayerDataSyncManager.Instance.OnConnectionStateChanged += OnConnectionStateChanged;
            }

            if (IsAuthenticated())
            {
                SyncFromCloud();
            }
        }

        private void OnDestroy()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess -= OnLoginSuccess;
                PlayFabAuthService.Instance.OnLogout -= OnLogout;
            }

            if (PlayerDataSyncManager.Instance != null)
            {
                PlayerDataSyncManager.Instance.OnConnectionStateChanged -= OnConnectionStateChanged;
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

        private bool IsOnline()
        {
            return PlayerDataSyncManager.Instance != null && PlayerDataSyncManager.Instance.IsOnline;
        }

        private void OnLoginSuccess(LoginResult result)
        {
            SyncFromCloud();
        }

        private void OnLogout()
        {
            offlineQueue.Clear();
            SaveOfflineQueue();
        }

        private void OnConnectionStateChanged(bool isOnline)
        {
            if (isOnline && IsAuthenticated())
            {
                LogDebug("Connection restored - processing offline queue...");
                ProcessOfflineQueue();
            }
        }

        #region Public API

        public void OnLocalSave()
        {
            if (!IsAuthenticated())
            {
                LogDebug("Not authenticated - skipping cloud sync");
                return;
            }

            if (!IsOnline())
            {
                LogDebug("Offline - queuing sync for later");
                QueueOfflineSync();
                return;
            }

            SyncToCloud();
        }

        public void SyncFromCloud()
        {
            if (!IsAuthenticated())
            {
                LogDebug("Cannot sync from cloud: Not authenticated");
                return;
            }

            if (isSyncing) return;

            isSyncing = true;
            LogDebug("Syncing game save from cloud...");

            var request = new GetUserDataRequest();

            PlayFabClientAPI.GetUserData(request,
                result =>
                {
                    isSyncing = false;
                    ProcessCloudGameSave(result.Data);
                },
                error =>
                {
                    isSyncing = false;
                    HandleSyncError("Failed to sync game save from cloud", error);
                });
        }

        public void SyncToCloud()
        {
            if (!IsAuthenticated())
            {
                QueueOfflineSync();
                return;
            }

            if (isSyncing) return;

            if (!File.Exists(LocalSavePath))
            {
                LogDebug("No local save file to sync");
                return;
            }

            isSyncing = true;
            string localJson = File.ReadAllText(LocalSavePath);

            LogDebug("Syncing game save to cloud...");

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { PlayerDataKeys.GAME_SAVE_DATA_KEY, localJson }
                }
            };

            PlayFabClientAPI.UpdateUserData(request,
                result =>
                {
                    isSyncing = false;
                    LogDebug("Game save synced to cloud successfully");
                    OnSyncComplete?.Invoke();
                    ProcessOfflineQueue();
                },
                error =>
                {
                    isSyncing = false;
                    if (error.Error == PlayFabErrorCode.ConnectionError)
                    {
                        QueueOfflineSync();
                    }
                    HandleSyncError("Failed to sync game save to cloud", error);
                });
        }

        public DateTime? GetLocalSaveTimestamp()
        {
            if (!File.Exists(LocalSavePath))
                return null;

            try
            {
                string json = File.ReadAllText(LocalSavePath);
                var saveData = JsonUtility.FromJson<GameSaveDataWrapper>(json);
                if (!string.IsNullOrEmpty(saveData.lastUpdatedAt))
                {
                    if (DateTime.TryParse(saveData.lastUpdatedAt, out DateTime timestamp))
                        return timestamp;
                }
                return File.GetLastWriteTimeUtc(LocalSavePath);
            }
            catch
            {
                return File.GetLastWriteTimeUtc(LocalSavePath);
            }
        }

        #endregion

        #region Cloud Data Processing

        private void ProcessCloudGameSave(Dictionary<string, UserDataRecord> cloudData)
        {
            if (cloudData == null || !cloudData.TryGetValue(PlayerDataKeys.GAME_SAVE_DATA_KEY, out UserDataRecord record))
            {
                LogDebug("No cloud game save found");
                UploadLocalIfNewer();
                return;
            }

            string cloudJson = record.Value;
            DateTime? cloudTimestamp = ExtractTimestamp(cloudJson);
            DateTime? localTimestamp = GetLocalSaveTimestamp();

            if (cloudTimestamp.HasValue && localTimestamp.HasValue)
            {
                if (cloudTimestamp.Value > localTimestamp.Value)
                {
                    LogDebug($"Cloud save is newer ({cloudTimestamp.Value} > {localTimestamp.Value}) - applying cloud save");
                    ApplyCloudSave(cloudJson);
                }
                else
                {
                    LogDebug($"Local save is newer or equal ({localTimestamp.Value} >= {cloudTimestamp.Value}) - uploading to cloud");
                    SyncToCloud();
                }
            }
            else if (cloudTimestamp.HasValue && !localTimestamp.HasValue)
            {
                LogDebug("No local save - downloading from cloud");
                ApplyCloudSave(cloudJson);
            }
            else if (!cloudTimestamp.HasValue && localTimestamp.HasValue)
            {
                LogDebug("No cloud save - uploading local");
                SyncToCloud();
            }
            else
            {
                LogDebug("No saves found anywhere");
            }

            OnSyncComplete?.Invoke();
        }

        private void UploadLocalIfNewer()
        {
            if (File.Exists(LocalSavePath))
            {
                SyncToCloud();
            }
        }

        private DateTime? ExtractTimestamp(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<GameSaveDataWrapper>(json);
                if (!string.IsNullOrEmpty(wrapper.lastUpdatedAt))
                {
                    if (DateTime.TryParse(wrapper.lastUpdatedAt, out DateTime timestamp))
                        return timestamp;
                }
            }
            catch { }
            return null;
        }

        private void ApplyCloudSave(string cloudJson)
        {
            try
            {
                string directory = Path.GetDirectoryName(LocalSavePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(LocalSavePath, cloudJson);
                LogDebug("Cloud save applied to local file");

                OnCloudSaveApplied?.Invoke();
            }
            catch (Exception e)
            {
                LogError($"Failed to apply cloud save: {e.Message}");
                OnSyncError?.Invoke($"Failed to apply cloud save: {e.Message}");
            }
        }

        #endregion

        #region Offline Operations

        private void QueueOfflineSync()
        {
            if (offlineQueue.Count >= MAX_OFFLINE_QUEUE_SIZE)
            {
                offlineQueue.Dequeue();
            }

            offlineQueue.Enqueue(new GameSaveOperation
            {
                timestamp = DateTime.UtcNow,
                type = GameSaveOperationType.FullSync
            });

            SaveOfflineQueue();
            LogDebug("Queued offline game save sync");
        }

        private void SaveOfflineQueue()
        {
            try
            {
                var queueList = new List<GameSaveOperation>(offlineQueue);
                string json = JsonUtility.ToJson(new GameSaveQueueWrapper { queue = queueList });
                PlayerPrefs.SetString(OFFLINE_GAME_QUEUE_KEY, json);
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
                string json = PlayerPrefs.GetString(OFFLINE_GAME_QUEUE_KEY, "");
                if (!string.IsNullOrEmpty(json))
                {
                    var wrapper = JsonUtility.FromJson<GameSaveQueueWrapper>(json);
                    if (wrapper?.queue != null)
                    {
                        offlineQueue = new Queue<GameSaveOperation>(wrapper.queue);
                        LogDebug($"Loaded {offlineQueue.Count} offline game save operations");
                    }
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to load offline queue: {e.Message}");
                offlineQueue = new Queue<GameSaveOperation>();
            }
        }

        private void ProcessOfflineQueue()
        {
            if (offlineQueue.Count == 0) return;
            if (!IsAuthenticated()) return;
            if (!IsOnline()) return;

            LogDebug($"Processing {offlineQueue.Count} offline game save operations...");

            while (offlineQueue.Count > 0)
            {
                var operation = offlineQueue.Dequeue();
                if (operation.type == GameSaveOperationType.FullSync)
                {
                    SyncToCloud();
                    break;
                }
            }

            SaveOfflineQueue();
        }

        #endregion

        #region Error Handling

        private void HandleSyncError(string context, PlayFabError error)
        {
            string errorMsg = $"{context}: {error.ErrorMessage}";
            LogError(errorMsg);
            OnSyncError?.Invoke(errorMsg);
        }

        #endregion

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameSaveSyncManager] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[GameSaveSyncManager] {message}");
        }
    }

    #region Data Models

    [Serializable]
    public class GameSaveDataWrapper
    {
        public string lastUpdatedAt;
    }

    [Serializable]
    public class GameSaveOperation
    {
        public DateTime timestamp;
        public GameSaveOperationType type;
    }

    public enum GameSaveOperationType
    {
        FullSync
    }

    [Serializable]
    public class GameSaveQueueWrapper
    {
        public List<GameSaveOperation> queue;
    }

    #endregion
}
