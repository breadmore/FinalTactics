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
using Cysharp.Threading.Tasks;
using TMPro;


public class GameManager : NetworkSingleton<GameManager>
{
    public Dictionary<string, PlayerData> PlayerDataDict { get; private set; } = new();
    public PlayerTeam teamRed { get; private set; } = new(TeamName.Red);
    public PlayerTeam teamBlue { get; private set; } = new(TeamName.Blue);


    public GridTile SelectedGridTile { get; private set; } = null;
    public CharacterData SelectedCharacterData { get; private set; } = null;
    public int SelectedActionData { get; private set; } = 0;
    public PlayerCharacter SelectedPlayerCharacter { get; private set; } = null;
    public int SelectedActionOptionData { get; private set; } = 0;

    public PlayerBrain thisPlayerBrain;

    public IGameState _currentState;

    private NetworkVariable<TeamName> attackingTeam = new NetworkVariable<TeamName>();
    public TeamName AttackingTeam => attackingTeam.Value;

    // 팀별 액션 카운트
    public Dictionary<TeamName, ActionCounter> teamActionCounters = new Dictionary<TeamName, ActionCounter>();

    public void SetCharacterData(CharacterData characterData) => SelectedCharacterData = characterData;
    public void SetGridTile(GridTile gridTile) => SelectedGridTile = gridTile;
    public void SetActionData(int actionData) => SelectedActionData = actionData;
    public void SetActionOptionData(int actionOptionData) => SelectedActionOptionData = actionOptionData;
    public void SetPlayerCharacter(PlayerCharacter playerCharacter) => SelectedPlayerCharacter = playerCharacter;

    // 상태 변경 메서드
    public void OnCharacterDataSelected(CharacterData characterData) => _currentState.OnCharacterDataSelected(characterData);
    public void OnGridTileSelected(GridTile gridTile) => _currentState.OnGridTileSelected(gridTile);
    public void OnPlayerCharacterSelected(PlayerCharacter playerCharacter) => _currentState.OnPlayerCharacterSelected(playerCharacter);
    public void OnActionSelected(ActionData actionData) => _currentState.OnActionSelected(actionData);
    public void OnActionOptionSelected(ActionOptionData actionOptionData) => _currentState.OnActionOptionSelected(actionOptionData);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            attackingTeam.OnValueChanged += OnAttackingTeamChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer)
            attackingTeam.OnValueChanged -= OnAttackingTeamChanged;
    }

    private void OnAttackingTeamChanged(TeamName prev, TeamName curr)
    {
        Debug.Log($"[CLIENT] Attacking team changed: {curr}");
        BallManager.Instance.UpdateSpawnBallButtonState();
    }

    public void DecideFirstAttack()
    {
        if (IsServer)
        {
            bool coinResult = CoinToss();
            attackingTeam.Value = coinResult ? TeamName.Red : TeamName.Blue;
            Debug.Log($"Attacking team is {AttackingTeam}");
        }
    }

    public void ChangeState(IGameState newState)
    {
        _currentState?.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    public void ChangeState<T>() where T : IGameState, new()
    {
        ChangeState(new T());
    }

    public void ClearAllSelected()
    {
        SelectedGridTile = null;

        SelectedCharacterData = null;
        SelectedActionData = 0;
        if (SelectedPlayerCharacter != null)
        {
            SelectedPlayerCharacter.clickParticle.gameObject.SetActive(false);
            SelectedPlayerCharacter = null;
        }
        SelectedActionOptionData = 0;
    }
    public void ClearSelected<T>(ref T selectedField)
    {
        selectedField = default;
    }


    public async Task LoadPlayers()
    {
        try
        {
            var lobby = LobbyManager.Instance.GetJoinedLobby();
            if (lobby == null)
            {
                Debug.LogError("Lobby is null!");
                return;
            }

            await RelayManager.Instance.ConnectRelay();

            PlayerDataDict.Clear();
            foreach (var player in lobby.Players)
            {
                Debug.Log($"Loading player: {player.Id}, Name: {player.Data["PlayerName"].Value}");
                PlayerDataDict[player.Id] = new PlayerData(player);
            }

            AssignTeams();
            Debug.Log($"Successfully loaded {PlayerDataDict.Count} players");
        }
        catch (Exception e)
        {
            Debug.LogError($"LoadPlayers failed: {e}");
        }
    }

    private void AssignTeams()
    {
        teamRed = new PlayerTeam(TeamName.Red);
        teamBlue = new PlayerTeam(TeamName.Blue);

        foreach (var playerData in PlayerDataDict.Values)
        {
            if (playerData.IsRedTeam)
                teamRed.Players.Add(playerData);
            else
                teamBlue.Players.Add(playerData);
        }
    }

    public void InitializeActionCounters()
    {
        teamActionCounters[TeamName.Red] = new ActionCounter();
        teamActionCounters[TeamName.Blue] = new ActionCounter();
    }

    [ClientRpc]
    public void IncrementActionCountClientRpc(TeamName team, ActionType actionType)
    {
        if (teamActionCounters.ContainsKey(team))
        {
            teamActionCounters[team].IncrementCount(actionType);
        }
    }

    public void ResetAllActionCounts()
    {
        foreach (var counter in teamActionCounters.Values)
        {
            counter.ResetCounts();
        }
    }


    public void StartNewTurn()
    {
        ChangeState<InitGameState>();
    }

    public void StartAction()
    {
        ChangeState<ActionExecutionState>();
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

        // 현재 상태에 처리 위임
        _currentState.HandleAllPlayersReady();

        await Task.Delay(1000);
        await ResetAllPlayersReadyStateAsync();
    }
    public async Task<TeamName> GetTeamNameAsync(Player player)
    {
        while (player == null || !player.Data.ContainsKey("PlayerTeam"))
        {
            Debug.Log("Waiting for player data...");
            await Task.Delay(500);
        }

        return (player.Data["PlayerTeam"].Value != "1") ? TeamName.Red : TeamName.Blue;
    }


    public async UniTask ResetAllPlayersReadyStateAsync()
    {
        foreach (var playerData in PlayerDataDict.Values)
        {
            playerData.SetReady(false);
            await UniTask.Yield(); // 연산 분산 처리 (플레이어 수가 많을 경우 대비)
        }

        Debug.Log("All players' ready state reset to false.");

        ResetAllPlayersReadyStateClientRpc();

        await UniTask.Yield();
    }


    [ClientRpc]
    private void ResetAllPlayersReadyStateClientRpc()
    {
        ResetAllPlayerReadyState().Forget();
    }

    private async UniTask ResetAllPlayerReadyState()
    {
        if (IsServer) return;

        foreach (var playerData in PlayerDataDict.Values)
        {
            playerData.SetReady(false);
            await UniTask.Yield();
        }

        Debug.Log("All players' ready state synced to false.");
        await UniTask.Yield();
    }

    public bool IsGameFinished()
    {
        return teamRed.score >= 3 || teamBlue.score >= 3;
    }

    public void Goal(TeamName teamName)
    {
        NotifyAlertClientRpc(GameConstants.GoalText, 1.2f);

        if (teamName == TeamName.Red)
        {
            teamRed.score++;
            attackingTeam.Value = TeamName.Blue;
            if (teamRed.score >= 3)
            {
                ChangeState<GameFinishedState>();
                return; // 바로 종료 상태로 전환
            }
        }
        else
        {
            teamBlue.score++;
            attackingTeam.Value = TeamName.Red;
            if (teamBlue.score >= 3)
            {
                ChangeState<GameFinishedState>();
                return; // 바로 종료 상태로 전환
            }
        }

        SyncScoreClientRpc(teamRed.score, teamBlue.score);

        if (IsServer)
        {
            // 게임 종료가 아닌 경우에만 리셋
            if (!IsGameFinished())
            {
                ResetAfterGoal().Forget();
            }
        }
    }


    [ClientRpc]
    public void NotifyAlertClientRpc(string alertText, float time)
    {
        AlertManager.Instance.ShowAlert(alertText, time);
    }

    [ClientRpc]
    public void NotifyAlertClientRpc(string alertText)
    {
        AlertManager.Instance.ShowAlert(alertText);
    }

    private async UniTaskVoid ResetAfterGoal()
    {
        await UniTask.Delay(2000); // 2초 대기
        ChangeState<GameResetState>();
    }

    [ClientRpc]
    public void ResetGameClientRpc()
    {
        Debug.Log("All Client Reset");
        // 모든 클라이언트 초기화
        ChangeState<CharacterDataSelectionState>();
        BallManager.Instance.UpdateSpawnBallButtonState();
    }

    [ClientRpc]
    public void FinishGameClientRpc()
    {
        Debug.Log("All Client Finish");
        // 모든 클라이언트 초기화
        ChangeState<GameFinishedState>();
    }

    [ClientRpc]
    private void SyncScoreClientRpc(int teamAScore, int teamBScore)
    {
        InGameUIManager.Instance.UpdateScoreUI(teamAScore, teamBScore);
    }

    public bool CoinToss()
    {
        int result = UnityEngine.Random.Range(0, 2); // 0 또는 1
        return result == 0;
    }

    #region === 퀀텀 콘솔 커맨드 ===
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
        Debug.Log(_currentState.GetType().ToString());
    }

    [Command]
    public void ShowPlayerStat()
    {
        Debug.Log(SelectedPlayerCharacter.CharacterStat.stamina.ToString());
    }

    [Command]
    public void ShowScore()
    {
        Debug.Log("A Team score : " + teamRed.score);

        Debug.Log("B Team score : " + teamBlue.score);
    }

    [Command]
    public void CheckAuthStatus()
    {
        Debug.Log($"IsAuthenticated: {AuthenticationService.Instance.IsSignedIn}");
        Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        Debug.Log($"PlayerName: {AuthenticationService.Instance.PlayerName}");
    }

    [Command]
    public void ShowAllTeam()
    {
        Debug.Log("Team Red : " + teamRed.Players.Count);
        Debug.Log("Team Blue : " + teamBlue.Players.Count);
    }
    #endregion
}