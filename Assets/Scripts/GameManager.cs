using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Linq;
using System;


public class GameManager : NetworkSingleton<GameManager>
{
    public Dictionary<string, PlayerData> PlayerDataDict { get; private set; } = new();
    public PlayerTeam teamA { get; private set; } = new(TeamName.TeamA);
    public PlayerTeam teamB { get; private set; } = new(TeamName.TeamB);


    public GridTile SelectedGridTile { get; private set; } = null;
    public CharacterData SelectedCharacterData { get; private set; } = null;
    public int SelectedActionData { get; private set; } = 0;
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
            SetState(GameState.GameStarted, () => SelectedGridTile = gridTile);
        }
        else if(CurrentState == GameState.WaitingForSpawnBall)
        {
            SetState(GameState.WaitingForPlayerReady, () => SelectedGridTile = gridTile);
        }
    }

    public void OnActionSelected(ActionData actionData)
    {
        if (CurrentState == GameState.PlayerCharacterSelected)
        {
            SetState(GameState.ActionSelected, () => SelectedActionData = actionData.id);
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
            if (playerCharacter.OwnerClientId == thisPlayerBrain.OwnerClientId)
            {
                SetState(GameState.PlayerCharacterSelected, () => SelectedPlayerCharacter = playerCharacter);
                Debug.Log("Action Slot Open!");
                InGameUIManager.Instance.ActionSlot.SetActive(true);
            }
            else
            {
                InGameUIManager.Instance.ActionSlot.SetActive(false);
                Debug.Log("You cannot select this character!");
            }
        }
    }

    // Test State
    public void OnWaitingForSpawnBall()
    {
        if(CurrentState == GameState.WaitingForPlayerReady)
        {
            SetState(GameState.WaitingForSpawnBall);
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
        //TurnManager.Instance.Initialize(PlayerDataDict.Count);

        SetState(GameState.GameStarted);
    }

    public void StartAction()
    {
        SetState(GameState.WaitingForOtherPlayerAction);
        TurnManager.Instance.ExecuteActions();
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

        if (CurrentState == GameState.WaitingForPlayerReady)
        {
            StartGame();
        }else if(CurrentState == GameState.GameStarted)
        {
                    // 행동 실행
            StartAction();
        }
        Debug.Log("All players are ready.");


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
        if (SelectedActionData == 0 || SelectedPlayerCharacter == null)
        {
            Debug.LogWarning("No action or character selected.");
            return;
        }

        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(targetPosition);
        ActionType selectAction = LoadDataManager.Instance.actionDataReader.GetActionDataById(SelectedActionData).action;
        IActionHandler handler = ActionHandlerFactory.CreateHandler(selectAction);

        if (handler != null && handler.CanExecute(SelectedPlayerCharacter, targetTile))
        {
            handler.ExecuteAction(SelectedPlayerCharacter, targetTile);
        }
        else
        {
            Debug.LogWarning("Action cannot be executed.");
        }
    }

    public void ResetAllPlayersReadyState()
    {
        foreach (var playerData in PlayerDataDict.Values)
        {
            playerData.SetReady(false);
        }

        Debug.Log("All players' ready state reset to false.");

        //클라이언트들에게 동기화
        ResetAllPlayersReadyStateClientRpc();
    }

    [ClientRpc]
    private void ResetAllPlayersReadyStateClientRpc()
    {
        if (IsHost) return;

        foreach (var playerData in PlayerDataDict.Values)
        {
            playerData.SetReady(false);
        }

        Debug.Log("All players' ready state synced to false.");
    }

    [Command]
    public void PrintAllPlayersReadyState()
    {
        Debug.Log("===== All Players Ready State =====");

        foreach (var playerData in PlayerDataDict.Values)
        {
            Debug.Log($"Player ID: {playerData.player.Id}, Ready: {playerData.isReady}");
        }

        Debug.Log("===================================");
    }

    [Command]
    public void ShowCurrentState()
    {
        Debug.Log(CurrentState.ToString());
    }

    [Command]
    public void TestReady()
    {
        ResetAllPlayersReadyState();
    }
}