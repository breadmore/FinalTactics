using GoogleSheetsToUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Reader", menuName = "Data Reader/ActionDataReader", order = int.MaxValue)]
public class ActionDataReader : DataReaderBase
{
    [Header("���������Ʈ���� ������ ����ȭ �� ������Ʈ")]
    [SerializeField] public List<ActionData> DataList = new List<ActionData>();

    public void UpdateStats(List<GSTU_Cell> list, int actionID)
    {
        int id = 0;
        ActionType action = ActionType.None;
        int type = 0;

        for (int i = 0; i < list.Count; i++)
        {
            switch (list[i].columnId)
            {
                case "id":
                    id = int.Parse(list[i].value);
                    break;
                case "name":
                    if (Enum.TryParse(list[i].value, out ActionType parsedAction))
                    {
                        action = parsedAction;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid action name: {list[i].value}");
                        action = ActionType.None;  // ���� ó��
                    }
                    break;
                case "type":
                    type = int.Parse(list[i].value);
                    break;
              
            }
        }

        DataList.Add(new ActionData(id, action, type));
    }

    public ActionData GetActionDataById(int? actionID)
    {
        return DataList.Find(data => data.id == actionID);
    }
}
