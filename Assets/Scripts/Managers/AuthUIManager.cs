using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Virtuery.PlayFab;

public class AuthUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private GameObject passwordResetPanel;

    [Header("Login UI Elements")]
    [SerializeField] private GameObject loginLoadingIndicator;
    [SerializeField] private TMP_Text loginErrorText;
    [SerializeField] private TMP_Text loginSuccessText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button goToRegisterButton;
    [SerializeField] private Button forgotPasswordButton;

    [Header("Register UI Elements")]
    [SerializeField] private GameObject registerLoadingIndicator;
    [SerializeField] private TMP_Text registerErrorText;
    [SerializeField] private TMP_Text registerSuccessText;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button goToLoginButton;

    [Header("Password Reset UI Elements")]
    [SerializeField] private GameObject passwordResetLoadingIndicator;
    [SerializeField] private TMP_Text passwordResetErrorText;
    [SerializeField] private TMP_Text passwordResetSuccessText;
    [SerializeField] private Button submitPasswordResetButton;
    [SerializeField] private Button backToLoginButton;

    private PlayFabAuthService AuthService => PlayFabAuthService.Instance;

    private void Start()
    {
        ShowLogin();

        if (loginLoadingIndicator != null) loginLoadingIndicator.SetActive(false);
        if (registerLoadingIndicator != null) registerLoadingIndicator.SetActive(false);
        if (passwordResetLoadingIndicator != null) passwordResetLoadingIndicator.SetActive(false);

        ClearAllMessages();

        if (AuthService != null)
        {
            AuthService.OnLoginSuccess += OnLoginSuccess;
            AuthService.OnLoginFailure += OnLoginFailure;
            AuthService.OnRegisterSuccess += OnRegisterSuccess;
            AuthService.OnRegisterFailure += OnRegisterFailure;
            AuthService.OnPasswordResetSuccess += OnPasswordResetSuccess;
            AuthService.OnPasswordResetFailure += OnPasswordResetFailure;
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

    public void ShowLogin()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (passwordResetPanel != null) passwordResetPanel.SetActive(false);
        ClearAllMessages();
    }

    public void ShowRegister()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
        if (passwordResetPanel != null) passwordResetPanel.SetActive(false);
        ClearAllMessages();
    }

    public void ShowPasswordReset()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(false);
        if (passwordResetPanel != null) passwordResetPanel.SetActive(true);
        ClearAllMessages();
    }

    public void SetLoginLoading(bool isLoading)
    {
        if (loginLoadingIndicator != null) loginLoadingIndicator.SetActive(isLoading);
        if (loginButton != null) loginButton.interactable = !isLoading;
        if (goToRegisterButton != null) goToRegisterButton.interactable = !isLoading;
        if (forgotPasswordButton != null) forgotPasswordButton.interactable = !isLoading;
    }

    public void SetRegisterLoading(bool isLoading)
    {
        if (registerLoadingIndicator != null) registerLoadingIndicator.SetActive(isLoading);
        if (registerButton != null) registerButton.interactable = !isLoading;
        if (goToLoginButton != null) goToLoginButton.interactable = !isLoading;
    }

    public void SetPasswordResetLoading(bool isLoading)
    {
        if (passwordResetLoadingIndicator != null) passwordResetLoadingIndicator.SetActive(isLoading);
        if (submitPasswordResetButton != null) submitPasswordResetButton.interactable = !isLoading;
        if (backToLoginButton != null) backToLoginButton.interactable = !isLoading;
    }

    public void ShowLoginError(string message)
    {
        if (loginErrorText != null)
        {
            loginErrorText.text = message;
            loginErrorText.gameObject.SetActive(true);
        }
        if (loginSuccessText != null) loginSuccessText.gameObject.SetActive(false);
    }

    public void ShowLoginSuccess(string message)
    {
        if (loginSuccessText != null)
        {
            loginSuccessText.text = message;
            loginSuccessText.gameObject.SetActive(true);
        }
        if (loginErrorText != null) loginErrorText.gameObject.SetActive(false);
    }

    public void ShowRegisterError(string message)
    {
        if (registerErrorText != null)
        {
            registerErrorText.text = message;
            registerErrorText.gameObject.SetActive(true);
        }
        if (registerSuccessText != null) registerSuccessText.gameObject.SetActive(false);
    }

    public void ShowRegisterSuccess(string message)
    {
        if (registerSuccessText != null)
        {
            registerSuccessText.text = message;
            registerSuccessText.gameObject.SetActive(true);
        }
        if (registerErrorText != null) registerErrorText.gameObject.SetActive(false);
    }

    public void ShowPasswordResetError(string message)
    {
        if (passwordResetErrorText != null)
        {
            passwordResetErrorText.text = message;
            passwordResetErrorText.gameObject.SetActive(true);
        }
        if (passwordResetSuccessText != null) passwordResetSuccessText.gameObject.SetActive(false);
    }

    public void ShowPasswordResetSuccess(string message)
    {
        if (passwordResetSuccessText != null)
        {
            passwordResetSuccessText.text = message;
            passwordResetSuccessText.gameObject.SetActive(true);
        }
        if (passwordResetErrorText != null) passwordResetErrorText.gameObject.SetActive(false);
    }

    public void ClearAllMessages()
    {
        if (loginErrorText != null) loginErrorText.gameObject.SetActive(false);
        if (loginSuccessText != null) loginSuccessText.gameObject.SetActive(false);
        if (registerErrorText != null) registerErrorText.gameObject.SetActive(false);
        if (registerSuccessText != null) registerSuccessText.gameObject.SetActive(false);
        if (passwordResetErrorText != null) passwordResetErrorText.gameObject.SetActive(false);
        if (passwordResetSuccessText != null) passwordResetSuccessText.gameObject.SetActive(false);
    }

    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        SetLoginLoading(false);
        ShowLoginSuccess("Login successful!");
    }

    private void OnLoginFailure(string error)
    {
        SetLoginLoading(false);
        ShowLoginError(error);
    }

    private void OnRegisterSuccess(PlayFab.ClientModels.RegisterPlayFabUserResult result)
    {
        SetRegisterLoading(false);
        ShowRegisterSuccess("Registration successful!");
    }

    private void OnRegisterFailure(string error)
    {
        SetRegisterLoading(false);
        ShowRegisterError(error);
    }

    private void OnPasswordResetSuccess()
    {
        SetPasswordResetLoading(false);
        ShowPasswordResetSuccess("Password reset email sent. Check your inbox.");
    }

    private void OnPasswordResetFailure(string error)
    {
        SetPasswordResetLoading(false);
        ShowPasswordResetError(error);
    }
}
