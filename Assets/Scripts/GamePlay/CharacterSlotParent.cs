using UnityEngine;
public class CharacterSlotParent : BaseLayoutGroupParent<CharacterSlotChild>
{
    private CharacterSlotBackgrounds characterSlotBackgrounds;
    private CharacterDataReader characterDataReader;
    private CharacterPrefabManager characterPrefabManager;

    private CharacterData selectedCharacterData;

    private bool isSelected = false;

    private void Awake()
    {
        characterSlotBackgrounds = Resources.Load<CharacterSlotBackgrounds>("ScriptableObjects/CharacterSlotBackgrounds");
        characterDataReader = Resources.Load<CharacterDataReader>("ScriptableObjects/CharacterDataReader");
        characterPrefabManager = Resources.Load<CharacterPrefabManager>("ScriptableObjects/CharacterPrefabManager");
    }

    private void Start()
    {
        CreateChild(2);
        for (int i = 0; i < 2; i++)
        {
            InitChild(i);
        }
    }

    private void Update()
    {
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
                            Vector3 spawnPosition = GridManager.Instance.GetNearestGridCenter(hit.point);
                            GameManager.Instance.selectedGridPosition = spawnPosition;
                            Debug.Log("Grid position stored: " + spawnPosition);
                            ToggleSelected();
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