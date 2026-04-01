using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using Virtuery.PlayFab;

public class AutoLoginChecker : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "MainMenu";
    [SerializeField] private string authSceneName = "AuthScreen";

    private void Start()
    {
        CheckAutoLogin();
    }

    private void CheckAutoLogin()
    {
        if (PlayFabAuthService.Instance == null)
        {
            LoadAuthScene();
            return;
        }

        if (PlayFabAuthService.Instance.IsLoggedIn)
        {
            LoadGameScene();
            return;
        }

        if (PlayFabAuthService.Instance.CanAutoLoginWithoutPassword())
        {
            PlayFabAuthService.Instance.OnLoginSuccess += OnAutoLoginSuccess;
            PlayFabAuthService.Instance.OnLoginFailure += OnAutoLoginFailure;
            PlayFabAuthService.Instance.TryAutoLogin();
        }
        else
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != authSceneName)
            {
                LoadAuthScene();
            }
        }
    }

    private void OnAutoLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        PlayFabAuthService.Instance.OnLoginSuccess -= OnAutoLoginSuccess;
        PlayFabAuthService.Instance.OnLoginFailure -= OnAutoLoginFailure;
        LoadGameScene();
    }

    private void OnAutoLoginFailure(string error)
    {
        PlayFabAuthService.Instance.OnLoginSuccess -= OnAutoLoginSuccess;
        PlayFabAuthService.Instance.OnLoginFailure -= OnAutoLoginFailure;
        LoadAuthScene();
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }
    }

    private void LoadAuthScene()
    {
        if (!string.IsNullOrEmpty(authSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(authSceneName);
        }
    }


    public void PlayAsGuest()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
