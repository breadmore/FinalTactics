using System;
using Unity.VisualScripting;
using UnityEngine;

public interface IActionHandler
{
    bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1);  // 실행 가능 여부 판단
    void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1);  // 액션 실행
    
}

public static class ActionHandlerFactory
{
    public static IActionHandler CreateHandler(PlayerAction playerAction)
    {
        // actionId로부터 ActionData 먼저 가져오기
        ActionData actionData = LoadDataManager.Instance.actionDataReader.GetActionDataById(playerAction.actionId);

        // actionType에 따라 핸들러 생성
        IActionHandler handler = actionData.actionType switch
        {
            ActionType.Move => new MoveActionHandler(),
            ActionType.Pass => new PassActionHandler(),
            ActionType.Dribble => new DribbleActionHandler(),
            ActionType.Block => new BlockActionHandler(),
            ActionType.Shoot => new ShootActionHandler(),
            ActionType.Tackle => new TackleActionHandler(),
            ActionType.Intercept => new InterceptActionHandler(),
            ActionType.Save => new SaveActionHandler(),
            _ => null
        };

        if (handler is IOptionHandler optionHandler && playerAction.optionId >= 0)
        {
            var optionData = LoadDataManager.Instance.actionOptionDataReader.GetActionOptionById(playerAction.optionId);
            optionHandler.SetOptionData(optionData);
        }

        return handler;
    }
}

public interface IOptionHandler
{
    void SetOptionData(ActionOptionData optionData);
}
public class MoveActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (targetTile.IsOccupied())
            return false;

        int moveRange = Mathf.Min(player.CharacterStat.speed, player.Stamina);
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot move.");
            return;
        }

        player.PlayAnimationMove();
        player.MoveToGridTile(targetTile);
    }
}
public class PassActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!BallManager.Instance.IsBallOwnedBy(player)) return false;

        int moveRange = player.CharacterStat.pass;
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot pass the ball.");
            return;
        }

        player.PlayAnimationPass();
        BallManager.Instance.MoveBall(targetTile);
    }
}
public class DribbleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (targetTile.IsOccupied() || !BallManager.Instance.IsBallOwnedBy(player))
        {
            return false;
        }

            int moveRange = Mathf.Min(player.CharacterStat.speed, player.Stamina);
            int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);
            return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot dribble.");
            return;
        }
        float randomValue = UnityEngine.Random.Range(0f, 1f);

        if (randomValue < targetTile.BlockProbability)
        {
            Debug.Log($"{player.GetCharacterId()}의 드리블이 블록으로 차단되었습니다!");
            // 실패 처리 로직 (볼 뺏김 or 제자리 유지)
            player.PlayAnimationTrip();
            player.MoveToGridTile(targetTile);

            BallManager.Instance.MoveBall(GridManager.Instance.GetGridTileAtPosition(targetTile.blockCharacter.GridPosition));
            return;
        }
        

        // 드리블 성공 시
        player.PlayAnimationDribble();
        player.MoveToGridTile(targetTile);
        BallManager.Instance.DribbleBall(targetTile,player);
    }
}

public class BlockActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        return GridUtils.IsStraightLineInRange(player.GridPosition, targetTile.gridPosition, GameConstants.BlockDistance);
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Block failed.");
            return;
        }

        player.PlayAnimationBlock();

        float probability = GridUtils.GetBlockSuccessProbability(player);
  
        targetTile.BlockProbabilityDecision(probability, player);
    }
}


public class TackleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);
        var opponent = targetTile.occupyingCharacter;

        if (opponent == null) return false;
        if (!BallManager.Instance.IsBallOwnedBy(opponent)) return false;

        return distance <= GameConstants.TackleDistance;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot tackle.");
            return;
        }

        var opponent = targetTile.occupyingCharacter;
        float successRate = GridUtils.GetTackleSuccessProbability(player, opponent);  // 능력 기반 확률

        float roll = UnityEngine.Random.Range(0f, 1f);
        Debug.Log($"[Tackle] SuccessChance: {successRate}, Rolled: {roll}");


        if (roll < successRate)
        {
            // 성공 시
            Debug.Log($"{player.NetworkObjectId}가 성공적으로 태클하여 {opponent.NetworkObjectId}에게서 공을 빼앗았습니다!");
            
            // 애니메이션 재생
            player.PlayAnimationTackle();
            opponent.PlayAnimationTrip();



            // 플레이어 위치 변경
            var (newTacklerPos, newTargetPos) = GridUtils.TryGetTacklePositions(
                player.GridPosition,
                targetTile.gridPosition,
                GridManager.Instance
            );
            player.MoveToGridTile(GridManager.Instance.GetGridTileAtPosition(newTacklerPos));
            opponent.MoveToGridTile(GridManager.Instance.GetGridTileAtPosition(newTargetPos));

            // 볼 위치 변경
            BallManager.Instance.MoveBall(GridManager.Instance.GetGridTileAtPosition(player.GridPosition));
        }
        else
        {
            // 실패 시: 경고 메시지 or 패널티
            player.PlayAnimationTrip();
            Debug.Log($"{player.NetworkObjectId}의 태클이 실패했습니다!");
            // 예: 이동 불가 상태로 만들거나 경고 등
        }
    }
}
public class ShootActionHandler : IActionHandler, IOptionHandler
{
    private ActionOptionData _optionData;
    public void SetOptionData(ActionOptionData optionData)
    {
        _optionData = optionData;
    }

    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        // 기본 조건 검사
        if (!BallManager.Instance.IsBallOwnedBy(player) ||
            targetTile.Type != TileType.GoalkeeperZone)
        {
            return false;
        }

        // 차지 레벨에 따른 사정거리
        int maxDistance = 1 + player.ShootChargeCount;
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= maxDistance;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {


        switch (_optionData.name)
        {
            case "Cancel":
                HandleCancel(player);
                break;

            case "Charge":
                HandleCharge(player);
                break;

            case "Shoot":
                HandleShoot(player, targetTile);
                break;

            default:
                Debug.LogError($"Unknown shoot option: {_optionData.name}");
                break;
        }
    }

    private void HandleCancel(PlayerCharacter player)
    {
        player.ResetShootCharge();
        Debug.Log("Shoot canceled. Choose another action.");
        GameManager.Instance.ChangeState<CharacterControlState>();
    }

    private void HandleCharge(PlayerCharacter player)
    {
        player.ChargeShoot();
        Debug.Log($"Charging... Current level: {player.ShootChargeCount}");
    }

    private void HandleShoot(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Can't shoot to this position");
            return;
        }

        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);
        var shootResult = GridUtils.GetShootSuccessProbability(
            player,
            distance,
            player.ShootChargeCount
        );

        Debug.Log($"[Shoot] Chance: {shootResult.successRate:P0}, " +
                 $"Critical: {shootResult.isCritical}, " +
                 $"Charge: {player.ShootChargeCount}");

        player.ResetShootCharge();

        if (shootResult.IsSuccess())
        {
            player.PlayAnimationShoot();
            BallManager.Instance.MoveBall(targetTile);
            GameManager.Instance.Goal(player.Team);

            if (shootResult.isCritical)
            {
                // 크리티컬 추가 효과
                //CameraManager.Instance.PlayScreenShake(0.3f);
            }
        }
        else
        {
            player.PlayAnimationTrip();
            Debug.Log("Shot missed...");
            // 미스 처리 - 볼을 랜덤한 근처 타일로 이동
            //BallManager.Instance.MoveToRandomAdjacentTile(targetTile);
        }
    }
}

public class InterceptActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        return BallManager.Instance.IsBallAtTile(targetTile);
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot intercept.");
            return;
        }

        
        Debug.Log($"{player.GetCharacterId()} intercepted the ball at {targetTile.gridPosition}.");
    }
}
public class SaveActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        // 골키퍼 체크
        //return player.IsGoalkeeper && BallManager.Instance.IsBallAtTile(targetTile);
        return false;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile, int optionId = -1)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot save the shot.");
            return;
        }

        Debug.Log($"{player.GetCharacterId()} saved the shot at {targetTile.gridPosition}!");
    }
}
