using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public TileType Type { get; private set; }
    public Vector2Int gridPosition { get; private set; }
    public PlayerCharacter occupyingCharacter { get; private set; }
    public bool isOccupied { get; private set; } = false;

    public float BlockProbability = 0;
    //public float BlockProbability { get; private set; } = 0;
    private bool isBlocking = false;
    public bool CanPlaceCharacter()
    {
        if (isOccupied) return false;
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
        isOccupied = true;
        occupyingCharacter = character;
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
        isOccupied = false;
        occupyingCharacter = null;
    }

    public void BlockProbabilityDecision(float blockProbability)
    {
        if (isBlocking) return;
        isBlocking = !isBlocking;
        BlockProbability = blockProbability;
    }
}
