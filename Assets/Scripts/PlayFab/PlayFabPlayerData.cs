using UnityEngine;
using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

namespace Virtuery.PlayFab
{
    public class PlayFabPlayerData : MonoBehaviour
    {
        public static PlayFabPlayerData Instance { get; private set; }

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;

        public event Action<Dictionary<string, string>> OnDataLoaded;
        public event Action OnDataSaved;
        public event Action<string> OnDataError;

        private Dictionary<string, string> cachedPlayerData = new Dictionary<string, string>();
        private Dictionary<string, string> cachedTitleData = new Dictionary<string, string>();

        public static class DataKeys
        {
            public const string USER_NAME = "UserName";
            public const string USER_GENDER = "UserGender";
            public const string SELECTED_CHARACTER = "SelectedCharacter";
            public const string PERSONALITY_TRAITS = "PersonalityTraits";
            public const string SUBSCRIPTION_TIER = "SubscriptionTier";
            public const string AVATAR_IN_CHAT = "AvatarInChat";
            public const string BACKGROUND_MUSIC = "BackgroundMusic";
            public const string NOTIFICATIONS = "Notifications";
            public const string CREDITS = "Credits";
            public const string MESSAGE_LIMIT = "MessageLimit";
            public const string LAST_LOGIN = "LastLogin";
        }

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

        #region Player Data Operations

        public void LoadPlayerData()
        {
            if (!PlayFabManager.Instance.IsReady() || !PlayFabAuthService.Instance.IsLoggedIn)
            {
                OnDataError?.Invoke("Cannot load data: Not logged in");
                return;
            }

            var request = new GetUserDataRequest()
            {
                Keys = null
            };

            PlayFabClientAPI.GetUserData(request,
                result =>
                {
                    cachedPlayerData.Clear();
                    if (result.Data != null)
                    {
                        foreach (var kvp in result.Data)
                        {
                            cachedPlayerData[kvp.Key] = kvp.Value.Value;
                        }
                    }
                    LogDebug($"Loaded {cachedPlayerData.Count} player data entries");
                    OnDataLoaded?.Invoke(new Dictionary<string, string>(cachedPlayerData));
                },
                error =>
                {
                    string errorMsg = $"Failed to load player data: {error.ErrorMessage}";
                    LogError(errorMsg);
                    OnDataError?.Invoke(errorMsg);
                });
        }
        public void SavePlayerData(Dictionary<string, string> data)
        {
            if (!PlayFabManager.Instance.IsReady() || !PlayFabAuthService.Instance.IsLoggedIn)
            {
                OnDataError?.Invoke("Cannot save data: Not logged in");
                return;
            }

            var request = new UpdateUserDataRequest()
            {
                Data = data
            };

            PlayFabClientAPI.UpdateUserData(request,
                result =>
                {
                    foreach (var kvp in data)
                    {
                        cachedPlayerData[kvp.Key] = kvp.Value;
                    }
                    LogDebug($"Saved {data.Count} player data entries");
                    OnDataSaved?.Invoke();
                },
                error =>
                {
                    string errorMsg = $"Failed to save player data: {error.ErrorMessage}";
                    LogError(errorMsg);
                    OnDataError?.Invoke(errorMsg);
                });
        }

        public void SavePlayerDataValue(string key, string value)
        {
            SavePlayerData(new Dictionary<string, string> { { key, value } });
        }
        public void SavePlayerData()
        {
            if (cachedPlayerData.Count > 0)
            {
                SavePlayerData(new Dictionary<string, string>(cachedPlayerData));
            }
        }

        public string GetPlayerDataValue(string key, string defaultValue = "")
        {
            if (cachedPlayerData.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        public Dictionary<string, string> GetAllPlayerData()
        {
            return new Dictionary<string, string>(cachedPlayerData);
        }

        public string GetData(string key, string defaultValue = "")
        {
            return GetPlayerDataValue(key, defaultValue);
        }

        public void SetData(string key, string value)
        {
            SavePlayerDataValue(key, value);
        }

        #endregion

        #region Title Data Operations

        public void LoadTitleData(List<string> keys = null)
        {
            if (!PlayFabManager.Instance.IsReady())
            {
                OnDataError?.Invoke("Cannot load title data: PlayFab not initialized");
                return;
            }

            var request = new GetTitleDataRequest()
            {
                Keys = keys
            };

            PlayFabClientAPI.GetTitleData(request,
                result =>
                {
                    cachedTitleData.Clear();
                    if (result.Data != null)
                    {
                        foreach (var kvp in result.Data)
                        {
                            cachedTitleData[kvp.Key] = kvp.Value;
                        }
                    }
                    LogDebug($"Loaded {cachedTitleData.Count} title data entries");
                },
                error =>
                {
                    string errorMsg = $"Failed to load title data: {error.ErrorMessage}";
                    LogError(errorMsg);
                    OnDataError?.Invoke(errorMsg);
                });
        }

        public string GetTitleDataValue(string key, string defaultValue = "")
        {
            if (cachedTitleData.TryGetValue(key, out string value))
            {
                return value;
            }
            return defaultValue;
        }

        #endregion

        #region Convenience Methods

        public void SaveUserProfile(string userName, string gender, string selectedCharacter = "")
        {
            var data = new Dictionary<string, string>
            {
                { DataKeys.USER_NAME, userName },
                { DataKeys.USER_GENDER, gender },
                { DataKeys.SELECTED_CHARACTER, selectedCharacter }
            };
            SavePlayerData(data);
        }

        public void SaveUserSettings(bool avatarInChat, bool backgroundMusic, bool notifications)
        {
            var data = new Dictionary<string, string>
            {
                { DataKeys.AVATAR_IN_CHAT, avatarInChat.ToString() },
                { DataKeys.BACKGROUND_MUSIC, backgroundMusic.ToString() },
                { DataKeys.NOTIFICATIONS, notifications.ToString() }
            };
            SavePlayerData(data);
        }

        public void UpdateSubscriptionTier(string tier)
        {
            SavePlayerDataValue(DataKeys.SUBSCRIPTION_TIER, tier);
        }

        public string GetSubscriptionTier()
        {
            return GetPlayerDataValue(DataKeys.SUBSCRIPTION_TIER, "Free");
        }

        public void UpdateLastLogin()
        {
            SavePlayerDataValue(DataKeys.LAST_LOGIN, DateTime.UtcNow.ToString("o"));
        }

        #endregion

        #region Offline Fallback

        public void SaveLocalData(string key, string value)
        {
            PlayerPrefs.SetString($"LocalData_{key}", value);
            PlayerPrefs.Save();
        }

        public string LoadLocalData(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString($"LocalData_{key}", defaultValue);
        }

        public void SyncLocalDataToCloud()
        {
            Dictionary<string, string> localData = new Dictionary<string, string>();
            
            string[] commonKeys = new string[]
            {
                DataKeys.USER_NAME,
                DataKeys.USER_GENDER,
                DataKeys.SELECTED_CHARACTER,
                DataKeys.PERSONALITY_TRAITS,
                DataKeys.AVATAR_IN_CHAT,
                DataKeys.BACKGROUND_MUSIC,
                DataKeys.NOTIFICATIONS
            };

            foreach (string key in commonKeys)
            {
                string localValue = LoadLocalData(key, "");
                if (!string.IsNullOrEmpty(localValue))
                {
                    localData[key] = localValue;
                }
            }

            if (localData.Count > 0)
            {
                SavePlayerData(localData);
                LogDebug($"Synced {localData.Count} local data entries to cloud");
            }
        }

        #endregion

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayFabPlayerData] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[PlayFabPlayerData] {message}");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}