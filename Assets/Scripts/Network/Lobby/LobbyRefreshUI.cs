using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyRefreshUI : MonoBehaviour
{
    [SerializeField] private Button refreshLobbyButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        LobbyManager.OnLobbyListUpdate += RefreshLobbyList; // �̺�Ʈ ����
    }

    private void OnDisable()
    {
        LobbyManager.OnLobbyListUpdate -= RefreshLobbyList; // �̺�Ʈ ���� ����
    }

    void Start()
    {
        refreshLobbyButton.onClick.AddListener(OnRefreshLobbyClicked);
    }

    public static async void RefreshLobbyList()
    {
        LobbyManager.Instance.lobbyListUI.DestroyAllLobbyList();
        await LobbyManager.Instance.ListLobbies();
        QueryResponse queryResponse = LobbyManager.Instance.GetQueryResponse();

        foreach (Lobby lobby in queryResponse.Results)
        {
            LobbyManager.Instance.lobbyListUI.CreateLobbyListSingleUI(lobby);
        }
    }
    public void OnRefreshLobbyClicked()
    {
        RefreshLobbyList();
    }
}
