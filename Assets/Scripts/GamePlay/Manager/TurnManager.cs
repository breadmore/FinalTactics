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

    public event Action OnTurnStart;
    public event Action OnAllActionsSubmitted;
    public event Action OnTurnEnd;

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
        InGameUIManager.Instance.turnText.text = currentTurn.ToString();

        OnTurnStart?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitActionServerRpc(ulong playerId, int actionId, Vector2Int targetTilePos)
    {
        if (!isGameActive) return;

        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(targetTilePos);
        ActionCategory actionCategory = LoadDataManager.Instance.actionDataReader.GetActionDataById(actionId).category;

        PlayerAction newAction = new PlayerAction
        {
            playerId = playerId,
            actionId = actionId,
            targetTile = targetTile
        };

        switch (actionCategory)
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

        Debug.Log($"Player {playerId} submitted {actionCategory} action: {actionId} at {targetTile.gridPosition}");
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
        actionList.Sort((a, b) =>
        {
            var charA = GridManager.Instance.GetCharacterByNetworkId(a.playerId);
            var charB = GridManager.Instance.GetCharacterByNetworkId(b.playerId);

            int result = charB.CharacterStat.speed.CompareTo(charA.CharacterStat.speed);
            if (result == 0)
                result = UnityEngine.Random.Range(-1, 2);
            return result;
        });

        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        foreach (var action in actionList)
        {
            PlayerCharacter character = GridManager.Instance.GetCharacterByNetworkId(action.playerId);
            Vector2Int targetPos = action.targetTile.gridPosition;
            ActionType actionType = (ActionType)action.actionId;

            // 액션 핸들러 생성
            IActionHandler handler = ActionHandlerFactory.CreateHandler(actionType);

            // 이동형 액션인지 확인
            bool isMovementAction = actionType == ActionType.Move || actionType == ActionType.Dribble;

            // 이동형이고 타일이 이미 점유되어 있다면 실패 처리
            if (isMovementAction && occupiedPositions.Contains(targetPos))
            {
                Debug.Log($"[TurnManager] {actionType} 실패: Player {action.playerId} - {targetPos}는 이미 점유됨.");
                character.OnActionFailed();
                continue;
            }

            handler.ExecuteAction(character, action.targetTile);

            if (isMovementAction)
            {
                occupiedPositions.Add(targetPos);
            }
        }
    }


    private IEnumerator EndTurn()
    {
        yield return new WaitForSeconds(1f);
        OnTurnEnd?.Invoke();

        NotifyTurnEndClientRpc(currentTurn);
        if (isGameActive)
        {
            currentTurn++;
            StartTurn();
        }
    }

    [ClientRpc]
    private void NotifyTurnEndClientRpc(int turnNumber)
    {
        Debug.Log($"[Client] Turn {turnNumber} ended.");
        GameManager.Instance.SetState(GameState.GameStarted);  // 또는 다음 상태로
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


    [Command]
    public void ShowCurrentTurn()
    {
        Debug.Log(currentTurn);
    }
}
