#if UNITY_EDITOR
using GoogleSheetsToUnity;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine;

[CustomEditor(typeof(CharacterDataReader))]
public class CharacterDataReaderEditor : Editor
{
    CharacterDataReader data;

    void OnEnable()
    {
        data = (CharacterDataReader)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("\n\n스프레드 시트 읽어오기");

        if (GUILayout.Button("데이터 읽기(API 호출)"))
        {
            UpdateStats(UpdateMethodOne);
            data.DataList.Clear();
        }
    }

    void UpdateStats(UnityAction<GstuSpreadSheet> callback, bool mergedCells = false)
    {
        SpreadsheetManager.Read(new GSTU_Search(data.associatedSheet, data.associatedWorksheet), callback, mergedCells);
    }

    void UpdateMethodOne(GstuSpreadSheet ss)
    {
        // ss.rows는 List<GSTU_Cell> 형식으로 되어 있으므로 이를 전달해야 합니다.
        for (int i = data.START_ROW_LENGTH; i <= data.END_ROW_LENGTH; ++i)
        {
            // 각 행 데이터를 UpdateStats에 전달
            data.UpdateStats(ss.rows[i], i);
        }

        EditorUtility.SetDirty(target);  // 편집 내용을 저장하여 반영
    }
}
#endif
