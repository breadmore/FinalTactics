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
        UpdateGridTileServerRpc(GridPosition, tile.gridPosition);
        GridPosition = tile.gridPosition;
        transform.position = GridManager.Instance.GetNearestGridCenter(tile.transform.position);
    }

    [ServerRpc]
    private void UpdateGridTileServerRpc(Vector2Int currentTilePosition,Vector2Int targetTilePosition)
    {
        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(currentTilePosition);
        currentTile.ClearOccupied();

        GridTile targetTile = GridManager.Instance.GetGridTileAtPosition(targetTilePosition);
        targetTile.SetOccupied(this);
        SyncGridTileClientRpc(currentTilePosition,targetTilePosition);
    }

    [ClientRpc]
    private void SyncGridTileClientRpc(Vector2Int currentTilePosition, Vector2Int tilePosition)
    {
        if (IsHost) return; // 서버에서는 실행하지 않음

        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(currentTilePosition);
        currentTile.ClearOccupied();

        GridTile gridTile = GridManager.Instance.GetGridTileAtPosition(tilePosition);
        gridTile.SetOccupied(this);
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
