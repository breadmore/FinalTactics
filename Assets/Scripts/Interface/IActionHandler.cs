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
        if (targetTile.isOccupied)
            return false;

        int moveRange = player.CharacterData.characterStat.speed;
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

        player.MoveToGridTile(targetTile);
    }
}
public class PassActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        if (!BallManager.Instance.IsBallOwnedBy(player)) return false;

        int moveRange = player.CharacterData.characterStat.pass;
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

        BallManager.Instance.PassBall(player, targetTile);
    }
}
public class DribbleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        if (!BallManager.Instance.IsBallOwnedBy(player)) return false;
        if (targetTile.isOccupied)
        {
            // 사람 있을경우
            return false;
        }
        else
        {
            return true;
        }
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
            Debug.Log($"{player.CharacterData.id}의 드리블이 차단되었습니다!");
            // 실패 처리 로직 (볼 뺏김 or 제자리 유지)
            // 예시: 공만 떨어뜨리기
            player.MoveToGridTile(targetTile);
            return;
        }

        // 드리블 성공 시
        player.MoveToGridTile(targetTile);
        BallManager.Instance.MoveBall(targetTile);
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
        float probability = GridUtils.GetBlockSuccessProbability(player);
  
        targetTile.BlockProbabilityDecision(probability);
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

            // 위치 변경
            player.MoveToGridTile(targetTile);
            opponent.MoveToGridTile(playerTile);

            // 공 탈취 + 소유권 이동
            BallManager.Instance.StealBall(player);
            Debug.Log($"{player.CharacterData.id} 성공적으로 태클하여 {opponent.CharacterData.id}에게서 공을 빼앗았습니다!");
        }
        else
        {
            // 실패 시: 경고 메시지 or 패널티
            Debug.Log($"{player.CharacterData.id}의 태클이 실패했습니다!");
            // 예: 이동 불가 상태로 만들거나 경고 등
        }
    }
}
public class ShootActionHandler : IActionHandler
{
    public ShootOption SelectedOption { get; set; } = GameManager.Instance.SelectedShootOption;

    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        if (!BallManager.Instance.IsBallOwnedBy(player)
            || targetTile.Type != TileType.GoalkeeperZone)
        {
            return false;
        }

        int moveRange = player.ShootChargeCount;
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= moveRange;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        switch (SelectedOption)
        {
            case ShootOption.Cancel:
                Debug.Log("Shoot canceled. Choose another action.");
                break;

            case ShootOption.Charge:
                player.ChargeShoot();
                Debug.Log($"Charging... current level: {player.ShootChargeCount}");
                break;

            case ShootOption.Shoot:
                if (!CanExecute(player, targetTile))
                {
                    Debug.LogWarning("Can't Shooting");
                    return;
                }
                int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);
                
                //float successRate = GridUtils.GetShootSuccessProbability(player, distance);
                float successRate = 1f;
                float roll = Random.Range(0f, 1f);
                Debug.Log($"[Shoot] SuccessChance: {successRate}, Rolled: {roll}");

                if (roll < successRate)
                {
                    // 골 처리
                    BallManager.Instance.MoveBall(targetTile);
                    GameManager.Instance.Goal(player.Team);
                }
                else
                {
                    Debug.Log("Missed...");
                    // 미스 처리 (볼 이동 or 상대에게?)
                }

                player.ResetShootCharge();
                break;
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
        Debug.Log($"{player.CharacterData.id} intercepted the ball at {targetTile.gridPosition}.");
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

        Debug.Log($"{player.CharacterData.id} saved the shot at {targetTile.gridPosition}!");
    }
}
