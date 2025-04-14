using UnityEngine;
using Unity.Netcode;
using UnityEngine.Splines;

public class PlayerCharacter : NetworkBehaviour
{
    public CharacterData CharacterData { get; private set; }
    public TeamName Team { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public int ShootChargeCount { get; private set; } = 0;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton?.IsServer == true)
        {
            ObjectPool.Instance?.RegisterActiveCharacter(this);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton?.IsServer == true)
        {
            ObjectPool.Instance?.UnregisterActiveCharacter(this);
        }
    }

    public void InitData(CharacterData characterData)
    {
        CharacterData = characterData;
    }
    public void Initialize(TeamName team, Vector2Int gridPosition)
    {
        Team = team;
        GridPosition = gridPosition;
    }

    public void MoveToGridTile(GridTile tile)
    {
        GridTile currentTile = GridManager.Instance.GetGridTileAtPosition(GridPosition);
        currentTile.ClearOccupied();

        GridTile newTile = GridManager.Instance.GetGridTileAtPosition(tile.gridPosition);
        newTile.SetOccupied(this);

        GridPosition = tile.gridPosition;
        transform.position = GridManager.Instance.GetNearestGridCenter(newTile.transform.position);

        SyncGridTileClientRpc(currentTile.gridPosition, tile.gridPosition);
    }


    // 서버 호출용
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
    public void ChargeShoot()
    {
        if (ShootChargeCount < 3)
            ShootChargeCount++;
    }

    public void ResetShootCharge()
    {
        ShootChargeCount = 0;
    }

    public void ResetPlayerCharacter()
    {
        ResetShootCharge();
    }

}
