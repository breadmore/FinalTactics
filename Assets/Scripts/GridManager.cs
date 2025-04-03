using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public class GridManager : Singleton<GridManager>
{
    private Grid grid;

    [SerializeField] private List<GridTile> gridTileList = new List<GridTile>();

    protected override void Awake()
    {
        grid = GetComponent<TilemapScaler>().tilemapGrid;
    }

    void Start()
    {
        InitializeGrid();
        StartTileTypeChange();
    }

    public void InitializeGrid()
    {
        if (gridTileList.Count != GameConstants.GRID_SIZE.x * GameConstants.GRID_SIZE.y)
        {
            Debug.LogWarning("gridTileList의 크기가 GRID_SIZE와 일치하지 않음!");
            return;
        }

        int count = 0;
        for (int x = 0; x < GameConstants.GRID_SIZE.x; x++)
        {
            for (int z = 0; z < GameConstants.GRID_SIZE.y; z++)
            {
                Vector2Int position = new Vector2Int(x, z);
                gridTileList[count].SetGridPosition(position);
                count++;
            }
        }
    }

    public async void StartTileTypeChange()
    {
        Player player = LobbyManager.Instance.FindPlayerById(AuthenticationService.Instance.PlayerId);
        TeamName teamName = await GameManager.Instance.GetTeamNameAsync(player);

        foreach (var gridTile in gridTileList)
        {
            Vector2Int position = gridTile.gridPosition;

            if ((position.x == 0 || position.x == GameConstants.GRID_SIZE.x - 1) && (position.y == 3 || position.y == 4))
            {
                gridTile.SetTileType(TileType.GoalkeeperZone);
            }
            else if ((teamName == TeamName.TeamA && position.x < 7) ||
                     (teamName == TeamName.TeamB && position.x >= 7))
            {
                gridTile.SetTileType(TileType.SpawnZone);
            }
        }
    }

    public void TileTypeChangeWithPosition(Vector2Int cellPosition, TileType type)
    {
        GridTile targetTile = gridTileList.Find(tile => tile.gridPosition == cellPosition);
        if (targetTile != null)
        {
            targetTile.SetTileType(type);
        }
        else
        {
            Debug.LogWarning($"GridTile not found at position {cellPosition}");
        }
    }

    public Vector3 GetNearestGridCenter(Vector3 worldPosition)
    {
        if (grid == null)
        {
            Debug.LogError("Grid is not assigned!");
            return worldPosition;
        }

        Vector3Int cellPosition = grid.WorldToCell(worldPosition);
        return grid.GetCellCenterWorld(cellPosition);
    }

    public GridTile GetGridTileAtPosition(Vector2Int position)
    {
        return gridTileList.Find(tile => tile.gridPosition == position);
    }

    public PlayerCharacter GetCharacterByNetworkId(ulong networkId)
    {
        foreach (var tile in gridTileList) // gridTiles는 현재 게임 내 모든 GridTile을 관리하는 리스트
        {
            if (tile.isOccupied && tile.occupyingCharacter != null && tile.occupyingCharacter.NetworkObjectId == networkId)
            {
                return tile.occupyingCharacter;
            }
        }
        return null; // 해당하는 캐릭터가 없을 경우 null 반환
    }

    public Grid GetGrid()
    {
        return grid;
    }
}
