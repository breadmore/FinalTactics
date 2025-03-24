using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : Singleton<TurnManager>
{
    public event Action<int> OnTurnStart; // 턴 시작 시 호출
    public event Action OnAllActionsSubmitted; // 모든 플레이어가 행동을 제출했을 때 호출
    public event Action<int> OnTurnEnd; // 턴 종료 시 호출

    private int currentTurn; // 현재 턴
    private int totalPlayers; // 총 플레이어 수
    private Dictionary<int, string> playerActions = new Dictionary<int, string>(); // 플레이어 행동 저장
    private bool isGameActive = false; // 게임 진행 상태

    public void Initialize(int playerCount)
    {
        totalPlayers = playerCount;
        currentTurn = 1;
        isGameActive = true;
        StartTurn();
    }

    private void StartTurn()
    {
        if (!isGameActive) return;

        playerActions.Clear(); // 이전 턴의 행동 초기화
        Debug.Log($"Turn {currentTurn} started!");
        OnTurnStart?.Invoke(currentTurn);
    }

    public void SubmitAction(int playerId, string action)
    {
        if (!isGameActive || playerActions.ContainsKey(playerId)) return;

        playerActions[playerId] = action;
        Debug.Log($"Player {playerId} submitted action: {action}");

        if (playerActions.Count == totalPlayers)
        {
            ExecuteActions();
        }
    }

    private void ExecuteActions()
    {
        Debug.Log($"Executing all actions for Turn {currentTurn}");

        foreach (var action in playerActions)
        {
            Debug.Log($"Player {action.Key} executes: {action.Value}");
        }

        OnAllActionsSubmitted?.Invoke();
        StartCoroutine(EndTurn());
    }

    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(1f);
        OnTurnEnd?.Invoke(currentTurn);

        if (isGameActive)
        {
            currentTurn++;
            StartTurn();
        }
    }

    public void NextTurn()
    {
        if (isGameActive)
        {
            StopAllCoroutines();
            StartCoroutine(EndTurn());
        }
    }

    public void EndGame()
    {
        isGameActive = false;
        Debug.Log("Game Over! Stopping turns.");
    }

    public int GetCurrentTurn() => currentTurn;
}
