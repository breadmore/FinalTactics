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
    public PlayerTeam teamA { get; private set; } = new(TeamName.TeamA);
    public PlayerTeam teamB { get; private set; } = new(TeamName.TeamB);


    public GridTile SelectedGridTile { get; private set; } = null;
    public CharacterData SelectedCharacterData { get; private set; } = null;
    public int SelectedActionData { get; private set; } = 0;
    public PlayerCharacter SelectedPlayerCharacter { get; private set; } = null;
    public int SelectedActionOptionData { get; private set; } = 0;

    public PlayerBrain thisPlayerBrain;

    public IGameState _currentState;

    private NetworkVariable<TeamName> attackingTeam = new NetworkVariable<TeamName>();
    public TeamName AttackingTeam => attackingTeam.Value;


    public void SetCharacterData(CharacterData characterData) => SelectedCharacterData = characterData;
    public void SetGridTile(GridTile gridTile) => SelectedGridTile = gridTile;
    public void SetActionData(int actionData) => SelectedActionData = actionData;
    public void SetActionOptionData(int actionOptionData) => SelectedActionOptionData = actionOptionData;
    public void SetPlayerCharacter(PlayerCharacter playerCharacter) => SelectedPlayerCharacter = playerCharacter;


    public void DecideFirstAttack()
    {
        if (!IsServer) return;
        bool coinResult = CoinToss();
        attackingTeam.Value = coinResult ? TeamName.TeamA : TeamName.TeamB;
        Debug.Log($"Attacking team is {AttackingTeam}");

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

    // 상태 변경 메서드들 (기존 메서드 대체)
    public void OnCharacterDataSelected(CharacterData characterData) => _currentState.OnCharacterDataSelected(characterData);
    public void OnGridTileSelected(GridTile gridTile) => _currentState.OnGridTileSelected(gridTile);
    public void OnPlayerCharacterSelected(PlayerCharacter playerCharacter) => _currentState.OnPlayerCharacterSelected(playerCharacter);
    public void OnActionSelected(ActionData actionData) => _currentState.OnActionSelected(actionData);
    public void OnActionOptionSelected(ActionOptionData actionOptionData) => _currentState.OnActionOptionSelected(actionOptionData);

    public void SetTeamCamera()
    {
        if (thisPlayerBrain.GetMyTeam() == TeamName.TeamB)
        {
            CameraManager.Instance.cinemachineCamera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }
    public void ClearAllSelected()
    {
        SelectedGridTile = null;
        SelectedCharacterData = null;
        SelectedActionData = 0;
        SelectedPlayerCharacter = null;
        SelectedActionOptionData = 0;
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
        ChangeState<MainGameState>();
    }

    public void StartAction()
    {
        ChangeState<ActionExcutionState>();
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

        // 현재 상태에 처리 위임
        _currentState.HandleAllPlayersReady();

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
        if (IsServer) return;

        foreach (var playerData in PlayerDataDict.Values)
        {
            playerData.SetReady(false);
        }

        Debug.Log("All players' ready state synced to false.");
    }

    public void Goal(TeamName teamName)
    {
        NotifyGoalClientRpc(teamName);

        if (teamName == TeamName.TeamA)
        {
            teamA.score++;
            attackingTeam.Value = TeamName.TeamB;
            if (teamA.score >= 3)
            {
                // A팀 승리
                InGameUIManager.Instance.resultPanel.ShowResult();
            }
        }
        else
        {
            teamB.score++;
            attackingTeam.Value = TeamName.TeamA;
            if (teamB.score >= 3)
            {
                // B팀 승리
                InGameUIManager.Instance.resultPanel.ShowResult();
            }
        }

        // 클라이언트에게 골 연출 보여주기

        SyncScoreClientRpc(teamA.score, teamB.score);

        if(IsServer)
        ResetAfterGoal().Forget();


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

    private async UniTaskVoid ResetAfterGoal()
    {
        await UniTask.Delay(2000); // 2초 대기
        ChangeState<GameResetState>();
    }

    [ClientRpc]
    public void ResetGameClientRpc()
    {
        // 모든 클라이언트 초기화
        ChangeState<CharacterSelectionState>();
        BallManager.Instance.UpdateSpawnBallButtonState();
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


    // Quantum Console Command

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