using UnityEngine;
using System;
using PlayFab;
using PlayFab.ClientModels;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

#if UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.SocialPlatforms;
#endif

namespace Virtuery.PlayFab
{
    public class PlayFabAuthService : MonoBehaviour
    {
        public static PlayFabAuthService Instance { get; private set; }

        [Header("Debug Settings")]
        [SerializeField] private bool enableDebugLogs = true;

        public event Action<LoginResult> OnLoginSuccess;
        public event Action<RegisterPlayFabUserResult> OnRegisterSuccess;
        public event Action<string> OnLoginFailure;
        public event Action<string> OnRegisterFailure;
        public event Action OnLogout;
        public event Action OnPasswordResetSuccess;
        public event Action<string> OnPasswordResetFailure;

        public string PlayFabId { get; private set; }
        public string SessionTicket { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(PlayFabId);

        private const string PLAYER_PREFS_EMAIL = "PlayFab_Email";
        private const string PLAYER_PREFS_CUSTOM_ID = "PlayFab_CustomId";
        private const string PLAYER_PREFS_REMEMBER_ME = "PlayFab_RememberMe";
        private const string PLAYER_PREFS_AUTH_TYPE = "PlayFab_AuthType";

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

        #region Email/Password Authentication
        public void RegisterWithEmail(string email, string password, string displayName)
        {
            var request = new RegisterPlayFabUserRequest
            {
                Email = email,
                Password = password,
                DisplayName = displayName,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(request, 
                result => 
                {
                    PlayFabId = result.PlayFabId;
                    SessionTicket = result.SessionTicket;
                    DisplayName = displayName;
                    LogDebug($"Registration successful: {result.PlayFabId}");
                    OnRegisterSuccess?.Invoke(result);
                },
                error => 
                {
                    string errorMsg = ParseError(error);
                    LogDebug($"Registration failed: {errorMsg}");
                    OnRegisterFailure?.Invoke(errorMsg);
                }
            );
        }

        public void LoginWithEmail(string email, string password, bool rememberMe = false)
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserData = true,
                    GetUserAccountInfo = true
                }
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, 
                result => HandleLoginSuccess(result, rememberMe ? "email" : null, email, password),
                error => HandleLoginFailure(error)
            );
        }

        #endregion

        #region Google Sign-In

        public void LoginWithGoogle(bool rememberMe = false)
        {
#if UNITY_ANDROID
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
                .AddOauthScope("profile")
                .RequestServerAuthCode(false)
                .Build();

            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();

            Social.localUser.Authenticate(success =>
            {
                if (success)
                {
                    string serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                    var request = new LoginWithGoogleAccountRequest
                    {
                        ServerAuthCode = serverAuthCode,
                        CreateAccount = true,
                        InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                        {
                            GetPlayerProfile = true,
                            GetUserData = true
                        }
                    };

                    PlayFabClientAPI.LoginWithGoogleAccount(request,
                        result => HandleLoginSuccess(result, rememberMe ? "google" : null, null, null),
                        error => HandleLoginFailure(error)
                    );
                }
                else
                {
                    OnLoginFailure?.Invoke("Google Sign-In failed. Please try again.");
                }
            });
#else
            OnLoginFailure?.Invoke("Google Sign-In is only supported on Android.");
#endif
        }

        #endregion

        #region Apple Sign-In

        public void LoginWithApple(bool rememberMe = false)
        {
#if UNITY_IOS
            // Apple Sign-In requires the Unity Apple Sign-In package or native implementation
            // This is a placeholder for the actual implementation
            LogDebug("Apple Sign-In initiated...");            
            OnLoginFailure?.Invoke("Apple Sign-In requires additional setup. Please refer to the documentation.");
#else
            OnLoginFailure?.Invoke("Apple Sign-In is only supported on iOS.");
#endif
        }

        #endregion

        #region Guest Login

        public void LoginAsGuest()
        {
            string customId = PlayerPrefs.GetString(PLAYER_PREFS_CUSTOM_ID, "");
            if (string.IsNullOrEmpty(customId))
            {
                customId = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString(PLAYER_PREFS_CUSTOM_ID, customId);
                PlayerPrefs.Save();
            }

            var request = new LoginWithCustomIDRequest
            {
                CustomId = customId,
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true,
                    GetUserData = true
                }
            };

            PlayFabClientAPI.LoginWithCustomID(request,
                result => HandleLoginSuccess(result, "guest", null, null),
                error => HandleLoginFailure(error)
            );
        }

        #endregion

        #region Auto-Login

        public bool CanAutoLogin()
        {
            string authType = PlayerPrefs.GetString(PLAYER_PREFS_AUTH_TYPE, "");
            bool rememberMe = PlayerPrefs.GetInt(PLAYER_PREFS_REMEMBER_ME, 0) == 1;
            return rememberMe && !string.IsNullOrEmpty(authType);
        }

        public bool CanAutoLoginWithoutPassword()
        {
            string authType = PlayerPrefs.GetString(PLAYER_PREFS_AUTH_TYPE, "");
            bool rememberMe = PlayerPrefs.GetInt(PLAYER_PREFS_REMEMBER_ME, 0) == 1;
            return rememberMe && (authType == "google" || authType == "apple" || authType == "guest");
        }

        public string GetSavedEmail()
        {
            return PlayerPrefs.GetString(PLAYER_PREFS_EMAIL, "");
        }

        public string GetSavedAuthType()
        {
            return PlayerPrefs.GetString(PLAYER_PREFS_AUTH_TYPE, "");
        }

        public void TryAutoLogin()
        {
            if (!CanAutoLogin())
            {
                OnLoginFailure?.Invoke("No saved credentials found for auto-login.");
                return;
            }

            string authType = PlayerPrefs.GetString(PLAYER_PREFS_AUTH_TYPE, "");

            switch (authType)
            {
                case "email":
                    string email = PlayerPrefs.GetString(PLAYER_PREFS_EMAIL, "");
                    OnLoginFailure?.Invoke($"Please enter your password for {email}");
                    break;
                case "google":
                    LoginWithGoogle(true);
                    break;
                case "apple":
                    LoginWithApple(true);
                    break;
                case "guest":
                    LoginAsGuest();
                    break;
                default:
                    OnLoginFailure?.Invoke("Unknown authentication type for auto-login.");
                    break;
            }
        }

        #endregion

        #region Account Management

        public void LinkEmailPassword(string email, string password)
        {
            var request = new AddUsernamePasswordRequest
            {
                Email = email,
                Password = password
            };

            PlayFabClientAPI.AddUsernamePassword(request,
                result =>
                {
                    LogDebug("Email/password linked successfully.");
                    PlayerPrefs.SetString(PLAYER_PREFS_EMAIL, email);
                    PlayerPrefs.SetString(PLAYER_PREFS_AUTH_TYPE, "email");
                    PlayerPrefs.Save();
                },
                error =>
                {
                    string errorMsg = ParseError(error);
                    LogDebug($"Failed to link email/password: {errorMsg}");
                }
            );
        }

        public void RequestPasswordReset(string email)
        {
            // Get the TitleId - SendAccountRecoveryEmail requires it explicitly
            string titleId = PlayFabSettings.staticSettings.TitleId;
            
            if (string.IsNullOrEmpty(titleId) && PlayFabManager.Instance != null && PlayFabManager.Instance.IsInitialized)
            {
                titleId = PlayFabManager.Instance.GetTitleId();
            }
            
            if (string.IsNullOrEmpty(titleId))
            {
                OnPasswordResetFailure?.Invoke("PlayFab is not properly configured. Please restart the app.");
                return;
            }
            
            // IMPORTANT: SendAccountRecoveryEmailRequest requires TitleId to be set explicitly
            // Unlike login methods, this API doesn't auto-populate TitleId from settings
            var request = new SendAccountRecoveryEmailRequest
            {
                Email = email,
                TitleId = titleId
            };

            PlayFabClientAPI.SendAccountRecoveryEmail(request,
                result =>
                {
                    LogDebug("Password reset email sent successfully.");
                    OnPasswordResetSuccess?.Invoke();
                },
                error =>
                {
                    string errorMsg = ParseError(error);
                    LogDebug($"Password reset failed: {errorMsg}");
                    OnPasswordResetFailure?.Invoke(errorMsg);
                }
            );
        }


        public void Logout()
        {
            PlayFabId = null;
            SessionTicket = null;
            DisplayName = null;
            OnLogout?.Invoke();
            LogDebug("User logged out.");
        }

        public void LogoutAndClearCredentials()
        {
            PlayerPrefs.DeleteKey(PLAYER_PREFS_EMAIL);
            PlayerPrefs.DeleteKey(PLAYER_PREFS_REMEMBER_ME);
            PlayerPrefs.DeleteKey(PLAYER_PREFS_AUTH_TYPE);
            PlayerPrefs.Save();
            Logout();
        }

        #endregion

        #region Account Info

        /// <summary>
        /// Fetch the player's account info including display name from PlayFab.
        /// Call this after login if display name wasn't returned in the login response.
        /// </summary>
        public void FetchAccountInfo()
        {
            if (!IsLoggedIn) return;

            var request = new GetAccountInfoRequest();

            PlayFabClientAPI.GetAccountInfo(request,
                result =>
                {
                    if (result.AccountInfo != null)
                    {
                        if (!string.IsNullOrEmpty(result.AccountInfo.TitleInfo?.DisplayName))
                        {
                            DisplayName = result.AccountInfo.TitleInfo.DisplayName;
                            LogDebug($"Display name fetched: {DisplayName}");
                        }
                        
                        // Also try to get username
                        if (string.IsNullOrEmpty(DisplayName) && !string.IsNullOrEmpty(result.AccountInfo.Username))
                        {
                            DisplayName = result.AccountInfo.Username;
                            LogDebug($"Username fetched: {DisplayName}");
                        }
                    }
                },
                error => LogDebug($"Failed to fetch account info: {error.ErrorMessage}")
            );
        }

        #endregion

        #region Private Helpers

        private void HandleLoginSuccess(LoginResult result, string authType, string email, string password)
        {
            PlayFabId = result.PlayFabId;
            SessionTicket = result.SessionTicket;

            // Get display name from InfoResultPayload if available
            if (result.InfoResultPayload != null && 
                result.InfoResultPayload.PlayerProfile != null && 
                !string.IsNullOrEmpty(result.InfoResultPayload.PlayerProfile.DisplayName))
            {
                DisplayName = result.InfoResultPayload.PlayerProfile.DisplayName;
            }
            else
            {
                // Fetch account info to get display name
                FetchAccountInfo();
            }

            if (!string.IsNullOrEmpty(authType))
            {
                PlayerPrefs.SetString(PLAYER_PREFS_AUTH_TYPE, authType);
                PlayerPrefs.SetInt(PLAYER_PREFS_REMEMBER_ME, 1);
                if (!string.IsNullOrEmpty(email))
                    PlayerPrefs.SetString(PLAYER_PREFS_EMAIL, email);
                PlayerPrefs.Save();
            }

            LogDebug($"Login successful: {result.PlayFabId}");
            OnLoginSuccess?.Invoke(result);
        }

        private void HandleLoginFailure(PlayFabError error)
        {
            string errorMsg = ParseError(error);
            LogDebug($"Login failed: {errorMsg}");
            OnLoginFailure?.Invoke(errorMsg);
        }

        private string ParseError(PlayFabError error)
        {
            switch (error.Error)
            {
                case PlayFabErrorCode.AccountNotFound:
                    return "Account not found. Please check your email or sign up.";
                case PlayFabErrorCode.InvalidEmailOrPassword:
                    return "Invalid email or password. Please try again.";
                case PlayFabErrorCode.EmailAddressNotAvailable:
                    return "This email is already registered. Please login instead.";
                case PlayFabErrorCode.InvalidUsernameOrPassword:
                    return "Invalid username or password. Please try again.";
                case PlayFabErrorCode.ConnectionError:
                    return "Connection error. Please check your internet connection.";
                case PlayFabErrorCode.NotAuthenticated:
                    return "Not authenticated. Please login again.";
                default:
                    return error.ErrorMessage ?? "An unknown error occurred.";
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[PlayFabAuthService] {message}");
            }
        }

        #endregion
    }
}