using Unity.VisualScripting;
using UnityEngine;

public interface IActionHandler
{
    bool CanExecute(PlayerCharacter player, GridTile targetTile);  // 실행 가능 여부 판단
    void ExecuteAction(PlayerCharacter player, GridTile targetTile);  // 액션 실행
}

public static class ActionHandlerFactory
{
    public static IActionHandler CreateHandler(ActionType actionType)
    {
        return actionType switch
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
    }
}
public class MoveActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        if (targetTile.IsOccupied)
            return false;

        int moveRange = Mathf.Min(player.CharacterStat.speed, player.Stamina);
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
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
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        if (!BallManager.Instance.IsBallOwnedBy(player)) return false;

        int moveRange = player.CharacterStat.pass;
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot pass the ball.");
            return;
        }

        player.PlayAnimationPass();
        BallManager.Instance.PassBall(player, targetTile);
    }
}
public class DribbleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        if (targetTile.IsOccupied || !BallManager.Instance.IsBallOwnedBy(player))
        {
            // 사람 있을경우
            return false;
        }

            int moveRange = Mathf.Min(player.CharacterStat.speed, player.Stamina);
            int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);
            return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot dribble.");
            return;
        }
        float randomValue = Random.Range(0f, 1f);

        if (randomValue < targetTile.BlockProbability)
        {
            Debug.Log($"{player.GetCharacterId()}의 드리블이 블록으로 차단되었습니다!");
            // 실패 처리 로직 (볼 뺏김 or 제자리 유지)
            player.PlayAnimationTrip();
            player.MoveToGridTile(targetTile);
            BallManager.Instance.StealBall(targetTile.blockCharacter);
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
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        return GridUtils.IsStraightLineInRange(player.GridPosition, targetTile.gridPosition, GameConstants.BlockDistance);
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
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
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);
        var opponent = targetTile.occupyingCharacter;

        return distance <= GameConstants.TackleDistance
               && opponent != null
               && BallManager.Instance.IsBallOwnedBy(opponent);
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot tackle.");
            return;
        }

        var opponent = targetTile.occupyingCharacter;
        float successRate = GridUtils.GetTackleSuccessProbability(player, opponent);  // 능력 기반 확률

        float roll = Random.Range(0f, 1f);
        Debug.Log($"[Tackle] SuccessChance: {successRate}, Rolled: {roll}");



        if (roll < successRate)
        {
            // 내 타일
            var playerTile = GridManager.Instance.GetGridTileAtPosition(player.GridPosition);

            player.PlayAnimationTackle();

            // 위치 변경
            player.MoveToGridTile(targetTile);
            // 공 탈취 + 소유권 이동
            BallManager.Instance.StealBall(player);
            Debug.Log($"{player.GetCharacterId()} 성공적으로 태클하여 {opponent.GetCharacterId()}에게서 공을 빼앗았습니다!");
        }
        else
        {
            player.PlayAnimationTrip();
            // 실패 시: 경고 메시지 or 패널티
            Debug.Log($"{player.GetCharacterId()}의 태클이 실패했습니다!");
            // 예: 이동 불가 상태로 만들거나 경고 등
        }
    }
}
public class ShootActionHandler : IActionHandler
{
    private ActionOptionData _option;

    public ShootActionHandler()
    {
        _option = LoadDataManager.Instance.actionOptionDataReader
            .GetActionOptionById(GameManager.Instance.SelectedActionOptionData);
    }

    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
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

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        switch (_option.name)
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
                Debug.LogError($"Unknown shoot option: {_option.name}");
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

    private void HandleShoot(PlayerCharacter player, GridTile targetTile)
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
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        return BallManager.Instance.IsBallAtTile(targetTile);
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot intercept.");
            return;
        }

        BallManager.Instance.StealBall(player);
        Debug.Log($"{player.GetCharacterId()} intercepted the ball at {targetTile.gridPosition}.");
    }
}
public class SaveActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        // 골키퍼 체크
        //return player.IsGoalkeeper && BallManager.Instance.IsBallAtTile(targetTile);
        return false;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot save the shot.");
            return;
        }

        Debug.Log($"{player.GetCharacterId()} saved the shot at {targetTile.gridPosition}!");
    }
}
