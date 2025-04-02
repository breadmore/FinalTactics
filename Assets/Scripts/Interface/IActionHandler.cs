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
        return targetTile.isOccupied == false;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot move.");
            return;
        }

        player.MoveToGridTile(targetTile);
        Debug.Log($"{player.CharacterData.id} moves to {targetTile.gridPosition}.");
    }
}
public class PassActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        return player.HasBall; // 공을 가진 경우만 패스 가능
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot pass the ball.");
            return;
        }

        BallManager.Instance.PassBall(player, targetTile);
        Debug.Log($"{player.CharacterData.id} passes the ball to {targetTile.gridPosition}.");
    }
}
public class DribbleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        return BallManager.Instance.IsBallOwnedBy(player) && targetTile.isOccupied == false;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Cannot dribble.");
            return;
        }

        player.MoveToGridTile(targetTile);
        BallManager.Instance.MoveBall(targetTile);
        Debug.Log($"{player.CharacterData.id} dribbles to {targetTile.gridPosition}.");
    }
}

public class BlockActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        return BallManager.Instance.IsBallOwnedBy(player) && targetTile.isOccupied == false;
    }

    public void ExecuteAction(PlayerCharacter player, GridTile targetTile)
    {
        if (!CanExecute(player, targetTile))
        {
            Debug.LogWarning("Block failed.");
            return;
        }

        Debug.Log($"{player.CharacterData.id} attempts to block the opponent at {targetTile.gridPosition}.");
        targetTile.occupyingCharacter.TryToBypassBlock();
    }
}


public class TackleActionHandler : IActionHandler
{
    public bool CanExecute(PlayerCharacter player, GridTile targetTile)
    {
        return BallManager.Instance.IsBallOwnedBy(player) && targetTile.isOccupied == false;
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
