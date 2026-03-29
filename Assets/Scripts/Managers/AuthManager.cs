using UnityEngine;
using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System;

public class AuthManager : MonoBehaviour
{

    [SerializeField]
    public TMP_InputField FullnameInputField;
    public TMP_InputField EmailInputField;
    public TMP_InputField PasswordInputField;
    public TMP_InputField ConfirmPasswordInputField;

    void Start()
    {
        //login();
    }

    void login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }
    // Player registration method
    public void registerNewUser()
    {
        string fullName = FullnameInputField.text;
        string email = EmailInputField.text;
        string password = PasswordInputField.text;
        //string confirmPassword = ConfirmPasswordInputField.text;

        var request = new RegisterPlayFabUserRequest
        {
            DisplayName = fullName,
            Email = email,
            Password = password,
            Username = fullName,
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnLoginFailure);
    }


    // Player login method
    public void loginWithEmail()
    {
        string email = EmailInputField.text;
        string password = PasswordInputField.text;

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password,
        };

        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log($"Login successful! {result.Username} , {result.PlayFabId}");
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"Login successful! {result.NewlyCreated} , {result.PlayFabId} , {result.LastLoginTime}");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }
}
