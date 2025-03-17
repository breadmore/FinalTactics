using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(Tilemap))]
public class TilemapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Tilemap tilemap = (Tilemap)target;

        // Tilemap Grid �� ũ�⸦ Tile Palette �� ũ��� ���߱�
        Grid grid = tilemap.GetComponentInParent<Grid>();
        if (grid != null)
        {
            Vector3 cellSize = grid.cellSize;
            // Tile Palette �� ũ��� �����ϰ� ���߱�
            // �� �κ��� Tile Palette�� �´� �� ũ�� ������ �ǹ��մϴ�.
            // ������ Tile Palette���� ���������� �� ũ�⸦ �����ϴ� ����� �����ϴ�.
        }

        base.OnInspectorGUI();
    }
}
