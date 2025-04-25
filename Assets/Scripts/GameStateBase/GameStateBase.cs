using System.Collections;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.TextCore.Text;
public abstract class GameStateBase : IGameState
{
    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void UpdateState() { }

    public virtual void OnCharacterDataSelected(CharacterData characterData) { }
    public virtual void OnGridTileSelected(GridTile gridTile) { }
    public virtual void OnPlayerCharacterSelected(PlayerCharacter playerCharacter) { }
    public virtual void OnActionSelected(ActionData actionData) { }
    public virtual void OnActionOptionSelected(ActionOptionData actionOptionData) { }
    public virtual void HandleAllPlayersReady() { }
}
public class PlayerConnectionState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered PlayerConnectionState");
        InGameUIManager.Instance.CloseAllSlot();
        GameManager.Instance.DecideFirstAttack();
        BallManager.Instance.UpdateSpawnBallButtonState();
        EnterGame();
    }

    private void EnterGame()
    {
        GameManager.Instance.ChangeState<CharacterDataSelectionState>();

    }
}

public class CharacterDataSelectionState : GameStateBase
{
    public override void EnterState()
    {
        TurnManager.Instance.IsActiveGame = false;
        InGameUIManager.Instance.OpenSlotUI();
        Debug.Log("Entered CharacterSelectionState");
    }

    public override void OnCharacterDataSelected(CharacterData characterData)
    {

        GameManager.Instance.SetCharacterData(characterData);
        GameManager.Instance.ChangeState<CharacterDataSelectedState>();
        // 상태 변경 없이 데이터만 설정
        // 상태 변경은 실제 그리드 타일 선택 시로 이동
    }

    public override void ExitState()
    {
        // UI 비활성화를 여기서 하지 않음
    }
}

public class CharacterDataSelectedState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered CharacterDataSelectedState");
    }

    public override void OnGridTileSelected(GridTile gridTile)
    {
        GameManager.Instance.SetGridTile(gridTile);
    }
}
public class CharacterPlacementCompleteState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered CharacterPlacementCompleteState");
        CheckAttackTeam();
    }

    private void CheckAttackTeam()
    {
        if(GameManager.Instance.AttackingTeam != GameManager.Instance.thisPlayerBrain.GetMyTeam())
        {
            GameManager.Instance.ChangeState<ReadyCheckState>();
        }
    }

}
public class BallPlacementState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered BallPlacementState");
    }

    public override void UpdateState()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Grid"))
            {
                var tile = hit.collider.GetComponent<GridTile>();
                GameManager.Instance.OnGridTileSelected(tile);
                BallManager.Instance.SpawnBall(tile);
            }
        }
    }

    public override void OnGridTileSelected(GridTile gridTile)
    {
        GameManager.Instance.SetGridTile(gridTile);
        GameManager.Instance.ChangeState(new ReadyCheckState());
    }
}


public class ReadyCheckState : GameStateBase
{

    public override void EnterState()
    {
        Debug.Log("Entered ReadyCheckState");
    }

    public override void HandleAllPlayersReady()
    {

        GameManager.Instance.StartNewTurn();

    }
}


public class InitGameState : GameStateBase
{
    public override void EnterState()
    {
        TurnManager.Instance.IsActiveGame = true;
        GridManager.Instance.TurnStartSetting();
        BallManager.Instance.SetBallOwner();
        TurnManager.Instance.NextTurn();
        GameManager.Instance.ClearAllSelected();

        Debug.Log($"Turn {TurnManager.Instance.currentTurn} started");
        GameManager.Instance.ChangeState<PlayerActionDecisionState>();
    }
}

public class PlayerActionDecisionState : GameStateBase
{
    public override void EnterState()
    {
        InGameUIManager.Instance.CloseAllSlot();
    }
    public override void OnPlayerCharacterSelected(PlayerCharacter playerCharacter)
    {
        // 데이터 오픈

        InGameUIManager.Instance.ActionSlot.RefreshActionSlots(BallManager.Instance.IsBallOwnedBy(GameManager.Instance.SelectedPlayerCharacter));
        if (playerCharacter.OwnerClientId == GameManager.Instance.thisPlayerBrain.OwnerClientId)
        {
            GameManager.Instance.SetPlayerCharacter(playerCharacter);
            GameManager.Instance.ChangeState(new CharacterControlState());
        }
        else
        {
            InGameUIManager.Instance.CloseAllSlot();
        }
    }

    public override void HandleAllPlayersReady()
    {
        GameManager.Instance.StartAction();
    }
}

public class CharacterControlState : GameStateBase
{

    public override void EnterState()
    {
        InGameUIManager.Instance.OpenSlotUI();
        Debug.Log("Entered CharacterControlState");
    }

    public override void OnActionSelected(ActionData actionData)
    {
        GameManager.Instance.SetActionData(actionData.id);
        if (!actionData.hasOption)
            GameManager.Instance.ChangeState<ActionDataSelectedState>();
        else
        {
            GameManager.Instance.ChangeState<ActionOptionSelecteState>();
        }
    }



}


public class ActionDataSelectedState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered ActionDataSelectedState");
    }

    public override void OnGridTileSelected(GridTile gridTile)
    {
        GameManager.Instance.SetGridTile(gridTile);
        ActionPreviewManager.Instance.ClearHighlights();
        InGameUIManager.Instance.CloseAllSlot();
    }
}


public class ActionOptionSelecteState : GameStateBase
{
    private int _currentActionId;

    public override void EnterState()
    {
        _currentActionId = GameManager.Instance.SelectedActionData;
        InGameUIManager.Instance.OpenSlotUI();
        Debug.Log($"Entered ActionOption selection for action ID: {_currentActionId}");
    }

    public override void ExitState()
    {
        InGameUIManager.Instance.CloseAllSlot();
    }
    public override void OnGridTileSelected(GridTile gridTile)
    {
        GameManager.Instance.SetGridTile(gridTile);
        ActionPreviewManager.Instance.ClearHighlights();
        InGameUIManager.Instance.CloseAllSlot();
    }

    public override void OnActionOptionSelected(ActionOptionData actionOptionData)
    {
        if (actionOptionData == null)
        {
            Debug.LogError("Invalid action option data!");
            return;
        }
        GameManager.Instance.SetActionOptionData(actionOptionData.id);

        switch (actionOptionData.id)
        {
            case 1:
                ExecuteCharge(actionOptionData);
                break;

            case 2:
                ExecuteShoot();
                break;

            case 0:
                CancelSelection();
                break;

            default:
                Debug.LogError($"Unknown option type: {actionOptionData.name}");
                break;
        }
    }

    private void ExecuteCharge(ActionOptionData option)
    {
        // 즉시 실행되는 액션 (예: 차지 슛)
        InGameUIManager.Instance.CloseAllSlot();

        // 서버에 액션 전송
        TurnManager.Instance.SubmitActionServerRpc(
            GameManager.Instance.SelectedPlayerCharacter.NetworkObjectId,
            option.actionId,
            GameManager.Instance.SelectedPlayerCharacter.GridPosition,
            option.id // 옵션 ID 추가 전송
        );
        GameManager.Instance.ChangeState<PlayerActionDecisionState>();
    }

    private void ExecuteShoot()
    {
        // 타겟 선택이 필요한 액션 (예: 일반 슛)
        GameManager.Instance.SetActionData(2);
    }

    private void CancelSelection()
    {
        // 옵션 선택 취소
        InGameUIManager.Instance.CloseAllSlot();
        GameManager.Instance.ChangeState<CharacterControlState>();
    }



}



public class GridTileSelectedState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered GridTileSelectedState");
    }
}

public class GameFinishedState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Game finished");
        InGameUIManager.Instance.resultPanel.ShowResult();
    }
}

public class ActionExecutionState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered ActionExcutionState");
        TurnManager.Instance.ExecuteActions().Forget();
    }

    public override void UpdateState()
    {
        // 액션 실행 완료 대기
        // TurnManager에서 ClientRpc로 완료 알림
    }

    public override void ExitState()
    {
        Debug.Log("End Action!");
    }


}

public class GameResetState : GameStateBase
{
    public override void EnterState()
    {
        Debug.Log("Entered GameResetState");
        ResetAsync().Forget();
    }

    private async UniTaskVoid ResetAsync()
    {
        await UniTask.Delay(2000); // 2초 대기

        PlayerCharacterNetworkPool.Instance.ReturnAllCharacters();
        BallManager.Instance.DespawnBall();
        GameManager.Instance.ResetAllPlayersReadyState();
        GridManager.Instance.ResetAllGridTile();


        ResetEnd();
    }

    private void ResetEnd()
    {
        GameManager.Instance.ResetGameClientRpc();
    }
}
