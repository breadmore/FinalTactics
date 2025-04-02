using UnityEngine;
using System.Collections.Generic;

public class ActionSlotParent : BaseLayoutGroupParent<ActionSlotChild>
{

    private int actionCount = 0;

    private void Start()
    {
        actionCount = 9; 
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
                        GameManager.Instance.ExecuteSelectedAction(GameManager.Instance.SelectedGridTile.gridPosition);

                        // spawn 성공시 아래 작업

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
