using UnityEngine;
using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

namespace Virtuery.PlayFab
{
    public class TitleDataManager : MonoBehaviour
    {
        public static TitleDataManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private float refreshInterval = 3600f;

        public event Action OnTitleDataLoaded;
        public event Action<string> OnTitleDataError;

        private Dictionary<string, string> cachedTitleData = new Dictionary<string, string>();
        private Dictionary<string, CharacterDefinition> characterDefinitions = new Dictionary<string, CharacterDefinition>();
        private Dictionary<string, PersonalityTemplate> personalityTemplates = new Dictionary<string, PersonalityTemplate>();
        private List<FeedPost> feedPosts = new List<FeedPost>();
        private AppConfig appConfig;
        private bool isLoading = false;

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
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess += OnLoginSuccess;
            }

            // Only load on start if user is already logged in
            if (autoLoadOnStart && PlayFabAuthService.Instance != null && PlayFabAuthService.Instance.IsLoggedIn)
            {
                LoadAllTitleData();
            }
        }

        private void OnDestroy()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess -= OnLoginSuccess;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnLoginSuccess(LoginResult result)
        {
            LoadAllTitleData();
        }

        public void LoadAllTitleData()
        {
            if (isLoading) return;

            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsReady())
            {
                OnTitleDataError?.Invoke("Cannot load title data: PlayFab not initialized");
                return;
            }

            isLoading = true;
            LogDebug("Loading all title data...");

            var request = new GetTitleDataRequest();

            PlayFabClientAPI.GetTitleData(request,
                result =>
                {
                    isLoading = false;
                    ProcessTitleData(result.Data);
                    OnTitleDataLoaded?.Invoke();
                    LogDebug($"Loaded {cachedTitleData.Count} title data entries");
                },
                error =>
                {
                    isLoading = false;
                    string errorMsg = $"Failed to load title data: {error.ErrorMessage}";
                    LogError(errorMsg);
                    OnTitleDataError?.Invoke(errorMsg);
                });
        }

        public void LoadSpecificTitleData(List<string> keys)
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsReady())
            {
                OnTitleDataError?.Invoke("Cannot load title data: PlayFab not initialized");
                return;
            }

            var request = new GetTitleDataRequest
            {
                Keys = keys
            };

            PlayFabClientAPI.GetTitleData(request,
                result =>
                {
                    foreach (var kvp in result.Data)
                    {
                        cachedTitleData[kvp.Key] = kvp.Value;
                    }
                    ProcessTitleData(result.Data);
                    OnTitleDataLoaded?.Invoke();
                    LogDebug($"Loaded {result.Data.Count} specific title data entries");
                },
                error =>
                {
                    string errorMsg = $"Failed to load title data: {error.ErrorMessage}";
                    LogError(errorMsg);
                    OnTitleDataError?.Invoke(errorMsg);
                });
        }

        private void ProcessTitleData(Dictionary<string, string> data)
        {
            if (data == null) return;

            foreach (var kvp in data)
            {
                cachedTitleData[kvp.Key] = kvp.Value;
            }

            if (data.TryGetValue(TitleDataKeys.CHARACTER_DEFINITIONS, out string charDefsJson))
            {
                ParseCharacterDefinitions(charDefsJson);
            }

            if (data.TryGetValue(TitleDataKeys.PERSONALITY_TEMPLATES, out string personalityJson))
            {
                ParsePersonalityTemplates(personalityJson);
            }

            if (data.TryGetValue(TitleDataKeys.FEED_POSTS, out string feedPostsJson))
            {
                ParseFeedPosts(feedPostsJson);
            }

            if (data.TryGetValue(TitleDataKeys.APP_CONFIG, out string appConfigJson))
            {
                ParseAppConfig(appConfigJson);
            }
        }

        private void ParseCharacterDefinitions(string json)
        {
            try
            {
                var definitions = JsonConvert.DeserializeObject<List<CharacterDefinition>>(json);
                characterDefinitions.Clear();
                if (definitions != null)
                {
                    foreach (var def in definitions)
                    {
                        characterDefinitions[def.id] = def;
                    }
                }
                LogDebug($"Parsed {characterDefinitions.Count} character definitions");
            }
            catch (Exception e)
            {
                LogError($"Failed to parse character definitions: {e.Message}");
            }
        }

        private void ParsePersonalityTemplates(string json)
        {
            try
            {
                var templates = JsonConvert.DeserializeObject<List<PersonalityTemplate>>(json);
                personalityTemplates.Clear();
                if (templates != null)
                {
                    foreach (var template in templates)
                    {
                        personalityTemplates[template.id] = template;
                    }
                }
                LogDebug($"Parsed {personalityTemplates.Count} personality templates");
            }
            catch (Exception e)
            {
                LogError($"Failed to parse personality templates: {e.Message}");
            }
        }

        private void ParseFeedPosts(string json)
        {
            try
            {
                var posts = JsonConvert.DeserializeObject<List<FeedPost>>(json);
                feedPosts = posts ?? new List<FeedPost>();
                LogDebug($"Parsed {feedPosts.Count} feed posts");
            }
            catch (Exception e)
            {
                LogError($"Failed to parse feed posts: {e.Message}");
            }
        }

        private void ParseAppConfig(string json)
        {
            try
            {
                appConfig = JsonConvert.DeserializeObject<AppConfig>(json);
                LogDebug("Parsed app config");
            }
            catch (Exception e)
            {
                LogError($"Failed to parse app config: {e.Message}");
            }
        }

        #region Public Getters

        public string GetTitleDataValue(string key, string defaultValue = "")
        {
            return cachedTitleData.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public Dictionary<string, CharacterDefinition> GetCharacterDefinitions()
        {
            return new Dictionary<string, CharacterDefinition>(characterDefinitions);
        }

        public CharacterDefinition GetCharacterDefinition(string characterId)
        {
            return characterDefinitions.TryGetValue(characterId, out CharacterDefinition def) ? def : null;
        }

        public Dictionary<string, PersonalityTemplate> GetPersonalityTemplates()
        {
            return new Dictionary<string, PersonalityTemplate>(personalityTemplates);
        }

        public PersonalityTemplate GetPersonalityTemplate(string templateId)
        {
            return personalityTemplates.TryGetValue(templateId, out PersonalityTemplate template) ? template : null;
        }

        public List<FeedPost> GetFeedPosts()
        {
            return new List<FeedPost>(feedPosts);
        }

        public AppConfig GetAppConfig()
        {
            return appConfig;
        }

        public bool IsMaintenanceMode()
        {
            if (cachedTitleData.TryGetValue(TitleDataKeys.MAINTENANCE_MODE, out string value))
            {
                return value.ToLower() == "true";
            }
            return false;
        }

        public string GetMinAppVersion()
        {
            return GetTitleDataValue(TitleDataKeys.MIN_APP_VERSION, "1.0.0");
        }

        public bool IsFeatureEnabled(string featureName)
        {
            if (cachedTitleData.TryGetValue(TitleDataKeys.FEATURE_FLAGS, out string flagsJson))
            {
                try
                {
                    var flags = JsonConvert.DeserializeObject<Dictionary<string, bool>>(flagsJson);
                    if (flags != null && flags.TryGetValue(featureName, out bool enabled))
                    {
                        return enabled;
                    }
                }
                catch { }
            }
            return false;
        }

        #endregion

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[TitleDataManager] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[TitleDataManager] {message}");
        }
    }

    #region Data Models

    [Serializable]
    public class CharacterDefinition
    {
        public string id;
        public string name;
        public string description;
        public string personality;
        public string backstory;
        public string avatarUrl;
        public string modelPrefab;
        public List<string> availablePersonalities;
        public Dictionary<string, string> traits;
    }

    [Serializable]
    public class PersonalityTemplate
    {
        public string id;
        public string name;
        public string description;
        public string systemPrompt;
        public float temperature;
        public int maxTokens;
        public Dictionary<string, string> traits;
    }

    [Serializable]
    public class FeedPost
    {
        public string id;
        public string characterId;
        public string title;
        public string content;
        public string imageUrl;
        public string timestamp;
        public int likes;
        public int comments;
    }

    [Serializable]
    public class AppConfig
    {
        public string apiEndpoint;
        public int maxMessageLength;
        public int dailyFreeMessages;
        public bool voiceChatEnabled;
        public bool videoEnabled;
        public Dictionary<string, int> tierMessageLimits;
    }

    #endregion
}
