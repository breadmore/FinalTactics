using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TilemapScaler : MonoBehaviour
{
    public GameObject footballGround; // FootballGround 오브젝트
    public Grid tilemapGrid; // Tilemap이 있는 Grid 오브젝트

    public void ResizeTilemap()
    {
        if (footballGround == null || tilemapGrid == null) return;

        // **1️⃣ Mesh Renderer에서 Bounds 가져오기**
        MeshRenderer meshRenderer = footballGround.GetComponent<MeshRenderer>();
        if (meshRenderer == null || meshRenderer.sharedMaterials.Length == 0) return;

        Bounds grassBounds = meshRenderer.bounds;

        // **2️⃣ Material의 UV Scale 가져오기 (Element 0 = Grass In)**
        Material grassMaterial = meshRenderer.sharedMaterials[0]; // Element 0 (grass in)

        Vector2 textureScale = Vector2.one; // 기본값

        if (grassMaterial.HasProperty("_MainTex"))
        {
            textureScale = grassMaterial.mainTextureScale; // UV 텍스처 스케일 가져오기
        }

        // **3️⃣ 실제 크기를 UV Scale로 조정**
        float actualWidth = grassBounds.size.x * textureScale.x;
        float actualHeight = grassBounds.size.z * textureScale.y;

        // **4️⃣ 5x8로 나누기**
        Vector3 newCellSize = new Vector3(
            actualWidth / 16f, // X축 크기
            actualHeight / 10f,  // Y축 크기
            tilemapGrid.cellSize.z // Z축 크기는 유지
        );

        // **5️⃣ Tilemap Cell Size 변경**
        if (tilemapGrid.cellSize != newCellSize)
        {
            tilemapGrid.cellSize = newCellSize;

#if UNITY_EDITOR
            EditorUtility.SetDirty(tilemapGrid);
#endif

            Debug.Log($"Tilemap 크기 조정됨: {newCellSize}");
        }

    }
}