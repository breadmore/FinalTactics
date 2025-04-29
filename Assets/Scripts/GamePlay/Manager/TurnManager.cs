using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using System;
using QFSW.QC;
using Cysharp.Threading.Tasks;

public struct PlayerAction
{
    public ulong playerId;
    public int actionId;
    public Vector2Int targetTile;
    public int optionId;
}

public class TurnManager : NetworkSingleton<TurnManager>
{


    public int currentTurn;
    private int totalPlayers;

    private List<PlayerAction> defenseActions = new List<PlayerAction>();
    private List<PlayerAction> commonActions = new List<PlayerAction>();
    private List<PlayerAction> offenseActions = new List<PlayerAction>();

    public bool IsActiveGame = false;

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
        currentTurn = 0;
        
    }

    public void NextTurn()
    {
        if (!IsServer) return;

        currentTurn++;

        defenseActions.Clear();
        commonActions.Clear();
        offenseActions.Clear();

        StartTurnClientRpc(currentTurn);
    }
    [ClientRpc]
    private void StartTurnClientRpc(int turnNumber)
    {
        InGameUIManager.Instance.turnText.text = turnNumber.ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SubmitActionServerRpc(ulong playerId, int actionId, Vector2Int targetTilePos, int optionId)
    {
        ActionCategory actionCategory = LoadDataManager.Instance.actionDataReader.GetActionDataById(actionId).category;

        RemoveExistingAction(playerId);

        PlayerAction newAction = new PlayerAction
        {
            playerId = playerId,
            actionId = actionId,
            targetTile = targetTilePos,
            optionId = optionId
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

        Debug.Log($"Player {playerId} submitted {actionCategory} action: {actionId} , Option : {optionId} at {targetTilePos}");
    }

    private void RemoveExistingAction(ulong playerId)
    {
        Debug.Log("Remvoe action :" + playerId);
        defenseActions.RemoveAll(a => a.playerId == playerId);
        commonActions.RemoveAll(a => a.playerId == playerId);
        offenseActions.RemoveAll(a => a.playerId == playerId);
    }


    [ClientRpc]
    private void UpdateClientStateClientRpc(ulong playerId, int actionId, Vector2Int position)
    {
        Debug.Log($"Updating Client State: Player {playerId} executed action {actionId} at {position}");
    }

    public async UniTask ExecuteActions()
    {
        if (!IsServer) return;
        Debug.Log("액션 실행");
        await ExecuteActionsAsync();
    }

    private async UniTask ExecuteActionsAsync()
    {
        Debug.Log("액션 리스트 실행");
        GameManager.Instance.NotifyAlertClientRpc(GameConstants.ActionExcuteText);
        await ExecuteActionListAsync(defenseActions);
        await ExecuteActionListAsync(commonActions);
        await ExecuteActionListAsync(offenseActions);

        await EndTurnAsync();
    }

    private async UniTask ExecuteActionListAsync(List<PlayerAction> actionList)
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
            Vector2Int targetPos = action.targetTile;
            ActionData actionData = LoadDataManager.Instance.actionDataReader.GetActionDataById(action.actionId);
            int optionId = action.optionId;
            // 액션 카운트 증가
            GameManager.Instance.IncrementActionCountClientRpc(character.Team, actionData.actionType);

            IActionHandler handler = ActionHandlerFactory.CreateHandler(action);
            bool isMovementAction = actionData.actionType == ActionType.Move || actionData.actionType == ActionType.Dribble;

            if (isMovementAction && occupiedPositions.Contains(targetPos))
            {
                character.OnActionFailed();
            }
            else
            {
                handler.ExecuteAction(character, GridManager.Instance.GetGridTileAtPosition(action.targetTile));
                if (isMovementAction)
                    occupiedPositions.Add(targetPos);

            }

            await UniTask.Delay(TimeSpan.FromSeconds(GameConstants.ActionTime));
        }
    }
    private async UniTask EndTurnAsync()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));



        // 게임 종료 상태 확인 후 상태 전환
        if (!GameManager.Instance.IsGameFinished())
        {
            NotifyTurnEndClientRpc(currentTurn);
        }
    }

    [ClientRpc]
    private void NotifyTurnEndClientRpc(int turnNumber)
    {
        // Goal에 의해 이미 상태가 변경되었는지 확인
        if (GameManager.Instance._currentState is not GameResetState and not GameFinishedState)
        {
            //AlertManager.Instance.HideAlert();
            GameManager.Instance.ChangeState<InitGameState>();
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
                Debug.Log($"Player {action.playerId}: Action {action.actionId} at {action.targetTile}");
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
