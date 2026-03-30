using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using PlayFab.ClientModels;

namespace Virtuery.PlayFab
{
    [ExecuteInEditMode]
    public class PlayFabSetupPrefab : MonoBehaviour
    {
        #region Configuration
        
        [Header("PlayFab Configuration")]
        [Tooltip("Your PlayFab Title ID from the PlayFab developer dashboard")]
        [SerializeField] private string titleId = "";
        
        [Tooltip("Automatically initialize PlayFab on Start")]
        [SerializeField] private bool autoInitialize = true;
        
        [Tooltip("Check for auto-login when the scene loads")]
        [SerializeField] private bool checkAutoLoginOnStart = true;
        
        [Tooltip("Enable debug logging for troubleshooting")]
        [SerializeField] private bool enableDebugLogs = true;
        
        #endregion
        
        #region Events
        
        [Header("Events")]
        [Tooltip("Called when a user successfully logs in or registers")]
        [SerializeField] private UnityEvent onAuthenticationSuccess;
        
        [Tooltip("Called when authentication fails. Parameter is the error message.")]
        [SerializeField] private UnityEvent<string> onAuthenticationFailed;
        
        [Tooltip("Called when the user logs out")]
        [SerializeField] private UnityEvent onLogout;
        
        [Tooltip("Called when player data is loaded from PlayFab")]
        [SerializeField] private UnityEvent onPlayerDataLoaded;
        
        #endregion
        
        #region Session Settings
        
        [Header("Session Settings")]
        [Tooltip("Automatically refresh session before it expires")]
        [SerializeField] private bool autoRefreshSession = true;
        
        [Tooltip("How often to check session validity (in seconds)")]
        [SerializeField] [Range(300f, 7200f)] private float sessionCheckInterval = 3600f;
        
        #endregion
        
        #region UI References (Optional)
        
        [Header("Optional UI References")]
        
        [Tooltip("Scene to load after successful authentication")]
        [SerializeField] private string mainSceneName = "MainScene";
        
        [Tooltip("Automatically load the main scene after authentication")]
        [SerializeField] private bool autoLoadMainScene = false;
        
        #endregion
        
        #region State Properties
        

        public bool IsInitialized => PlayFabManager.Instance != null && PlayFabManager.Instance.IsInitialized;
        
        public bool IsAuthenticated => PlayFabAuthService.Instance != null && PlayFabAuthService.Instance.IsLoggedIn;
        
        public string PlayFabId => PlayFabAuthService.Instance?.PlayFabId;
        
        public string SessionTicket => PlayFabAuthService.Instance?.SessionTicket;
        
        #endregion
        
        #region Private Fields
        
        private bool isSetupComplete = false;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (!Application.isPlaying)
                return;            
            SetupComponents();
        }
        
        private void Start()
        {
            if (!Application.isPlaying)
                return;
            
            SubscribeToEvents();

            if (autoInitialize)
            {
                InitializePlayFab();
            }
            
            if (checkAutoLoginOnStart && PlayFabAuthService.Instance != null && PlayFabAuthService.Instance.CanAutoLogin())
            {
                LogDebug("Attempting auto-login...");
                PlayFabAuthService.Instance.TryAutoLogin();
            }
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion
        
        #region Setup
        
        private void SetupComponents()
        {
            if (isSetupComplete)
                return;
            
            if (PlayFabManager.Instance == null)
            {
                GameObject managerObj = new GameObject("[PlayFabManager]");
                managerObj.transform.SetParent(transform);
                PlayFabManager manager = managerObj.AddComponent<PlayFabManager>();
                
                if (!string.IsNullOrEmpty(titleId))
                {
                    manager.SetTitleId(titleId);
                }
                
                LogDebug("PlayFabManager created");
            }
            
            if (PlayFabAuthService.Instance == null)
            {
                GameObject authObj = new GameObject("[PlayFabAuthService]");
                authObj.transform.SetParent(transform);
                authObj.AddComponent<PlayFabAuthService>();
                LogDebug("PlayFabAuthService created");
            }
            
            if (PlayFabPlayerData.Instance == null)
            {
                GameObject dataObj = new GameObject("[PlayFabPlayerData]");
                dataObj.transform.SetParent(transform);
                PlayFabPlayerData playerData = dataObj.AddComponent<PlayFabPlayerData>();
                
                playerData.OnDataLoaded += (data) => onPlayerDataLoaded?.Invoke();
                
                LogDebug("PlayFabPlayerData created");
            }
            
            isSetupComplete = true;
        }
        
        private void SubscribeToEvents()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess += HandleLoginSuccess;
                PlayFabAuthService.Instance.OnRegisterSuccess += HandleRegisterSuccess;
                PlayFabAuthService.Instance.OnLoginFailure += HandleLoginFailure;
                PlayFabAuthService.Instance.OnRegisterFailure += HandleRegisterFailure;
                PlayFabAuthService.Instance.OnLogout += HandleLogout;
            }
        }
        
        private void UnsubscribeFromEvents()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.OnLoginSuccess -= HandleLoginSuccess;
                PlayFabAuthService.Instance.OnRegisterSuccess -= HandleRegisterSuccess;
                PlayFabAuthService.Instance.OnLoginFailure -= HandleLoginFailure;
                PlayFabAuthService.Instance.OnRegisterFailure -= HandleRegisterFailure;
                PlayFabAuthService.Instance.OnLogout -= HandleLogout;
            }
        }
        
        #endregion
        
        #region Public API
        
        public void InitializePlayFab()
        {
            if (string.IsNullOrEmpty(titleId))
            {
                Debug.LogError("[PlayFabSetupPrefab] Title ID is not set! Please set it in the Inspector.");
                return;
            }
            
            if (PlayFabManager.Instance != null)
            {
                PlayFabManager.Instance.SetTitleId(titleId);
                LogDebug($"PlayFab initialized with Title ID: {titleId}");
            }
        }
        public void SetTitleId(string newTitleId)
        {
            titleId = newTitleId;
            if (PlayFabManager.Instance != null)
            {
                PlayFabManager.Instance.SetTitleId(titleId);
            }
        }
        public void LoginWithEmail(string email, string password, bool rememberMe = true)
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.LoginWithEmail(email, password, rememberMe);
            }
        }
        public void RegisterWithEmail(string email, string password, string displayName = null)
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.RegisterWithEmail(email, password, displayName);
            }
        }
        
        public void LoginWithGoogle()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.LoginWithGoogle(true);
            }
        }
        
        public void LoginWithApple()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.LoginWithApple(true);
            }
        }
        public void LoginAsGuest()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.LoginAsGuest();
            }
        }
        
        public void Logout()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.Logout();
            }
        }
        
        public bool CanAutoLogin()
        {
            return PlayFabAuthService.Instance != null && PlayFabAuthService.Instance.CanAutoLogin();
        }
        
        public void TryAutoLogin()
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.TryAutoLogin();
            }
        }
        public void RequestPasswordReset(string email)
        {
            if (PlayFabAuthService.Instance != null)
            {
                PlayFabAuthService.Instance.RequestPasswordReset(email);
            }
        }

        public string GetPlayerData(string key)
        {
            return PlayFabPlayerData.Instance?.GetData(key);
        }
        
        public void SetPlayerData(string key, string value)
        {
            if (PlayFabPlayerData.Instance != null)
            {
                PlayFabPlayerData.Instance.SetData(key, value);
            }
        }
        
        public void SavePlayerData()
        {
            if (PlayFabPlayerData.Instance != null)
            {
                PlayFabPlayerData.Instance.SavePlayerData();
            }
        }
        
        public void LoadPlayerData()
        {
            if (PlayFabPlayerData.Instance != null)
            {
                PlayFabPlayerData.Instance.LoadPlayerData();
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleLoginSuccess(LoginResult result)
        {
            LogDebug($"Login successful: {result.PlayFabId}");
            
            if (PlayFabPlayerData.Instance != null)
            {
                PlayFabPlayerData.Instance.LoadPlayerData();
            }
            
            onAuthenticationSuccess?.Invoke();
            
            if (autoLoadMainScene && !string.IsNullOrEmpty(mainSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainSceneName);
            }
        }
        
        private void HandleRegisterSuccess(RegisterPlayFabUserResult result)
        {
            LogDebug($"Registration successful: {result.PlayFabId}");
            
            onAuthenticationSuccess?.Invoke();
            
            if (autoLoadMainScene && !string.IsNullOrEmpty(mainSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainSceneName);
            }
        }
        
        private void HandleLoginFailure(string errorMessage)
        {
            LogDebug($"Login failed: {errorMessage}");
            onAuthenticationFailed?.Invoke(errorMessage);
        }
        
        private void HandleRegisterFailure(string errorMessage)
        {
            LogDebug($"Registration failed: {errorMessage}");
            onAuthenticationFailed?.Invoke(errorMessage);
        }
        
        private void HandleLogout()
        {
            LogDebug("User logged out");
            onLogout?.Invoke();
        }
        
        #endregion
        
        #region Debug
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayFabSetupPrefab] {message}");
            }
        }
        
        #endregion
        
        #region Editor Support
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(titleId) && titleId.Length > 0)
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(titleId, @"^[A-Za-z0-9]+$"))
                {
                    Debug.LogWarning("[PlayFabSetupPrefab] Title ID should be alphanumeric only.");
                }
            }
        }
        
        [ContextMenu("Create All Components")]
        private void CreateAllComponents()
        {
            SetupComponents();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        [ContextMenu("Open PlayFab Dashboard")]
        private void OpenPlayFabDashboard()
        {
            Application.OpenURL("https://developer.playfab.com/");
        }
        
        [ContextMenu("Open Integration Guide")]
        private void OpenIntegrationGuide()
        {
            string guidePath = System.IO.Path.Combine(Application.dataPath, "Scripts/PlayFab/INTEGRATION_GUIDE.md");
            if (System.IO.File.Exists(guidePath))
            {
                UnityEditor.EditorUtility.OpenWithDefaultApp(guidePath);
            }
            else
            {
                Debug.LogWarning($"Integration guide not found at: {guidePath}");
            }
        }
        
        [ContextMenu("Reset All Settings")]
        private void ResetSettings()
        {
            titleId = "";
            autoInitialize = true;
            checkAutoLoginOnStart = true;
            enableDebugLogs = true;
            autoRefreshSession = true;
            sessionCheckInterval = 3600f;
            mainSceneName = "MainScene";
            autoLoadMainScene = false;
            
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[PlayFabSetupPrefab] Settings reset to defaults");
        }
#endif
        
        #endregion
    }
}