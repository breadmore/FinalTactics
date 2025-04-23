using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlotParent : BaseLayoutGroupParent<CharacterSlotChild>
{
    private int characterCount = 0;
    public CharacterSlotChild selectedChild;

    private void OnEnable()
    {
        characterCount = 0;
    }

    private void Start()
    {
        InitializeCharacterSlots();
    }

    private void InitializeCharacterSlots()
    {
        CreateChild(8);
        for (int i = 0; i < 8; i++)
        {
            InitChild(i);
        }
    }

    private void Update()
    {
        if (!IsCharacterDataSelectedState())
        {
            return;
        }
        HandleCharacterPlacementInput();
    }

    private bool IsCharacterDataSelectedState()
    {
        // 상태 패턴 버전: 현재 상태가 CharacterDataSelectedState인지 확인
        return GameManager.Instance._currentState is CharacterDataSelectedState;
    }

    private void HandleCharacterPlacementInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("UI");
            return;
        }

        var ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit) || !hit.collider.CompareTag("Grid")) return;

        TryPlaceCharacter(hit.collider.GetComponent<GridTile>());
    }

    private void TryPlaceCharacter(GridTile gridTile)
    {
        if (GameManager.Instance.SelectedCharacterData == null)
        {
            Debug.LogError("No character selected!");
            return;
        }

        GameManager.Instance.OnGridTileSelected(gridTile);

        if (!gridTile.CanPlaceCharacter()) return;

        GameManager.Instance.thisPlayerBrain.SpawnPlayer(gridTile);
        OnCharacterSpawnSuccess();
        
    }

    private void OnCharacterSpawnSuccess()
    {
        selectedChild.gameObject.SetActive(false);
        characterCount++;
        CheckCharacterLimit();

    }

    private void InitChild(int index)
    {
        if (LoadDataManager.Instance.characterSlotBackgrounds == null ||
            LoadDataManager.Instance.characterDataReader == null)
        {
            Debug.LogError("Data is not assigned!");
            return;
        }

        CharacterData characterData = LoadDataManager.Instance.characterDataReader.DataList[index];
        childList[index].SetCharacterData(characterData);
        childList[index].SetcharacterSprite(
            LoadDataManager.Instance.characterSlotBackgrounds.GetBackground(index)
        );
    }

    private void CheckCharacterLimit()
    {
        if (characterCount >= GameConstants.MaxCharacterCount)
        {
            InGameUIManager.Instance.CharacterSlot.SetActive(false);
            GameManager.Instance.ChangeState<CharacterPlacementCompleteState>();
        }
    }
}