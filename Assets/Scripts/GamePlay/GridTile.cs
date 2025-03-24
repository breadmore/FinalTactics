using UnityEngine;

public class GridTile : MonoBehaviour
{
    //public TileType Type { get; private set; }
    public TileType type;
    public PlayerCharacter occupyingCharacter { get; private set; }
    public bool isOccupied = false;


    public bool CanPlaceCharacter()
    {
        if (isOccupied) return false;
        if (type != TileType.SpawnZone) return false;

        return true;
    }

    public void SetOccupied(PlayerCharacter character)
    {
        occupyingCharacter = character;
    }

    public void SetTileType(TileType type)
    {
        this.type = type;
    }

    public void ClearOccupied()
    {
        occupyingCharacter = null;
    }
}