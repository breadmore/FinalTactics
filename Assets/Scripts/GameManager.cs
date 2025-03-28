﻿using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Linq;
using System;


public class GameManager : Singleton<GameManager>
{
    public Dictionary<string, PlayerData> PlayerDataDict { get; private set; } = new();
    public PlayerTeam teamA { get; private set; } = new(TeamName.TeamA);
    public PlayerTeam teamB { get; private set; } = new(TeamName.TeamB);


    public GridTile SelectedGridTile { get; private set; } = null;
    public CharacterData SelectedCharacterData { get; private set; } = null;
    public ActionData SelectedActionData { get; private set; } = null;
    public PlayerCharacter SelectedPlayerCharacter { get; private set; } = null;


    public PlayerBrain thisPlayerBrain;

    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayerReady;


    public void SetState(GameState newState, Action callback = null)
    {
        CurrentState = newState;
        callback?.Invoke();  // 선택된 액션에 따라 필요한 추가 작업을 실행
        Debug.Log($"State changed to {CurrentState}");
    }
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"State changed to {CurrentState}");
    }

    public void OnCharacterDataSelected(CharacterData characterData)
    {
        if (CurrentState == GameState.WaitingForPlayerReady)
        {
            SetState(GameState.CharacterDataSelected, () => SelectedCharacterData = characterData);
        }
    }

    public void OnGridTileSelected(GridTile gridTile)
    {
        // 캐릭터 데이터 선택 후
        if (CurrentState == GameState.CharacterDataSelected)
        {
            SetState(GameState.WaitingForPlayerReady, () => SelectedGridTile = gridTile);
        }
        // 액션 선택 후
        else if(CurrentState == GameState.ActionSelected)
        {
            SetState(GameState.WaitingForPlayerReady, () => SelectedGridTile = gridTile);
        }
    }

    public void OnActionSelected(ActionData actionData)
    {
        if (CurrentState == GameState.PlayerCharacterSelected)
        {
            SetState(GameState.ActionSelected, () => SelectedActionData = actionData);
        }
    }
    public void OnPlayerCharacterSelected(PlayerCharacter playerCharacter)
    {
        if(CurrentState == GameState.WaitingForPlayerReady)
        {
            SetState(GameState.PlayerCharacterSelected, () => SelectedPlayerCharacter = playerCharacter);
        }
        else if (CurrentState == GameState.GameStarted)
        {
            SetState(GameState.PlayerCharacterSelected, () => SelectedPlayerCharacter = playerCharacter);
        }
    }

    public void ClearSelected<T>(ref T selectedField)
    {
        selectedField = default;
    }


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

    // 행동 결정 함수
    public void ExecuteSelectedAction(Vector2Int targetPosition)
    {
        if (SelectedActionData == null || SelectedPlayerCharacter == null)
        {
            Debug.LogWarning("No action or character selected.");
            return;
        }

        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(targetPosition);
        IActionHandler handler = ActionHandlerFactory.CreateHandler(SelectedActionData.action);

        if (handler != null && handler.CanExecute(SelectedPlayerCharacter, targetTile))
        {
            handler.ExecuteAction(SelectedPlayerCharacter, targetTile);
        }
        else
        {
            Debug.LogWarning("Action cannot be executed.");
        }
    }


}