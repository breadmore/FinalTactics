using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Linq;


public class GameManager : Singleton<GameManager>
{
    public Camera mainCamera;
    public Dictionary<string, PlayerData> PlayerDataDict { get; private set; } = new();
    public PlayerTeam teamA { get; private set; } = new(TeamName.TeamA);
    public PlayerTeam teamB { get; private set; } = new(TeamName.TeamB);

    [HideInInspector] public GridTile selectedGridTile = null;
    [HideInInspector] public PlayerBrain thisPlayerBrain;
    private bool gameStarted = false;

    

    public async Task LoadPlayers()
    {
        var lobby = LobbyManager.Instance.GetJoinedLobby();
        if (lobby != null)
        {
            await RelayManager.Instance.ConnectRelay();

            PlayerDataDict.Clear();
            foreach (var player in lobby.Players)
                PlayerDataDict[player.Id] = new PlayerData(player);

            AssignTeams();
            Debug.Log($"Loaded {PlayerDataDict.Count} players from the lobby.");
        }
        else
        {
            Debug.LogWarning("No lobby found, players not loaded.");
        }
    }

    private void AssignTeams()
    {
        teamA = new PlayerTeam(TeamName.TeamA);
        teamB = new PlayerTeam(TeamName.TeamB);

        foreach (var playerData in PlayerDataDict.Values)
        {
            if (playerData.IsInTeamA)
                teamA.Players.Add(playerData);
            else
                teamB.Players.Add(playerData);
        }
    }

    public void StartGame()
    {
        Debug.Log("Game Started!");
        gameStarted = true;
        TurnManager.Instance.Initialize(PlayerDataDict.Count);
    }

    public async void SetPlayerReady()
    {
        if (PlayerDataDict.TryGetValue(AuthenticationService.Instance.PlayerId, out var playerData))
        {
            playerData.SetReady(true);
            thisPlayerBrain.UpdateReadyStateServerRpc(playerData.player.Id, true);
            await WaitForAllPlayersReady();
        }
        else
        {
            Debug.Log("No player data");
        }
    }

    private async Task WaitForAllPlayersReady()
    {
        while (PlayerDataDict.Values.Any(p => !p.isReady))
        {
            Debug.Log("Waiting for players...");
            await Task.Delay(500);
        }
        Debug.Log("All players are ready.");
        StartGame();
    }
    public async Task<TeamName> GetTeamNameAsync(Player player)
    {
        while (player == null || !player.Data.ContainsKey("PlayerTeam"))
        {
            Debug.Log("Waiting for player data...");
            await Task.Delay(500);
        }

        return (player.Data["PlayerTeam"].Value != "False") ? TeamName.TeamB : TeamName.TeamA;
    }

}