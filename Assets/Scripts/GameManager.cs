using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.Netcode;
using System.Threading.Tasks;

public class PlayerTeam
{
    public TeamName name { get; private set; }
    public List<Player> Players { get; private set; }

    public PlayerTeam(TeamName name)
    {
        this.name = name;
        Players = new List<Player>();
    }
}
public class GameManager : DontDestroySingleton<GameManager>
{
    public Camera mainCamera;
    public List<Player> PlayerList { get; private set; } = new List<Player>();
    public List<PlayerData> PlayerDataList { get; private set; } = new List<PlayerData> { };
    public GridTile selectedGridTile = null;

    public PlayerTeam teamA;
    public PlayerTeam teamB;

    private void Start()
    {

    }

    public async Task LoadPlayers()
    {
        if (LobbyManager.Instance.GetJoinedLobby() != null)
        {
            PlayerList = new List<Player>(LobbyManager.Instance.GetJoinedLobby().Players);
            AssignTeams();
            Debug.Log($"Loaded {PlayerList.Count} players from the lobby.");
            await RelayManager.Instance.ConnectRelay();
        }
        else
        {
            Debug.LogWarning("No lobby found, players not loaded.");
        }
    }

    public void InitPlayerData()
    {
        if (PlayerList.Count == 0) return;
        PlayerDataList.Clear();
        for (int i = 0; i < PlayerList.Count; i++)
        {
            var player = PlayerList[i];
            var playerData = new PlayerData(player.Id, i, 0);
            PlayerDataList.Add(playerData);
        }
        Debug.Log($"Initialized {PlayerDataList.Count} player data entries.");
    }


    private void AssignTeams()
    {
        teamA = new PlayerTeam(TeamName.TeamA);
        teamB = new PlayerTeam(TeamName.TeamB);

        teamA.Players.Clear();
        teamB.Players.Clear();

        foreach (var player in PlayerList)
        {
            if (player.Data["PlayerTeam"].Value == "False")
            {
                teamB.Players.Add(player);
            }
            else
            {
                teamA.Players.Add(player);
            }
        }
    }



    [Command]
    private void ShowPlayers()
    {
        Debug.Log(PlayerList.Count + " Players in Game!");
        foreach (var player in PlayerList)
        {
            Debug.Log("Player Id : " + player.Id);
        }
    }
}
