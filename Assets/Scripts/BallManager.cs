using UnityEngine;

public class BallManager : MonoBehaviour
{
    public static BallManager Instance { get; private set; }

    private GridTile currentTile;  // 공의 현재 위치
    private PlayerCharacter ballOwner;  // 공을 가진 플레이어

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetBallOwner(PlayerCharacter player)
    {
        ballOwner = player;
        currentTile = GridManager.Instance.GetGridTileAtPosition(player.GridPosition); // 캐릭터 위치로 공 이동
        Debug.Log($"Ball is now owned by {player.CharacterData.id}");
    }

    public void MoveBall(GridTile targetTile)
    {
        currentTile = targetTile;
        ballOwner = null; // 소유자가 없음 (공만 이동)

        Debug.Log($"Ball moved to {targetTile.gridPosition}");
    }

    public void PassBall(PlayerCharacter passer, GridTile targetTile)
    {
        if (ballOwner != passer)
        {
            Debug.LogWarning("Player does not have the ball!");
            return;
        }

        MoveBall(targetTile);
        Debug.Log($"{passer.CharacterData.id} passed the ball to {targetTile.gridPosition}");
    }

    public void StealBall(PlayerCharacter stealer)
    {
        ballOwner = stealer;
        currentTile = GridManager.Instance.GetGridTileAtPosition(stealer.GridPosition);
        Debug.Log($"{stealer.CharacterData.id} stole the ball!");
    }

    public bool IsBallAtTile(GridTile tile)
    {
        return currentTile == tile;
    }

    public bool IsBallOwnedBy(PlayerCharacter player)
    {
        return ballOwner == player;
    }

    //public bool IsBallHeadingTowardsGoal()
    //{
    //    return currentTile.IsGoalTile();
    //}
}
