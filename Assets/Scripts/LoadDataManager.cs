using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDataManager : DontDestroySingleton<LoadDataManager>
{
    public CharacterSlotBackgrounds characterSlotBackgrounds { get; private set; }
    public CharacterDataReader characterDataReader{ get; private set; }
    public CharacterPrefabManager characterPrefabManager { get; private set; }

    protected override void Awake()
    {
        characterSlotBackgrounds = Resources.Load<CharacterSlotBackgrounds>("ScriptableObjects/CharacterSlotBackgrounds");
        characterDataReader = Resources.Load<CharacterDataReader>("ScriptableObjects/CharacterDataReader");
        characterPrefabManager = Resources.Load<CharacterPrefabManager>("ScriptableObjects/CharacterPrefabManager");
    }

}
