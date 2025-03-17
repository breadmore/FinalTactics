using UnityEngine;

public class GridManager : Singleton<GridManager>
{
    private Grid grid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = GetComponent<TilemapScaler>().tilemapGrid;
    }

    public Vector3 GetNearestGridCenter(Vector3 worldPosition)
    {
        if (grid == null)
        {
            Debug.LogError("Grid is not assigned!");
            return worldPosition;
        }

        Vector3Int cellPosition = grid.WorldToCell(worldPosition);

        Vector3 cellCenter = grid.GetCellCenterWorld(cellPosition);

        return cellCenter;
    }

    public Grid GetGrid()
    {
        return grid;
    }


}
