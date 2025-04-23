#nullable enable

using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Authentication;
using System;
using UnityEngine.EventSystems;

public class ActionSlotParent : BaseLayoutGroupParent<ActionSlotChild>
{
    private int actionCount = 0;

    private void OnEnable()
    {
        // ���� �ʱ�ȭ �� �� ��� ����
        if (childList == null || childList.Count == 0)
        {
            Debug.Log("No child");

            return;
        }

        RefreshActionSlots(BallManager.Instance.IsBallOwnedBy(GameManager.Instance.SelectedPlayerCharacter));
    }
    private void Start()
    {
        actionCount = (int)ActionType.Length; 
        CreateChild(actionCount);
        for (int i = 0; i < actionCount; i++)
        {
            InitChild(i);
        }

    }

    private void Update()
    {
        if (!IsActionSelectedState()) return;

        HandleActionInput();
    }

    private bool IsActionSelectedState()
    {
        return GameManager.Instance._currentState is ActionDataSelectedState;
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

        TryAction(hit.collider.GetComponent<GridTile>());
    }

    private void TryAction(GridTile gridTile)
    {
        if (GameManager.Instance.SelectedPlayerCharacter != null)
        {
            GameManager.Instance.OnGridTileSelected(gridTile);

            TurnManager.Instance.SubmitActionServerRpc(GameManager.Instance.SelectedPlayerCharacter.NetworkObjectId,
                GameManager.Instance.SelectedActionData,
                GameManager.Instance.SelectedGridTile.gridPosition,
                GameManager.Instance.SelectedActionOptionData);
            // Action ������ �Ʒ� �۾�

            GameManager.Instance.ChangeState<PlayerActionDecisionState>();

        }
        else
        {
            Debug.LogError("No character selected!");
        }
    }
    private void InitChild(int index)
    {
        if (LoadDataManager.Instance.actionSlotBackgrounds == null || LoadDataManager.Instance.actionDataReader == null)
        {
            Debug.LogError("Data is not assigned!");
            return;
        }

        childList[index].SetAction(index);
        childList[index].SetActionSprite(LoadDataManager.Instance.actionSlotBackgrounds.GetBackground(index));
    }

    // �� ���� ���ο� ���� � �׼ǵ��� �������� ����
    public void RefreshActionSlots(bool hasBall)
    {
        foreach (var slot in childList)
        {
            var actionData = LoadDataManager.Instance.actionDataReader.GetActionDataById(slot.ActionId);

            if (actionData == null)
            {
                slot.gameObject.SetActive(false);
                continue;
            }

            bool isCommon = actionData.category == ActionCategory.Common;
            bool isOffensive = actionData.category == ActionCategory.Offense;
            bool isDefensive = actionData.category == ActionCategory.Defense;
            bool isKeeper = actionData.category == ActionCategory.Keeper;

            if (hasBall)
            {
                // ���� ���� ��: ���� + ����
                slot.gameObject.SetActive(isCommon || isOffensive);
            }
            else
            {
                // ���� ���� ��: ���� + ����
                slot.gameObject.SetActive(isCommon || isDefensive);
            }
        }
    }

    public void ClearChildList()
    {
        for(int i=0; i<childList.Count; i++)
        {
            DeleteChild(i);
        }
    }

    private void SetAllInactive()
    {
        foreach (var slot in childList)
        {
            slot.gameObject.SetActive(false);
        }
    }

}
