using UnityEngine;
public class CharacterSlotParent : BaseLayoutGroupParent<CharacterSlotChild>
{
    private CharacterSlotBackgrounds characterSlotBackgrounds;
    private CharacterDataReader characterDataReader;
    private CharacterPrefabManager characterPrefabManager;

    private CharacterData selectedCharacterData;

    private bool isSelected = false;
    private int characterCount;

    private void Awake()
    {
        characterSlotBackgrounds = Resources.Load<CharacterSlotBackgrounds>("ScriptableObjects/CharacterSlotBackgrounds");
        characterDataReader = Resources.Load<CharacterDataReader>("ScriptableObjects/CharacterDataReader");
        characterPrefabManager = Resources.Load<CharacterPrefabManager>("ScriptableObjects/CharacterPrefabManager");
    }

    private void Start()
    {
        CreateChild(8);
        for (int i = 0; i < 8; i++)
        {
            InitChild(i);
        }
        characterCount = 0;
    }

    private void Update()
    {
        // 최대 생성 수 까지만
        if (characterCount >= GameConstants.MaxCharacterCount)
        {
            gameObject.SetActive(false);
            return;
        }
        if (isSelected)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = GameManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.CompareTag("Grid"))
                    {
                        if (selectedCharacterData != null)
                        {
                            GameManager.Instance.selectedGridTile = hit.collider.GetComponent<GridTile>();
                            Debug.Log(GameManager.Instance.selectedGridTile.name);
                            ToggleSelected();
                            characterCount++;
                        }
                        else
                        {
                            Debug.LogError("No character selected!");
                        }
                    }
                }
            }
        }
    }

    private void InitChild(int index)
    {
        if (characterSlotBackgrounds == null || characterDataReader == null)
        {
            Debug.LogError("Data is not assigned!");
            return;
        }

        CharacterData characterData = characterDataReader.DataList[index];
        characterData.characterSprite = characterSlotBackgrounds.GetBackground(index);

        childList[index].SetCharacterData(characterData);
    }

    public void SelectCharacterData(CharacterData characterData)
    {
        selectedCharacterData = characterData;
    }

    public void ToggleSelected()
    {
        isSelected = !isSelected;
    }
}