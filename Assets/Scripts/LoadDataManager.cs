using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDataManager : DontDestroySingleton<LoadDataManager>
{
    // ĳ���� ���� ����
    public CharacterDataReader characterDataReader{ get; private set; }
    public CharacterSlotBackgrounds characterSlotBackgrounds { get; private set; }

    // �׼� ���� ����
    public ActionDataReader actionDataReader { get; private set; }
    public ActionSlotBackgrounds actionSlotBackgrounds { get; private set; }

    // --
    public CharacterPrefabManager characterPrefabManager { get; private set; }

    protected override void Awake()
    {
        characterDataReader = Resources.Load<CharacterDataReader>("ScriptableObjects/CharacterDataReader");
        characterSlotBackgrounds = Resources.Load<CharacterSlotBackgrounds>("ScriptableObjects/CharacterSlotBackgrounds");
        actionDataReader = Resources.Load<ActionDataReader>("ScriptableObjects/ActionDataReader");
        actionSlotBackgrounds = Resources.Load<ActionSlotBackgrounds>("ScriptableObjects/ActionSlotBackgrounds");
        characterPrefabManager = Resources.Load<CharacterPrefabManager>("ScriptableObjects/CharacterPrefabManager");
    
    }

}
