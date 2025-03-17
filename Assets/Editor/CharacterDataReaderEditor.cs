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

        GUILayout.Label("\n\n�������� ��Ʈ �о����");

        if (GUILayout.Button("������ �б�(API ȣ��)"))
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
        // ss.rows�� List<GSTU_Cell> �������� �Ǿ� �����Ƿ� �̸� �����ؾ� �մϴ�.
        for (int i = data.START_ROW_LENGTH; i <= data.END_ROW_LENGTH; ++i)
        {
            // �� �� �����͸� UpdateStats�� ����
            data.UpdateStats(ss.rows[i], i);
        }

        EditorUtility.SetDirty(target);  // ���� ������ �����Ͽ� �ݿ�
    }
}
#endif
