using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSlotParent : BaseLayoutGroupParent<CharacterSlotChild>
{
    private int characterCount = 0;
    public CharacterSlotChild selectedChild;
    public Action spawnAction;

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

        PlayerCharacter.OnSpawned += HandleCharacterSpawned;
        GameManager.Instance.thisPlayerBrain.SpawnPlayer(gridTile);

        
    }

    public void RegisterSpawnCallback(Action callback)
    {
        spawnAction = callback;
    }

    private void HandleCharacterSpawned(PlayerCharacter character)
    {
        PlayerCharacter.OnSpawned -= HandleCharacterSpawned;
        OnCharacterSpawnSuccess();
    }

    public void OnCharacterSpawnSuccess()
    {
        Debug.Log("Character Spawn!");

        selectedChild.gameObject.SetActive(false);
        characterCount++;
        GameManager.Instance.ClearAllSelected();
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
        Debug.Log("character count : " + characterCount);
        if (characterCount >= GameConstants.MaxCharacterCount)
        {
            InGameUIManager.Instance.CloseAllSlot();
            GameManager.Instance.ChangeState<CharacterPlacementCompleteState>();

            characterCount = 0;
        }
        else
        {
            GameManager.Instance.ChangeState<CharacterDataSelectionState>();
        }
    }
}