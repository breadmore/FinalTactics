using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    public static event Action OnAuthenticationSuccess;
    private bool isSigningIn = false; // 로그인 상태를 추적할 플래그

    [SerializeField] private Button authenticateButton;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private Button createPlayerConfirmButton;

    private void Start()
    {
        authenticateButton.onClick.AddListener(Authenticate);
        createPlayerConfirmButton.onClick.AddListener(CreatePlayer);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Authenticate()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn && !isSigningIn)
        {
            isSigningIn = true;

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e)
            {
                Debug.LogError("Authentication failed: " + e.Message);
            }
            finally
            {
                isSigningIn = false;
                //UIManager.Instance.SetState(UIState.Authentication,UIState.Lobby);
                PopUpGroup.Instance.PushPopUp(PopUpGroup.Instance.createPlayerPopUp);
            }
        }
        else
        {
            Debug.Log("Already signed in or login is in progress.");
        }
    }

    private void CreatePlayer()
    {
        AuthenticationService.Instance.UpdatePlayerNameAsync(playerNameInput.text);
        PopUpGroup.Instance.CloseTopPopUp();
        LobbyUIManager.Instance.SetState(UIState.Authentication, UIState.Lobby);
        OnAuthenticationSuccess?.Invoke();
    }
}
