#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilemapScaler))]
public class TilemapScalerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TilemapScaler scaler = (TilemapScaler)target;
        if (GUILayout.Button("Update Tilemap Size"))
        {
            scaler.ResizeTilemap();
            EditorUtility.SetDirty(scaler.tilemapGrid);
        }
    }
}
#endif