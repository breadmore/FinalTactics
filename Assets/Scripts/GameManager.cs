using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.Netcode;
using System.Threading.Tasks;

public class GameManager : DontDestroySingleton<GameManager>
{
    public Camera mainCamera;
    public List<Player> playerList { get; private set; } = new List<Player>();
    public List<PlayerData> playerDataList { get; private set; } = new List<PlayerData> { };
    public Vector3? selectedGridPosition { get; set; }

    private Player[] Team1;
    private Player[] Team2;

    private void Start()
    {

    }

    public async Task LoadPlayers()
    {
        if (LobbyManager.Instance.GetJoinedLobby() != null)
        {
            playerList = new List<Player>(LobbyManager.Instance.GetJoinedLobby().Players);
            AssignTeams();
            Debug.Log($"Loaded {playerList.Count} players from the lobby.");
            await RelayManager.Instance.ConnectRelay();
        }
        else
        {
            Debug.LogWarning("No lobby found, players not loaded.");
        }
    }

    public void InitPlayerData()
    {
        if (playerList.Count == 0) return;
        playerDataList.Clear();
        for (int i = 0; i < playerList.Count; i++)
        {
            var player = playerList[i];
            var playerData = new PlayerData(player.Id, i, 0);
            playerDataList.Add(playerData);
        }
        Debug.Log($"Initialized {playerDataList.Count} player data entries.");
    }


    private void AssignTeams()
    {
        // Clear previous teams (if any)
        Team1 = new Player[0];
        Team2 = new Player[0];

        List<Player> team1Players = new List<Player>();
        List<Player> team2Players = new List<Player>();

        // Loop through the players and assign to teams based on playerTeam value
        foreach (var player in playerList)
        {
            // Check the player's team based on the LobbyManager's playerTeam value
            if (player.Data["PlayerTeam"].Value == "False")
            {
                team1Players.Add(player);
            }
            else
            {
                team2Players.Add(player);
            }
        }

        // Assign the players to Team1 and Team2
        Team1 = team1Players.ToArray();
        Team2 = team2Players.ToArray();

        Debug.Log($"Team 1 has {Team1.Length} players, Team 2 has {Team2.Length} players.");
    }



    [Command]
    private void ShowPlayers()
    {
        Debug.Log(playerList.Count + " Players in Game!");
        foreach (var player in playerList)
        {
            Debug.Log("Player Id : " + player.Id);
        }
    }
}
