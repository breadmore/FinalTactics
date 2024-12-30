using Unity.Services.Lobbies.Models;
using UnityEngine;
public class LobbyListUI : MonoBehaviour
{
    [SerializeField] private LobbyListSingleUI lobbyListSingleUIPrefab;

    public void CreateLobbyListSingleUI(Lobby lobby)
    {
        LobbyListSingleUI lobbyListSingleUI = Instantiate(lobbyListSingleUIPrefab, transform);
        lobbyListSingleUI.SetLobbyInfo(lobby.Name, lobby.Players.Count, lobby.MaxPlayers, lobby.Data["GameMode"].Value);
    }

    public void DestroyAllLobbyList()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
