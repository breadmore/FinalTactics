using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotChild : BaseLayoutGroupChild<CharacterSlotChild>
{
    private Image thisImage;
    private Button button;
    private CharacterSlotParent slotParent;
    private CharacterData characterData;
    public override void SetParent<T>(ILayoutGroupParent<T> newParent)
    {
        base.SetParent(newParent);

        if (newParent is CharacterSlotParent parent)
        {
            slotParent = parent;
        }
    }

    private void Awake()
    {
        thisImage = GetComponent<Image>();
        button = GetComponent<Button>();
    }
    protected override void Start()
    {
        base.Start();
        button.onClick.AddListener(SelectCharacterData);
    }
    public void PrintCharacterData()
    {
        Debug.Log(characterData.characterStat.shoot);
    }

    public void CallParentMethod()
    {
        //slotParent?.ShowAll();
    }

    public void SetCharacterData(CharacterData newCharacterData)
    {
        if (newCharacterData == null) return;

        characterData = newCharacterData;
        SetCharacterStat(characterData.characterStat);
    }

    public void SetcharacterSprite(Sprite newSprite)
    {
        if (characterData == null) return;

        if (thisImage != null && newSprite != null)
        {
            thisImage.sprite = newSprite;
        }
        else
        {
            Debug.LogError("No Image or Sprite is null");
        }
    }

    public void SetCharacterStat(CharacterStat newCharacterStat)
    {
        if (characterData == null) return;

        characterData.characterStat = newCharacterStat;
    }

    public CharacterData GetCharacterData()
    {
        return characterData;
    }

    private void SelectCharacterData()
    {
        Debug.Log("Selecte!");
        GameManager.Instance.OnCharacterDataSelected(characterData);
        slotParent.selectedChild = this;
    }

}
