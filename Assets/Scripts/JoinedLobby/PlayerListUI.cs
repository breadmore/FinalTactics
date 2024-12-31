using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private PlayerListSingleUI playerListSingleUIPrefab;

    public void CreatePlayer(Player player)
    {
        if (player.Data == null || !player.Data.ContainsKey("PlayerName") || player.Data["PlayerName"].Value == null)
        {
            Debug.LogError("Player data or PlayerName is null.");
            return;
        }

        PlayerListSingleUI playerListSingleUI = Instantiate(playerListSingleUIPrefab, transform);

        bool ready = false;
        if (player.Data.TryGetValue("PlayerReady", out PlayerDataObject readyData))
        {
            bool.TryParse(readyData.Value, out ready);
        }

        playerListSingleUI.SetPlayerInfo(player.Data["PlayerName"].Value, ready);
    }

    public void CreatePlayerListInLobby(Lobby lobby)
    {
        Debug.Log(lobby.Players.Count);
        foreach(Player player in lobby.Players)
        {
            print("Players create");
            CreatePlayer(player);
        }
    }

    public void UpdatePlayerInfo()
    {

    }

    public void DestroyAllPlayerList()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
