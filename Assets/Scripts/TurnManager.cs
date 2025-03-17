using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : Singleton<TurnManager>
{

    public event Action<int> OnTurnStart; // �� ���� �� ȣ��
    public event Action OnAllActionsSubmitted; // ��� �÷��̾ �ൿ�� �������� �� ȣ��
    public event Action<int> OnTurnEnd; // �� ���� �� ȣ��

    private int currentTurn = 1; // ���� ��
    private int totalPlayers = 4; // �� �÷��̾� ��
    private Dictionary<int, string> playerActions = new Dictionary<int, string>(); // �÷��̾� �ൿ ����

    private void Start()
    {
        StartTurn();
    }

    // �� ����
    private void StartTurn()
    {
        playerActions.Clear(); // ���� ���� �ൿ�� �ʱ�ȭ

        Debug.Log($"Turn {currentTurn} started!");
        OnTurnStart?.Invoke(currentTurn);
    }

    // �÷��̾ �ൿ ����
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

    // ��� �ൿ ����
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

    // �� ���� �� ���� ������ �̵�
    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(1f); // ��� ���

        OnTurnEnd?.Invoke(currentTurn);

        currentTurn++; // �� ����
        StartTurn();
    }

    // ���� �� ��������
    public int GetCurrentTurn() => currentTurn;
    
}
