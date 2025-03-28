using UnityEngine;
using Unity.Netcode;

public class PlayerCharacter : NetworkBehaviour
{
    public CharacterData CharacterData { get; private set; }
    public TeamName Team { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public bool HasBall { get; private set; }

    public void Initialize(CharacterData characterData, TeamName team, Vector2Int gridPosition)
    {
        CharacterData = characterData;
        Team = team;
        GridPosition = gridPosition;
    }

    public void Move(Vector2Int targetPosition)
    {
        GridPosition = targetPosition;
        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(GridPosition);
        transform.position = GridManager.Instance.GetNearestGridCenter(targetTile.transform.position);
    }

    public void MoveToGridTile(GridTile tile)
    {
        GridPosition = tile.gridPosition;
        transform.position = GridManager.Instance.GetNearestGridCenter(tile.transform.position);
    }

    // 임시 확률
    public void TryToBypassBlock()
    {
        // 블록을 회피할 수 있는 로직 구현
        bool success = Random.Range(0f, 1f) > 0.5f;  // 예시로 50% 확률로 회피 성공

        if (success)
        {
            Debug.Log($"{CharacterData.id} successfully bypassed the block!");
        }
        else
        {
            Debug.Log($"{CharacterData.id} failed to bypass the block.");
        }
    }


}
