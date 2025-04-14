using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using QFSW.QC;

public class TurnManager : NetworkSingleton<TurnManager>
{
    private struct PlayerAction
    {
        public ulong playerId;
        public int actionId;
        public GridTile targetTile;
    }

    public event Action<int> OnTurnStart;
    public event Action OnAllActionsSubmitted;
    public event Action<int> OnTurnEnd;

    private int currentTurn;
    private int totalPlayers;

    private List<PlayerAction> defenseActions = new List<PlayerAction>();
    private List<PlayerAction> commonActions = new List<PlayerAction>();
    private List<PlayerAction> offenseActions = new List<PlayerAction>();

    private bool isGameActive = false;

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
        //StartTurn();
    }

    private void StartTurn()
    {
        if (!isGameActive) return;
        defenseActions.Clear();
        commonActions.Clear();
        offenseActions.Clear();

        Debug.Log($"Turn {currentTurn} started!");
        GameManager.Instance.SetState(GameState.GameStarted);
        OnTurnStart?.Invoke(currentTurn);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitActionServerRpc(ulong playerId, int actionId, Vector2Int targetTilePos)
    {
        if (!isGameActive) return;

        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(targetTilePos);
        int actionType = LoadDataManager.Instance.actionDataReader.GetActionDataById(actionId).type;
        ActionCategory category = CategorizeAction(actionType);

        PlayerAction newAction = new PlayerAction
        {
            playerId = playerId,
            actionId = actionId,
            targetTile = targetTile
        };

        switch (category)
        {
            case ActionCategory.Defense:
                defenseActions.Add(newAction);
                break;
            case ActionCategory.Common:
                commonActions.Add(newAction);
                break;
            case ActionCategory.Offense:
                offenseActions.Add(newAction);
                break;
        }

        Debug.Log($"Player {playerId} submitted {category} action: {actionId} at {targetTile.gridPosition}");
    }

    private ActionCategory CategorizeAction(int actionType)
    {
        switch (actionType)
        {
            case 2:
                return ActionCategory.Defense;

            case 1:
                return ActionCategory.Offense;

            default:
                return ActionCategory.Common;
        }
    }

    [ClientRpc]
    private void UpdateClientStateClientRpc(ulong playerId, int actionId, Vector2Int position)
    {
        Debug.Log($"Updating Client State: Player {playerId} executed action {actionId} at {position}");
    }

    public void ExecuteActions()
    {
        if (!IsServer) return;
        Debug.Log($"Executing all actions for Turn {currentTurn}");

        ExecuteActionList(defenseActions);
        ExecuteActionList(commonActions);
        ExecuteActionList(offenseActions);

        OnAllActionsSubmitted?.Invoke();
        StartCoroutine(EndTurn());
    }

    private void ExecuteActionList(List<PlayerAction> actionList)
    {
        foreach (var action in actionList)
        {
            PlayerCharacter player = GridManager.Instance.GetCharacterByNetworkId(action.playerId);
            IActionHandler handler = ActionHandlerFactory.CreateHandler((ActionType)action.actionId);
            Debug.Log(player.name);
            Debug.Log(action.actionId);
            handler.ExecuteAction(player, action.targetTile);
        }
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

    [Command]
    public void PrintAllActions()
    {
        Debug.Log("======= All Submitted Actions =======");

        void PrintActionList(string category, List<PlayerAction> actions)
        {
            Debug.Log($"--- {category} Actions ---");
            foreach (var action in actions)
            {
                Debug.Log($"Player {action.playerId}: Action {action.actionId} at {action.targetTile.gridPosition}");
            }
        }

        PrintActionList("Defense", defenseActions);
        PrintActionList("Common", commonActions);
        PrintActionList("Offense", offenseActions);

        Debug.Log("=====================================");
    }
}
