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
        // 로비의 현재 플레이어 ID 목록
        HashSet<string> currentPlayerIds = new HashSet<string>();
        foreach (Player player in lobby.Players)
        {
            currentPlayerIds.Add(player.Id);

            string playerId = player.Id;

            // 기존 UI가 있으면 업데이트
            if (existingPlayer.TryGetValue(playerId, out PlayerListSingleUI existPlayer))
            {
                existPlayer.SetPlayerInfo(player);
            }
            else
            {
                // 새 플레이어는 생성
                CreatePlayer(player);
            }
        }

        // 기존 딕셔너리에서 로비에 없는 플레이어를 제거
        foreach (string playerId in new List<string>(existingPlayer.Keys))
        {
            if (!currentPlayerIds.Contains(playerId))
            {
                RemovePlayer(playerId);
            }
        }
    }

    // 특정 플레이어 제거 메서드
    public void RemovePlayer(string playerId)
    {
        if (existingPlayer.TryGetValue(playerId, out PlayerListSingleUI playerUI))
        {
            Destroy(playerUI.gameObject); // UI 게임 오브젝트 삭제
            existingPlayer.Remove(playerId); // 딕셔너리에서 제거
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
