using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;

public static class GridUtils
{
    public static int GetDistance(Vector2Int from, Vector2Int to)
    {
        return Mathf.Max(Mathf.Abs(from.x - to.x), Mathf.Abs(from.y - to.y));
    }

      // ���� �������θ� �Ÿ� ��� (�밢�� ����)
    public static bool IsStraightLineInRange(Vector2Int from, Vector2Int to, int range)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

            // �� �� �������θ� �Ÿ� �̵� + �Ÿ� ����
        return (dx == 0 && dy <= range) || (dy == 0 && dx <= range);
    }

    public static float GetBlockSuccessProbability(PlayerCharacter blocker)
    {
        float probability;
        if (blocker.CharacterData.characterStat.type == 0)
        {
            probability = Random.Range(0f, 0.5f);
        }
        else
        {
            probability = Random.Range(0.5f, 1f);
        }
        return probability;
    }

    public static float GetTackleSuccessProbability(PlayerCharacter tackler, PlayerCharacter target)
    {
        float baseChance = 0.6f;
        float bonus = (tackler.CharacterData.characterStat.tackle - target.CharacterData.characterStat.dribble) * 0.05f;
        return Mathf.Clamp01(baseChance + bonus);
    }

    public static float GetShootSuccessProbability(PlayerCharacter shooter, int distance)
    {
        float baseProbability = (shooter.CharacterData.characterStat.shoot * 8f) - (distance * 10f);
        return Mathf.Clamp01(baseProbability / 100f); // 0~1 ���̷� ����
    }
}

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
            Debug.LogWarning("gridTileList�� ũ�Ⱑ GRID_SIZE�� ��ġ���� ����!");
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
        foreach (var tile in gridTileList) // gridTiles�� ���� ���� �� ��� GridTile�� �����ϴ� ����Ʈ
        {
            if (tile.isOccupied && tile.occupyingCharacter != null && tile.occupyingCharacter.NetworkObjectId == networkId)
            {
                return tile.occupyingCharacter;
            }
        }
        return null; // �ش��ϴ� ĳ���Ͱ� ���� ��� null ��ȯ
    }

    public Grid GetGrid()
    {
        return grid;
    }

    public List<GridTile> GetAllGridTiles()
    {
        return gridTileList;
    }
}
