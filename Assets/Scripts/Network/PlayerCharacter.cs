using UnityEngine;
using Unity.Netcode;

public class PlayerCharacter : NetworkBehaviour
{
    public int CharacterID { get; private set; }
    public PlayerTeam Team { get; private set; }
    public CharacterStat Stats { get; private set; }

    private Vector3Int gridPosition;

    public void Initialize(int id, PlayerTeam team, CharacterStat stats)
    {
        CharacterID = id;
        Team = team;
        Stats = stats;
    }

    public Vector3Int GetGridPosition()
    {
        return gridPosition;
    }

    public void SetGridPosition(Vector3Int newPosition)
    {
        gridPosition = newPosition;
    }

    public void Move(Vector3Int targetPosition)
    {
        if (!IsOwner) return; // Netcode: 본인 캐릭터만 조작 가능
        SetGridPosition(targetPosition);
        //transform.position = GridManager.Instance.GetNearestGridCenter(targetPosition);
    }
}
