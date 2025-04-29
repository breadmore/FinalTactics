using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Cysharp.Threading.Tasks;

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

    public static (Vector2Int tacklerNewPos, Vector2Int targetNewPos) TryGetTacklePositions(
        Vector2Int tacklerPos,
        Vector2Int targetPos,
        GridManager gridManager)
    {
        Vector2Int knockbackDir = targetPos - tacklerPos;
        Vector2Int knockbackTargetPos = targetPos + knockbackDir;

        bool isInsideBounds = knockbackTargetPos.x >= 0 && knockbackTargetPos.x < GameConstants.GRID_SIZE.x
                              && knockbackTargetPos.y >= 0 && knockbackTargetPos.y < GameConstants.GRID_SIZE.y;

        GridTile knockbackTile = gridManager.GetGridTileAtPosition(knockbackTargetPos);
        bool isBlocked = knockbackTile == null || knockbackTile.IsOccupied();

        if (isInsideBounds && !isBlocked)
        {
            // �з��� ��ġ�� ��ȿ�ϸ� ��Ŭ���� Ÿ�� �ڸ���, Ÿ���� �з���
            return (targetPos, knockbackTargetPos);
        }
        else
        {
            // �׷��� ������ ���� �ڸ��� ��ȯ
            return (targetPos, tacklerPos);
        }
    }

    // �� ���� Ȯ�� (���� ���ʽ� �ݿ�)
    public static ShootResult GetShootSuccessProbability(PlayerCharacter shooter, int distance, int chargeLevel = 0)
    {
        // �⺻ ���
        float baseRate = shooter.CharacterStat.shoot * 0.08f; // 0~100 ������ 0~8 ������

        // �Ÿ� �г�Ƽ (�ִ� 70% �г�Ƽ)
        float distancePenalty = Mathf.Min(distance * 0.07f, 0.7f);

        // ���� ���ʽ� (�ִ� 30% ���ʽ�)
        float chargeBonus = Mathf.Min(chargeLevel * 0.1f, 0.3f);

        // ���� Ȯ�� ���
        float successRate = Mathf.Clamp01(baseRate - distancePenalty + chargeBonus);

        // ũ��Ƽ�� Ȯ�� (�⺻ 10% + ���� ���ʽ�)
        bool isCritical = Random.value < (0.1f + chargeLevel * 0.05f);

        return new ShootResult(
            successRate: isCritical ? Mathf.Min(successRate * 1.5f, 0.95f) : successRate,
            isCritical: isCritical
        );

        
    }

    public readonly struct ShootResult
    {
        public readonly float successRate;
        public readonly bool isCritical;

        public ShootResult(float successRate, bool isCritical)
        {
            this.successRate = successRate;
            this.isCritical = isCritical;
        }

        public bool IsSuccess()
        {
            return Random.value <= successRate;
        }
    }
}

public class GridManager : NetworkSingleton<GridManager>
{
    private Grid grid;

    [SerializeField] private List<GridTile> gridTileList = new List<GridTile>();


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

            if ((position.x == 0 || position.x == GameConstants.GRID_SIZE.x - 1) && (position.y == 4 || position.y == 5))
            {
                gridTile.SetTileType(TileType.GoalkeeperZone);
            }
            else if ((teamName == TeamName.Red && position.x < GameConstants.GRID_SIZE.x/2) ||
                     (teamName == TeamName.Blue && position.x >= GameConstants.GRID_SIZE.x/2))
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

    public async UniTask ResetAllGridTileAsync()
    {
        foreach (var gridTile in gridTileList)
        {
            gridTile.ResetGridTile();
            await UniTask.Yield(); // Ÿ�� �� ���� ��� ���
        }
    }


    public void TurnStartSetting()
    {
        foreach(var gridTile in gridTileList)
        {
            gridTile.TurnStartSetting();
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
            if (tile.IsOccupied() && tile.occupyingCharacter.NetworkObjectId == networkId)
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
