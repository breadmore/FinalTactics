using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private PlayerListSingleUI playerListSingleUIPrefab;
    private Dictionary<string, PlayerListSingleUI> existingPlayer = new Dictionary<string, PlayerListSingleUI>();
    public void CreatePlayer(Player player)
    {
        if (player.Data == null || !player.Data.ContainsKey("PlayerName") || player.Data["PlayerName"].Value == null)
        {
            Debug.LogError("Player data or PlayerName is null.");
            return;
        }

        PlayerListSingleUI playerListSingleUI = Instantiate(playerListSingleUIPrefab, transform);

        playerListSingleUI.SetPlayerInfo(player);

        existingPlayer.Add(player.Id, playerListSingleUI);
    }


    public void CreatePlayerListInLobby(Lobby lobby)
    {
        DestroyAllPlayerList();
        foreach (Player player in lobby.Players)
        {
            CreatePlayer(player);
        }
    }
    public void UpdatePlayerListInLobby(Lobby lobby)
    {
        // �κ��� ���� �÷��̾� ID ���
        HashSet<string> currentPlayerIds = new HashSet<string>();
        foreach (Player player in lobby.Players)
        {
            currentPlayerIds.Add(player.Id);

            string playerId = player.Id;

            // ���� UI�� ������ ������Ʈ
            if (existingPlayer.TryGetValue(playerId, out PlayerListSingleUI existPlayer))
            {
                existPlayer.SetPlayerInfo(player);
            }
            else
            {
                // �� �÷��̾�� ����
                CreatePlayer(player);
            }
        }

        // ���� ��ųʸ����� �κ� ���� �÷��̾ ����
        foreach (string playerId in new List<string>(existingPlayer.Keys))
        {
            if (!currentPlayerIds.Contains(playerId))
            {
                RemovePlayer(playerId);
            }
        }
    }

    // Ư�� �÷��̾� ���� �޼���
    public void RemovePlayer(string playerId)
    {
        if (existingPlayer.TryGetValue(playerId, out PlayerListSingleUI playerUI))
        {
            Destroy(playerUI.gameObject); // UI ���� ������Ʈ ����
            existingPlayer.Remove(playerId); // ��ųʸ����� ����
        }
        else
        {
            Debug.LogWarning($"Player with ID {playerId} not found in the existing player list.");
        }
    }




    public void DestroyAllPlayerList()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        existingPlayer.Clear();
    }

    public Player FindPlayerList(string playerId)
    {
        Player player = null;



        return player;
    }
}
