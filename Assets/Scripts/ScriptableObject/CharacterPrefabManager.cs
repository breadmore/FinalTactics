using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPrefabManager", menuName = "Game Data/CharacterPrefab Manager")]
public class CharacterPrefabManager : ScriptableObject
{
    [SerializeField] private GameObject characterPrefab; // �÷��̾� ĳ���� Prefab

    public GameObject GetPlayerCharacterPrefab()
    {
        return characterPrefab;
    }
}
