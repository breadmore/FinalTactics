﻿using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class GridTile : NetworkBehaviour
{
    public TileType Type { get; private set; }
    public Vector2Int gridPosition { get; private set; }
    public PlayerCharacter occupyingCharacter { get; private set; }
    private NetworkVariable<bool> isOccupied = new NetworkVariable<bool>();
    public bool IsOccupied => isOccupied.Value;
    public float BlockProbability = 0;
    //public float BlockProbability { get; private set; } = 0;
    private bool isBlocking = false;
    public bool CanPlaceCharacter()
    {
        if (IsOccupied) return false;
        if (Type != TileType.SpawnZone) return false;

        return true;
    }

    public void UpdateGridTileState(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject characterNetworkObject))
        {
            PlayerCharacter playerCharacter = characterNetworkObject.GetComponent<PlayerCharacter>();
            SetOccupied(playerCharacter);
        }
    }

    public void SetOccupied(PlayerCharacter character)
    {
        occupyingCharacter = character;
        if(occupyingCharacter != null)
        isOccupied.Value = true;
    }

    public void SetTileType(TileType type)
    {
        this.Type = type;
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public void ClearOccupied()
    {
        occupyingCharacter = null;
        isOccupied.Value = false;
    }

    public void BlockProbabilityDecision(float blockProbability)
    {
        if (isBlocking) return;
        isBlocking = !isBlocking;
        BlockProbability = blockProbability;
    }

    public void ResetGridTile()
    {
        ClearOccupied();

        BlockProbability = 0;
        isBlocking = false;

    }
}
