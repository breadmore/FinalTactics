using UnityEngine;
using UnityEngine.UI;

public class CharacterSlotChild : BaseLayoutGroupChild<CharacterSlotChild>
{
    private Image thisImage;
    private Button button;
    private CharacterSlotParent slotParent;
    private CharacterData characterData;
    private bool isSpawned = false;
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
        SetCharacterData(characterData.characterStat);
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

    public void SetCharacterData(CharacterStat newCharacterStat)
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
        // 클릭 이미 돼있으면 리턴
        if (isSpawned) return;

        Debug.Log("Select Character!");
        slotParent?.ToggleChildSelected();
        GameManager.Instance.OnCharacterDataSelected(characterData);
        CheckSpawned();
    }

    // 클릭(생성) 했는지 체크
    public void CheckSpawned()
    {
        // 클릭 시 추가할 효과

        //
        isSpawned = !isSpawned;
    }


}
