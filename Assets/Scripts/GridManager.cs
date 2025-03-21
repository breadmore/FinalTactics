using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    private Grid grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private List<GridTile> gridTileList = new List<GridTile>();
    private Dictionary<Vector2Int, GridTile> gridTileDic = new Dictionary<Vector2Int, GridTile>();
    void Start()
    {
        grid = GetComponent<TilemapScaler>().tilemapGrid;
        InitializeGrid();
        TileTypeChange();
    }

    private void InitializeGrid()
    {
        gridTileDic.Clear();  // �̹� �����ϴ� �����ʹ� ����

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
                    Debug.LogWarning($"gridTileList�� �׸� ���� {x}, {z}�� ���� ����");
                    return;
                }
            }
        }
    }

    private void TileTypeChange()
    {
        // gridTileDic���� ��� GridTile�� ��ȸ�ϸ� ���ǿ� �°� TileType ����
        foreach (var kvp in gridTileDic)
        {
            Vector2Int position = new Vector2Int((int)kvp.Key.x, (int)kvp.Key.y);
            GridTile gridTile = kvp.Value;

            // x�� 0�̰ų� GRID_SIZE.x - 1�� ��, y�� 3 �Ǵ� 4��� GoalkeeperZone���� ����
            if ((position.x == 0 || position.x == GameConstants.GRID_SIZE.x - 1) && (position.y == 3 || position.y == 4))
            {
                gridTile.type = TileType.GoalkeeperZone;
            }
            // position.x�� 7���� ������ TeamA_Start, �� �ܿ��� TeamB_Start
            else
            {
                gridTile.type = position.x < 7 ? TileType.TeamA_Start : TileType.TeamB_Start;
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