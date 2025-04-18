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

      // 십자 방향으로만 거리 허용 (대각선 제외)
    public static bool IsStraightLineInRange(Vector2Int from, Vector2Int to, int range)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);

            // 한 쪽 방향으로만 거리 이동 + 거리 제한
        return (dx == 0 && dy <= range) || (dy == 0 && dx <= range);
    }

    public static float GetBlockSuccessProbability(PlayerCharacter blocker)
    {
        float probability;
        if (blocker.CharacterStat.type == 0)
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
        float bonus = (tackler.CharacterStat.tackle - target.CharacterStat.dribble) * 0.05f;
        return Mathf.Clamp01(baseChance + bonus);
    }

    public static float GetShootSuccessProbability(PlayerCharacter shooter, int distance)
    {
        float baseProbability = (shooter.CharacterStat.shoot * 8f) - (distance * 10f);
        return Mathf.Clamp01(baseProbability / 100f); // 0~1 사이로 보정
    }
}

public class GridManager : NetworkSingleton<GridManager>
{
    private Grid grid;

    [SerializeField] private List<GridTile> gridTileList = new List<GridTile>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.WaitingForReset)
        {
            Debug.Log($"{name}: 턴 시작 시 준비 행동 실행");
            ResetAllGridTile();
        }
    }

    public override void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        base.OnDestroy();
    }

    private void Awake()
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

            if ((position.x == 0 || position.x == GameConstants.GRID_SIZE.x - 1) && (position.y == 4 || position.y == 5))
            {
                gridTile.SetTileType(TileType.GoalkeeperZone);
            }
            else if ((teamName == TeamName.TeamA && position.x < GameConstants.GRID_SIZE.x/2) ||
                     (teamName == TeamName.TeamB && position.x >= GameConstants.GRID_SIZE.x/2))
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

    public void ResetAllGridTile()
    {
        foreach (var gridTile in gridTileList) 
        {
            gridTile.ResetGridTile();
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
            if (tile.IsOccupied && tile.occupyingCharacter.NetworkObjectId == networkId)
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

    public List<GridTile> GetAllGridTiles()
    {
        return gridTileList;
    }
}
