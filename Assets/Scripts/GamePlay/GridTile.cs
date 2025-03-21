using UnityEngine;

public class GridTile : MonoBehaviour
{
    //public TileType Type { get; private set; }
    public TileType type { get; set; }
    public PlayerCharacter occupyingCharacter { get; private set; }
    public bool isOccupied = false;


    public bool CanPlaceCharacter(PlayerTeam team)
    {
        if (isOccupied) return false;
        if (type == TileType.GoalkeeperZone) return false;

        // A���� A�� ��ġ ��������, B���� B�� ��ġ �������� ��ġ ����
        if (team.name == TeamName.TeamA && type != TileType.TeamA_Start) return false;
        if (team.name == TeamName.TeamB && type != TileType.TeamB_Start) return false;

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