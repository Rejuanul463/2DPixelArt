using UnityEngine;
using PlayFab;

namespace Virtuery.PlayFab
{
    public class PlayFabManager : MonoBehaviour
    {
        public static PlayFabManager Instance { get; private set; }

        [Header("PlayFab Configuration")]
        [SerializeField] private string playFabTitleId = "YOUR_TITLE_ID_HERE";
        
        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;


        public bool IsInitialized { get; private set; }
        
        public bool IsSessionActive { get; private set; }

        public event System.Action OnInitialized;
        
        public event System.Action<bool> OnSessionStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);        
            EnsureAuthServiceExists();           
            InitializePlayFab();
        }

        private void EnsureAuthServiceExists()
        {
            if (PlayFabAuthService.Instance == null)
            {
                GameObject authObj = new GameObject("PlayFabAuthService");
                authObj.AddComponent<PlayFabAuthService>();
                LogDebug("PlayFabAuthService created automatically");
            }
            
            if (PlayFabPlayerData.Instance == null)
            {
                GameObject dataObj = new GameObject("PlayFabPlayerData");
                dataObj.AddComponent<PlayFabPlayerData>();
                LogDebug("PlayFabPlayerData created automatically");
            }

            if (PlayerDataSyncManager.Instance == null)
            {
                GameObject syncObj = new GameObject("PlayerDataSyncManager");
                syncObj.AddComponent<PlayerDataSyncManager>();
                LogDebug("PlayerDataSyncManager created automatically");
            }

        if (TitleDataManager.Instance == null)
        {
            GameObject titleDataObj = new GameObject("TitleDataManager");
            titleDataObj.AddComponent<TitleDataManager>();
            LogDebug("TitleDataManager created automatically");
        }
    }

        private void InitializePlayFab()
        {
            if (string.IsNullOrEmpty(playFabTitleId) || playFabTitleId == "YOUR_TITLE_ID_HERE")
            {
                LogError("PlayFab Title ID is not configured! Please set it in the PlayFabManager inspector.");
                return;
            }

            PlayFabSettings.staticSettings.TitleId = playFabTitleId;
            IsInitialized = true;
            
            LogDebug($"PlayFab initialized with Title ID: {playFabTitleId}");
            OnInitialized?.Invoke();
        }
        public void SetTitleId(string titleId)
        {
            if (string.IsNullOrEmpty(titleId))
            {
                LogError("Cannot set empty Title ID");
                return;
            }

            playFabTitleId = titleId;
            PlayFabSettings.staticSettings.TitleId = playFabTitleId;
            IsInitialized = true;
            
            LogDebug($"PlayFab Title ID updated to: {playFabTitleId}");
        }

        public string GetTitleId()
        {
            return playFabTitleId;
        }

        public void SetSessionActive(bool isActive)
        {
            if (IsSessionActive != isActive)
            {
                IsSessionActive = isActive;
                OnSessionStateChanged?.Invoke(isActive);
                LogDebug($"Session state changed to: {(isActive ? "Active" : "Inactive")}");
            }
        }


        public void ClearSession()
        {
            SetSessionActive(false);
            LogDebug("Session cleared");
        }

        public bool IsReady()
        {
            if (!IsInitialized)
            {
                LogError("PlayFab is not initialized. Call InitializePlayFab() first.");
                return false;
            }
            return true;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayFabManager] {message}");
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[PlayFabManager] {message}");
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
