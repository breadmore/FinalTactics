using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPrefabManager", menuName = "Game Data/CharacterPrefab Manager")]
public class CharacterPrefabManager : ScriptableObject
{
    [SerializeField] private List<GameObject> characterPrefab = new List<GameObject>(); // 플레이어 캐릭터 Prefab

    public GameObject GetPlayerCharacterPrefabById(int id)
    {
        return characterPrefab[id];
    }
}
