#nullable enable

using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Authentication;
using System;

public class ActionSlotParent : BaseLayoutGroupParent<ActionSlotChild>
{
    private int actionCount = 0;
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
        if (GameManager.Instance.CurrentState == GameState.ActionSelected && Input.GetMouseButtonDown(0))
        {
            Ray ray = CameraManager.Instance.mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Grid"))
                {
                    if (GameManager.Instance.SelectedCharacterData != null)
                    {
                        GameManager.Instance.OnGridTileSelected(hit.collider.GetComponent<GridTile>());
                        //GameManager.Instance.ExecuteSelectedAction(GameManager.Instance.SelectedGridTile.gridPosition);
                        TurnManager.Instance.SubmitActionServerRpc(GameManager.Instance.SelectedPlayerCharacter.NetworkObjectId, 
                            GameManager.Instance.SelectedActionData, 
                            GameManager.Instance.SelectedGridTile.gridPosition);
                        // Action 성공시 아래 작업

                    }
                    else
                    {
                        Debug.LogError("No character selected!");
                    }
                }
                else
                {
                    Debug.LogError("No Grid Selected");
                }
            }
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

}
