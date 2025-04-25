using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class GridTile : NetworkBehaviour
{
    public TileType Type { get; private set; }
    public Vector2Int gridPosition { get; private set; }
    public PlayerCharacter occupyingCharacter { get; private set; }
    public bool IsOccupied() => occupyingCharacter != null;
    public PlayerCharacter blockCharacter { get; private set; }
    public float BlockProbability { get; private set; } = 0;
    private bool isBlocking = false;

    public bool CanPlaceCharacter()
    {
        if (IsOccupied()) return false;
        if (Type != TileType.SpawnZone) return false;

        return true;
    }


    public void SetTileType(TileType type)
    {
        this.Type = type;
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }



    public void BlockProbabilityDecision(float blockProbability, PlayerCharacter blocker)
    {
        if (isBlocking) return;
        blockCharacter = blocker;
        isBlocking = !isBlocking;

        BlockProbability = blockProbability;
    }

    public void ResetGridTile()
    {
        ClearOccupiedClientRpc(0);

        BlockProbability = 0;
        isBlocking = false;
    }

    public void TurnStartSetting()
    {
        if (occupyingCharacter != null)
        {
            occupyingCharacter.PlayAnimationIdle();
        }

        BlockProbability = 0;
        isBlocking = false;

        if (blockCharacter != null)
        {
            blockCharacter = null;
        }

    }

    [ClientRpc]
    public void SetOccupiedClientRpc(ulong networkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out NetworkObject obj))
        {
            // 이미 같은 캐릭터가 점유 중이면 무시
            if (occupyingCharacter != null && occupyingCharacter.NetworkObjectId == networkId)
                return;

            occupyingCharacter = obj.GetComponent<PlayerCharacter>();
        }
    }
    [ClientRpc]
    public void ClearOccupiedClientRpc(ulong networkId)
    {
        if (networkId == 0) occupyingCharacter = null;

        // 현재 점유 중인 캐릭터와 요청된 네트워크 ID가 일치할 때만 클리어
        if (occupyingCharacter != null && occupyingCharacter.NetworkObjectId == networkId)
        {
            occupyingCharacter = null;
        }
    }

}
