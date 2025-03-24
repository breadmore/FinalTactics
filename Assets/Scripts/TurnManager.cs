using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : Singleton<TurnManager>
{
    public event Action<int> OnTurnStart; // �� ���� �� ȣ��
    public event Action OnAllActionsSubmitted; // ��� �÷��̾ �ൿ�� �������� �� ȣ��
    public event Action<int> OnTurnEnd; // �� ���� �� ȣ��

    private int currentTurn; // ���� ��
    private int totalPlayers; // �� �÷��̾� ��
    private Dictionary<int, string> playerActions = new Dictionary<int, string>(); // �÷��̾� �ൿ ����
    private bool isGameActive = false; // ���� ���� ����

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

        playerActions.Clear(); // ���� ���� �ൿ �ʱ�ȭ
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
