using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private PlayerListSingleUI playerListSingleUIPrefab;

    public void CreateAllPlayerList(Lobby lobby)
    {
        foreach(Player player in lobby.Players)
        {
            PlayerListSingleUI playerListSingleUI = Instantiate(playerListSingleUIPrefab, transform);
            playerListSingleUI.SetPlayerInfo(player.Data["PlayerName"].Value, false, lobby.Data["GameMode"].Value);
        }
    }

    public void DestroyAllPlayerList()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
