using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionSlotParent : BaseLayoutGroupParent<OptionSlotChild>
{
    private void Start()
    {
       List<ActionOptionData> actionOptionList = LoadDataManager.Instance.actionOptionDataReader.GetAllActionOptions();
        InitChildOption(actionOptionList);
    }

    private void OnEnable()
    {
        ShowOptionByActionId(GameManager.Instance.SelectedActionData);
    }

    private void OnDisable()
    {
        HideAllOption();
    }

    private void Update()
    {
        if (!IsOptionSelectedState()) return;

        HandleActionInput();
    }

    private bool IsOptionSelectedState()
    {
        return GameManager.Instance._currentState is ActionOptionSelecteState;
    }
    private void HandleActionInput()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        var ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit) || !hit.collider.CompareTag("Grid")) return;

        TryActionOption(hit.collider.GetComponent<GridTile>());
    }

    private void TryActionOption(GridTile gridTile)
    {
        if (GameManager.Instance.SelectedPlayerCharacter != null)
        {
            GameManager.Instance.OnGridTileSelected(gridTile);

            TurnManager.Instance.SubmitActionServerRpc(GameManager.Instance.SelectedPlayerCharacter.NetworkObjectId,
                GameManager.Instance.SelectedActionData,
                GameManager.Instance.SelectedGridTile.gridPosition,
                GameManager.Instance.SelectedActionOptionData);
            // Action 성공시 아래 작업

            GameManager.Instance.ChangeState<PlayerActionDecisionState>();

        }
        else
        {
            Debug.LogError("No character selected!");
        }
    }

    public void InitChildOption(List<ActionOptionData> actionOptionDataList)
    {
        CreateChild(actionOptionDataList.Count);

        for(int i=0; i<actionOptionDataList.Count; i++)
        {
            InitChild(i, actionOptionDataList[i]);
        }

    }

    private void InitChild(int index, ActionOptionData actionOptionData)
    {
        var sprite = LoadDataManager.Instance.optionSlotBackgrounds.GetBackground(actionOptionData.id);
        if (sprite == null) return;

        childList[index].SetOption(actionOptionData);
        childList[index].SetOptionSprite(sprite);
    }

    public void ShowOptionByActionId(int id)
    {
        foreach (var option in childList)
        {
            if (option.ActionOptionData.actionId == id)
            {
                option.gameObject.SetActive(true);
            }
        }
    }

    public void HideAllOption()
    {
        foreach (var option in childList)
        {
            option.gameObject.SetActive(false);
        }
    }
}
