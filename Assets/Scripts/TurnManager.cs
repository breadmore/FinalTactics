using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : Singleton<TurnManager>
{

    public event Action<int> OnTurnStart; // 턴 시작 시 호출
    public event Action OnAllActionsSubmitted; // 모든 플레이어가 행동을 제출했을 때 호출
    public event Action<int> OnTurnEnd; // 턴 종료 시 호출

    private int currentTurn = 1; // 현재 턴
    private int totalPlayers = 4; // 총 플레이어 수
    private Dictionary<int, string> playerActions = new Dictionary<int, string>(); // 플레이어 행동 저장

    private void Start()
    {
        StartTurn();
    }

    // 턴 시작
    private void StartTurn()
    {
        playerActions.Clear(); // 이전 턴의 행동을 초기화

        Debug.Log($"Turn {currentTurn} started!");
        OnTurnStart?.Invoke(currentTurn);
    }

    // 플레이어가 행동 제출
    public void SubmitAction(int playerId, string action)
    {
        if (!playerActions.ContainsKey(playerId))
        {
            playerActions[playerId] = action;
            Debug.Log($"Player {playerId} submitted action: {action}");

            if (playerActions.Count == totalPlayers)
            {
                ExecuteActions();
            }
        }
    }

    // 모든 행동 실행
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

    // 턴 종료 후 다음 턴으로 이동
    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(1f); // 잠깐 대기

        OnTurnEnd?.Invoke(currentTurn);

        currentTurn++; // 턴 증가
        StartTurn();
    }

    // 현재 턴 가져오기
    public int GetCurrentTurn() => currentTurn;
    
}
