using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using QFSW.QC;
using Unity.Netcode;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using System.Linq;
using System;
using System.Collections;


public class GameManager : NetworkSingleton<GameManager>
{
    public Dictionary<string, PlayerData> PlayerDataDict { get; private set; } = new();
    public PlayerTeam teamA { get; private set; } = new(TeamName.TeamA);
    public PlayerTeam teamB { get; private set; } = new(TeamName.TeamB);


    public GridTile SelectedGridTile { get; private set; } = null;
    public CharacterData SelectedCharacterData { get; private set; } = null;
    public int SelectedActionData { get; private set; } = 0;
    public PlayerCharacter SelectedPlayerCharacter { get; private set; } = null;

    // Option
    public ShootOption SelectedShootOption { get; private set; } = ShootOption.Cancel;

    public PlayerBrain thisPlayerBrain;

    public GameState CurrentState { get; private set; } = GameState.WaitingForPlayerReady;

    public event Action<GameState> OnGameStateChanged;

    public void SetTeamCamera()
    {
        if (thisPlayerBrain.GetMyTeam() == TeamName.TeamB)
        {
            CameraManager.Instance.cinemachineCamera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }

    public void SetState(GameState newState, Action callback = null)
    {
        CurrentState = newState;
        callback?.Invoke();  // 선택된 액션에 따라 필요한 추가 작업을 실행
        OnGameStateChanged?.Invoke(CurrentState);
        Debug.Log($"State changed to {CurrentState}");
    }
    public void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
        Debug.Log($"State changed to {CurrentState}");

    }

    public void OnCharacterDataSelected(CharacterData characterData)
    {
        if (CurrentState == GameState.WaitingForPlayerReady)
        {
            SetState(GameState.CharacterDataSelected, () => SelectedCharacterData = characterData);
        }
        
        
        // Test State
        else if(CurrentState == GameState.TestState)
        {
            Debug.Log("Test Character Data");
            SelectedCharacterData = characterData;
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
            ActionPreviewManager.Instance.ClearHighlights();
            InGameUIManager.Instance.CloseAllSlot();
        }
        else if(CurrentState == GameState.WaitingForSpawnBall)
        {
            SetState(GameState.WaitingForPlayerReady, () => SelectedGridTile = gridTile);
        }
        // Test State
        else if (CurrentState == GameState.TestState)
        {
            SelectedGridTile = gridTile;
        }
    }

    public void OnActionSelected(ActionData actionData)
    {

        if (CurrentState == GameState.PlayerCharacterSelected)
        {
            if (actionData.action == ActionType.Shoot)
            {
                InGameUIManager.Instance.ToggleOption();
            }
            else
            {
                SetState(GameState.ActionSelected, () => SelectedActionData = actionData.id);
            }

            
        }

        // Test State
        else if (CurrentState == GameState.TestState)
        {
            SelectedActionData = actionData.id;
        }
    }
    public void OnPlayerCharacterSelected(PlayerCharacter playerCharacter)
    {
        ActionPreviewManager.Instance.ClearHighlights();
        
        if (CurrentState == GameState.GameStarted)
        {
            if (playerCharacter.OwnerClientId == thisPlayerBrain.OwnerClientId)
            {
                SetState(GameState.PlayerCharacterSelected, () => SelectedPlayerCharacter = playerCharacter);
                InGameUIManager.Instance.ActionSlot.SetActive(true);
            }
            else
            {
                InGameUIManager.Instance.CloseAllSlot();
            }
        }

        // Test State
        else if (CurrentState == GameState.TestState)
        {
            Debug.Log("Test Player Character");
            SelectedPlayerCharacter = playerCharacter;
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

    public void OnShootOption(ShootOption shootOption)
    {
        SelectedShootOption = shootOption;
        switch (shootOption)
        {
            case ShootOption.Cancel:
                InGameUIManager.Instance.ToggleOption();
                break;
            case ShootOption.Charge:
                InGameUIManager.Instance.CloseAllSlot();

                SetState(GameState.GameStarted);
                TurnManager.Instance.SubmitActionServerRpc(SelectedPlayerCharacter.NetworkObjectId,
                    2,  // 슛 아이디
                    SelectedPlayerCharacter.GridPosition);
                break;
            case ShootOption.Shoot:
                InGameUIManager.Instance.ToggleOption();
                SetState(GameState.ActionSelected, () => SelectedActionData = 2); // 슛 아이디
                break;
        }

    }

    public void ClearAllSelected()
    {
        SelectedGridTile = null;
        SelectedCharacterData = null;
        SelectedActionData = 0;
        SelectedPlayerCharacter = null;
        SelectedShootOption = ShootOption.Cancel;
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

    public void InitGame()
    {
        InGameUIManager.Instance.CharacterSlot.SetActive(true);
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

        Debug.Log("All players are ready.");

        if (CurrentState == GameState.WaitingForPlayerReady)
        {
            StartGame();
        }
        else if (CurrentState == GameState.GameStarted)
        {
            // 행동 실행
            StartAction();
        }

            // 시작 딜레이
        await Task.Delay(1000);

        ResetAllPlayersReadyState();
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

    public void Goal(TeamName teamName)
    {
        if (teamName == TeamName.TeamA)
        {
            teamA.score++;
            Debug.Log("Team A Goal!!!!!");

            if (teamA.score >= 3)
            {
                // A팀 승리
                InGameUIManager.Instance.resultPanel.ShowResult();
            }
        }
        else
        {
            teamB.score++;
            Debug.Log("Team B Goal!!!!!");
            if (teamB.score >= 3)
            {
                // B팀 승리
                InGameUIManager.Instance.resultPanel.ShowResult();
            }
        }

        // 클라이언트에게 골 연출 보여주기
        NotifyGoalClientRpc(teamName);
        SyncScoreClientRpc(teamA.score, teamB.score);

        // 리셋 루틴 시작 (서버만 실행)
        StartCoroutine(ResetAfterGoal());

        InitGame();
    }

    private void ReturnAllCharacter()
    {
        PlayerCharacterNetworkPool.Instance.ReturnAllCharacter();

        Debug.Log("All characters returned to pool.");
    }


    [ClientRpc]
    private void NotifyGoalClientRpc(TeamName scoringTeam)
    {
        InGameUIManager.Instance.ShowGoalMessage(scoringTeam);
    }

    private IEnumerator ResetAfterGoal()
    {
        yield return new WaitForSeconds(2f);  // 골 연출 시간

        SetState(GameState.WaitingForReset);

        ReturnAllCharacter();
        BallManager.Instance.DespawnBall();
        ResetAllPlayersReadyState();  // 준비 상태 초기화
        ResetGameClientRpc();
    }
    [ClientRpc]
    private void ResetGameClientRpc()
    {
        // 클라이언트에서 상태 초기화 및 게임 준비 UI 띄우기
        SetState(GameState.WaitingForPlayerReady);
        InitGame();
    }

    [ClientRpc]
    private void SyncScoreClientRpc(int teamAScore, int teamBScore)
    {
        InGameUIManager.Instance.UpdateScoreUI(teamAScore, teamBScore);
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

    [Command]
    public void ShowPlayerStat()
    {
        Debug.Log(SelectedPlayerCharacter.CharacterStat.stamina.ToString());
    }

    [Command]
    public void ShowScore()
    {
        Debug.Log("A Team score : " + teamA.score);

        Debug.Log("B Team score : " + teamB.score);
    }
}