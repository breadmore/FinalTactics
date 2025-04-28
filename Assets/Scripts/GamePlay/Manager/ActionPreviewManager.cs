using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewManager : Singleton<ActionPreviewManager>
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material baseMaterial;

    private List<GridTile> highlightedTiles = new();

    public void HighlightTilesForAction(int actionId, PlayerCharacter player)
    {
        ClearHighlights();

        // ActionData ��������
        ActionData actionData = LoadDataManager.Instance.actionDataReader.GetActionDataById(actionId);
        if (actionData == null)
        {
            Debug.LogWarning($"No action data found for id: {actionId}");
            return;
        }

        // �ڵ鷯 ���� (�ɼ��� �̸����⿡���� �ʿ� �����Ƿ� �⺻�� -1 ����)
        PlayerAction dummyAction = new PlayerAction
        {
            actionId = actionId,
            optionId = -1
        };

        var handler = ActionHandlerFactory.CreateHandler(dummyAction);
        if (handler == null)
        {
            Debug.LogWarning($"No handler for action type: {actionData.actionType}");
            return;
        }

        // Ÿ�� ���̶���Ʈ
        foreach (var tile in GridManager.Instance.GetAllGridTiles())
        {
            if (handler.CanExecute(player, tile))
            {
                HighlightTile(tile);
            }
        }
    }

    private void HighlightTile(GridTile tile)
    {
        var renderer = tile.GetComponent<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            renderer.material = highlightMaterial;
            highlightedTiles.Add(tile);
        }
    }

    public void ClearHighlights()
    {
        foreach (var tile in highlightedTiles)
        {
            var renderer = tile.GetComponent<Renderer>();
            if (renderer != null && baseMaterial != null)
            {
                renderer.material = baseMaterial;
            }
        }
        highlightedTiles.Clear();
    }
}