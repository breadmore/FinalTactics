using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPrefabManager", menuName = "Game Data/CharacterPrefab Manager")]
public class CharacterPrefabManager : ScriptableObject
{
    [SerializeField] private GameObject characterPrefab; // 플레이어 캐릭터 Prefab

    public GameObject GetPlayerCharacterPrefab()
    {
        return characterPrefab;
    }
}
