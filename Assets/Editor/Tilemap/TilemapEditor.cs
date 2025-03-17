using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Tilemap tilemap = (Tilemap)target;

        // Tilemap Grid 셀 크기를 Tile Palette 셀 크기와 맞추기
        Grid grid = tilemap.GetComponentInParent<Grid>();
        if (grid != null)
        {
            Vector3 cellSize = grid.cellSize;
            // Tile Palette 셀 크기와 동일하게 맞추기
            // 이 부분은 Tile Palette에 맞는 셀 크기 설정을 의미합니다.
            // 하지만 Tile Palette에서 직접적으로 셀 크기를 설정하는 방법은 없습니다.
        }

        base.OnInspectorGUI();
    }
}
