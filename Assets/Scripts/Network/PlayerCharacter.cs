using UnityEngine;
using Unity.Netcode;

public class PlayerCharacter : NetworkBehaviour
{
    public CharacterData CharacterData { get; private set; }
    public TeamName Team { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    public void Initialize(CharacterData characterData, TeamName team, Vector2Int gridPosition)
    {
        CharacterData = characterData;
        Team = team;
        GridPosition = gridPosition;
    }

    public void Move(Vector2Int targetPosition)
    {
        GridPosition = targetPosition;
    }
}
