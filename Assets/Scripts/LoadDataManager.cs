using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LoadDataManager : DontDestroySingleton<LoadDataManager>
{
    // 캐릭터 슬롯 관련
    public CharacterDataReader characterDataReader{ get; private set; }
    public CharacterSlotBackgrounds characterSlotBackgrounds { get; private set; }

    // 액션 슬롯 관련
    public ActionDataReader actionDataReader { get; private set; }
    public ActionOptionDataReader actionOptionDataReader { get; private set; }
    public ActionSlotBackgrounds actionSlotBackgrounds { get; private set; }
    public OptionSlotBackgrounds optionSlotBackgrounds { get; private set; }

    // --
    public CharacterPrefabManager characterPrefabManager { get; private set; }

    protected override void Awake()
    {
        characterDataReader = Resources.Load<CharacterDataReader>("ScriptableObjects/CharacterDataReader");
        characterSlotBackgrounds = Resources.Load<CharacterSlotBackgrounds>("ScriptableObjects/CharacterSlotBackgrounds");
        actionDataReader = Resources.Load<ActionDataReader>("ScriptableObjects/ActionDataReader");
        actionOptionDataReader = Resources.Load<ActionOptionDataReader>("ScriptableObjects/ActionOptionDataReader");
        actionSlotBackgrounds = Resources.Load<ActionSlotBackgrounds>("ScriptableObjects/ActionSlotBackgrounds");
        optionSlotBackgrounds = Resources.Load<OptionSlotBackgrounds>("ScriptableObjects/OptionSlotBackgrounds");
        characterPrefabManager = Resources.Load<CharacterPrefabManager>("ScriptableObjects/CharacterPrefabManager");
    
    }

}
