using Unity.VisualScripting;
using UnityEngine;

public interface IActionHandler
{
    bool CanExecute(PlayerCharacter player, GridTile targetTile);  // ���� ���� ���� �Ǵ�
    void ExecuteAction(PlayerCharacter player, GridTile targetTile);  // �׼� ����
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
            // ��� �������
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
            Debug.Log($"{player.CharacterData.id}�� �帮���� ���ܵǾ����ϴ�!");
            // ���� ó�� ���� (�� ���� or ���ڸ� ����)
            // ����: ���� ����߸���
            player.MoveToGridTile(targetTile);
            return;
        }

        // �帮�� ���� ��
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
        float probability;
        if (player.CharacterData.characterStat.type == 0)
        {
            probability = Random.Range(0f, 5f);
        }
        else
        {
            probability = Random.Range(0.5f, 1f);
        }
            
        targetTile.BlockProbabilityDecision(probability);
    }
}


public class TackleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        int distance = GridUtils.GetDistance(player.GridPosition, targetTile.gridPosition);

        return distance <= GameConstants.TackleDistance;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot tackle.");
            return;
        }

        var opponent = targetTile.occupyingCharacter;
        BallManager.Instance.StealBall(player);
        Debug.Log($"{player.CharacterData.id} tackled and took the ball from {opponent.CharacterData.id}.");
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
        // ��Ű�� üũ
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
