using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
public class LobbyListSingleUI : MonoBehaviour
{
    private Lobby lobby;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI gameModeText;
    private Button thisButton;

    private void Start()
    {
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(JoinLobby);
    }

    public void SetLobbyInfo(Lobby _lobby)
    {
        lobby = _lobby;

        lobbyNameText.text = lobby.Name;
        playersText.text = lobby.Players.Count + "/" + lobby.MaxPlayers;
        gameModeText.text = lobby.Data["GameMode"].Value;
    }

    private async void JoinLobby()
    {
        await LobbyManager.Instance.JoinLobbyById(lobby.Id);
        LobbyManager.Instance.SyncJoinLobby(lobby);
        LobbyUIManager.Instance.SetState(UIState.Lobby, UIState.JoinedLobby);

        LobbyManager.Instance.playerListUI.CreatePlayerListInLobby(lobby);
    }
}
