using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    private Grid grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private List<GridTile> gridTileList = new List<GridTile>();
    private Dictionary<Vector2Int, GridTile> gridTileDic = new Dictionary<Vector2Int, GridTile>();

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
        gridTileDic.Clear();  // 이미 존재하는 데이터는 제거

        int count = 0;
        for (int x = 0; x < GameConstants.GRID_SIZE.x; x++)
        {
            for (int z = 0; z < GameConstants.GRID_SIZE.y; z++)
            {
                if (count < gridTileList.Count)
                {
                    gridTileDic[new Vector2Int(x, z)] = gridTileList[count];
                    count++;
                }
                else
                {
                    Debug.LogWarning($"gridTileList의 항목 수가 {x}, {z}에 맞지 않음");
                    return;
                }
            }
        }
    }

    public async void StartTileTypeChange()
    {
        Player player = LobbyManager.Instance.FindPlayerById(AuthenticationService.Instance.PlayerId);
        TeamName teamName = await GameManager.Instance.GetTeamNameAsync(player);
        // gridTileDic에서 모든 GridTile을 순회하며 조건에 맞게 TileType 변경
        foreach (var kvp in gridTileDic)
        {
            Vector2Int position = new Vector2Int((int)kvp.Key.x, (int)kvp.Key.y);
            GridTile gridTile = kvp.Value;

            // x가 0이거나 GRID_SIZE.x - 1일 때, y가 3 또는 4라면 GoalkeeperZone으로 설정
            if ((position.x == 0 || position.x == GameConstants.GRID_SIZE.x - 1) && (position.y == 3 || position.y == 4))
            {
                gridTile.type = TileType.GoalkeeperZone;
            }
            // position.x가 7보다 작으면 TeamA_Start, 그 외에는 TeamB_Start
            else if ((teamName == TeamName.TeamA && position.x < 7) ||
                             (teamName == TeamName.TeamB && position.x >= 7))
            {

                gridTile.type = TileType.SpawnZone;
            }
        }
    }

    public void TileTypeChangeWithPosition(Vector2Int cellPosition, TileType type)
    {
        gridTileDic[cellPosition].SetTileType(type);
    }



    public Vector3 GetNearestGridCenter(Vector3 worldPosition)
    {
        if (grid == null)
        {
            Debug.LogError("Grid is not assigned!");
            return worldPosition;
        }

        Vector3Int cellPosition = grid.WorldToCell(worldPosition);

        Vector3 cellCenter = grid.GetCellCenterWorld(cellPosition);

        return cellCenter;
    }

    public Vector2Int GetGridTilePosition(GridTile targetTile)
    {

        foreach (var kvp in gridTileDic)
        {
            if (kvp.Value == targetTile)
            {
                return kvp.Key;
            }
        }

        Debug.LogWarning("GridTile not found in dictionary!");
        return Vector2Int.zero;
    }

    public Grid GetGrid()
    {
        return grid;
    }


}