using System.Collections.Generic;
using UnityEngine;

public class ActionPreviewManager : Singleton<ActionPreviewManager>
{
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material baseMaterial;

    private List<GridTile> highlightedTiles = new();

    public void HighlightTilesForAction(ActionType actionType, PlayerCharacter player)
    {
        Debug.Log("HightLight for action");

        // 1. �׼� �ڵ鷯 ����
        var handler = ActionHandlerFactory.CreateHandler(actionType);
        if (handler == null)
        {
            Debug.LogWarning("Invalid action type.");
            return;
        }

        // 3. ��� Ÿ�� �� ��ȿ�� �͸� üũ
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
        Debug.Log("HighLight");
        var renderer = tile.GetComponent<Renderer>();
        if (renderer != null && highlightMaterial != null)
        {
            renderer.material = highlightMaterial;
            highlightedTiles.Add(tile);
        }
    }

    public void ClearHighlights()
    {
        Debug.Log("Clear!");
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
