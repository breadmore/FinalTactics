using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TilemapScaler : MonoBehaviour
{
    public GameObject footballGround; // Plane 오브젝트
    public Grid tilemapGrid; // Grid 오브젝트

    public int columns = 16;
    public int rows = 10;

    public void ResizeTilemap()
    {
        if (footballGround == null || tilemapGrid == null) return;

        // Plane의 실제 월드 사이즈 (Plane은 기본적으로 10x10 유닛이므로 scale 곱해줘야 함)
        Vector3 worldSize = new Vector3(
            footballGround.transform.localScale.x,
            footballGround.transform.localScale.y,
            footballGround.transform.localScale.z
        );

        float unitWidth = worldSize.x / columns;
        float unitHeight = worldSize.z / rows;

        // 정사각형 유지하려면 더 작은 쪽을 기준으로 맞춤
        float squareSize = Mathf.Min(unitWidth, unitHeight);

        Vector3 newCellSize = new Vector3(
            squareSize,
            squareSize,
            1f
        );

        if (tilemapGrid.cellSize != newCellSize)
        {
            tilemapGrid.cellSize = newCellSize;

#if UNITY_EDITOR
            EditorUtility.SetDirty(tilemapGrid);
#endif

            Debug.Log($"✅ 정사각형 그리드 적용됨: {newCellSize}");
        }
    }
}
