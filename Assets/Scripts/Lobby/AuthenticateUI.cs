using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class AuthenticateUI : MonoBehaviour
{
    public static event Action OnAuthenticationSuccess;
    private bool isSigningIn = false; // 로그인 상태를 추적할 플래그

    private string playerName;

    [SerializeField] private Button authenticateButton;

    private void Start()
    {
        authenticateButton.onClick.AddListener(Authenticate);
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async void Authenticate()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            OnAuthenticationSuccess?.Invoke();
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
                UIManager.Instance.SetState(UIState.Authentication,UIState.Lobby);
            }
        }
        else
        {
            Debug.Log("Already signed in or login is in progress.");
        }

        playerName = "TestName" + UnityEngine.Random.Range(10, 99);
        Debug.Log("Player Name : " + playerName);
    }
}
