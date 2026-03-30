using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Virtuery.PlayFab;

public class AuthManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameTown";

    [Header("Login Fields")]
    [SerializeField] private TMP_InputField loginEmailInputField;
    [SerializeField] private TMP_InputField loginPasswordInputField;

    [Header("Register Fields")]
    [SerializeField] private TMP_InputField registerDisplayNameInputField;
    [SerializeField] private TMP_InputField registerEmailInputField;
    [SerializeField] private TMP_InputField registerPasswordInputField;

    private PlayFabAuthService AuthService => PlayFabAuthService.Instance;
    private bool isAttemptingAutoLogin = false;

    private void Start()
    {
        if (AuthService != null)
        {
            AuthService.OnLoginSuccess += OnLoginSuccess;
            AuthService.OnLoginFailure += OnLoginFailure;
            AuthService.OnRegisterSuccess += OnRegisterSuccess;
            AuthService.OnRegisterFailure += OnRegisterFailure;
            AuthService.OnPasswordResetSuccess += OnPasswordResetSuccess;
            AuthService.OnPasswordResetFailure += OnPasswordResetFailure;

            if (AuthService.CanAutoLoginWithoutPassword())
            {
                isAttemptingAutoLogin = true;
                AuthService.TryAutoLogin();
                return;
            }
        }

        if (AuthService != null && AuthService.CanAutoLogin())
        {
            string savedEmail = AuthService.GetSavedEmail();
            if (!string.IsNullOrEmpty(savedEmail) && loginEmailInputField != null)
            {
                loginEmailInputField.text = savedEmail;
            }
        }
    }

    private void OnDestroy()
    {
        if (AuthService != null)
        {
            AuthService.OnLoginSuccess -= OnLoginSuccess;
            AuthService.OnLoginFailure -= OnLoginFailure;
            AuthService.OnRegisterSuccess -= OnRegisterSuccess;
            AuthService.OnRegisterFailure -= OnRegisterFailure;
            AuthService.OnPasswordResetSuccess -= OnPasswordResetSuccess;
            AuthService.OnPasswordResetFailure -= OnPasswordResetFailure;
        }
    }

    public void LoginWithEmail()
    {
        if (AuthService == null)
        {
            Debug.LogError("[AuthManager] PlayFabAuthService not available");
            return;
        }

        string email = loginEmailInputField?.text ?? "";
        string password = loginPasswordInputField?.text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[AuthManager] Email and password are required");
            return;
        }

        AuthService.LoginWithEmail(email, password, false);
    }

    public void RegisterNewUser()
    {
        if (AuthService == null)
        {
            Debug.LogError("[AuthManager] PlayFabAuthService not available");
            return;
        }

        string displayName = registerDisplayNameInputField?.text ?? "";
        string email = registerEmailInputField?.text ?? "";
        string password = registerPasswordInputField?.text ?? "";

        if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("[AuthManager] All fields are required");
            return;
        }

        AuthService.RegisterWithEmail(email, password, displayName);
    }

    public void RequestPasswordReset()
    {
        if (AuthService == null)
        {
            Debug.LogError("[AuthManager] PlayFabAuthService not available");
            return;
        }

        string email = loginEmailInputField?.text ?? "";

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("[AuthManager] Email is required for password reset");
            return;
        }

        AuthService.RequestPasswordReset(email);
    }

    public void LoginAsGuest()
    {
        if (AuthService == null)
        {
            Debug.LogError("[AuthManager] PlayFabAuthService not available");
            return;
        }
        AuthService.LoginAsGuest();
    }

    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        Debug.Log($"[AuthManager] Login successful! PlayFabId: {result.PlayFabId}");
        LoadGameScene();
    }

    private void OnLoginFailure(string error)
    {
        isAttemptingAutoLogin = false;
        Debug.LogError($"[AuthManager] Login failed: {error}");
    }

    private void OnRegisterSuccess(PlayFab.ClientModels.RegisterPlayFabUserResult result)
    {
        Debug.Log($"[AuthManager] Registration successful! PlayFabId: {result.PlayFabId}");
        LoadGameScene();
    }

    private void OnRegisterFailure(string error)
    {
        Debug.LogError($"[AuthManager] Registration failed: {error}");
    }

    private void OnPasswordResetSuccess()
    {
        Debug.Log("[AuthManager] Password reset email sent successfully");
    }

    private void OnPasswordResetFailure(string error)
    {
        Debug.LogError($"[AuthManager] Password reset failed: {error}");
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }
    }
}
