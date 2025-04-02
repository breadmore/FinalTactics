using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
public class TurnManager : NetworkSingleton<TurnManager>
{
    public event Action<int> OnTurnStart; // �� ���� �� ȣ��
    public event Action OnAllActionsSubmitted; // ��� �÷��̾ �ൿ�� �������� �� ȣ��
    public event Action<int> OnTurnEnd; // �� ���� �� ȣ��

    private int currentTurn; // ���� ��
    private int totalPlayers; // �� �÷��̾� ��
    private Dictionary<ulong, int> PlayerActions = new Dictionary<ulong, int>(); // �÷��̾� �ൿ ����
    private bool isGameActive = false; // ���� ���� ����

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            totalPlayers = (int)NetworkManager.Singleton.ConnectedClients.Count;
            Initialize(totalPlayers);
        }
    }

    public void Initialize(int playerCount)
    {
        if (!IsServer) return;

        totalPlayers = playerCount;
        currentTurn = 1;
        isGameActive = true;
        StartTurn();
    }

    private void StartTurn()
    {
        if (!isGameActive) return;

        PlayerActions.Clear(); // ���� ���� �ൿ �ʱ�ȭ
        Debug.Log($"Turn {currentTurn} started!");
        OnTurnStart?.Invoke(currentTurn);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitActionServerRpc(ulong networkId, int actionId)
    {
        if (!isGameActive || PlayerActions.ContainsKey(networkId)) return;

        PlayerActions[networkId] = actionId;
        Debug.Log($"Player {networkId} submitted action: {actionId}");

        //if (PlayerActions.Count == totalPlayers)
        //{
        //    ExecuteActions();
        //}
    }

    private void ExecuteActions()
    {
        Debug.Log($"Executing all actions for Turn {currentTurn}");

        foreach (var action in PlayerActions)
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
