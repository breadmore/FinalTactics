using GoogleSheetsToUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Reader", menuName = "Data Reader/ActionDataReader", order = int.MaxValue)]
public class ActionDataReader : DataReaderBase
{
    [Header("���������Ʈ���� ������ ����ȭ �� ������Ʈ")]
    [SerializeField] public List<ActionData> DataList = new List<ActionData>();

    public void UpdateAction(List<GSTU_Cell> list, int actionID)
    {
        int id = 0;
        ActionType actionType = ActionType.None;
        ActionCategory category = ActionCategory.Common;
        bool hasOption = false;
        List<string> options = new List<string>();
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
                        actionType = parsedAction;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid action name: {list[i].value}");
                        actionType = ActionType.None;  // ���� ó��
                    }
                    break;
                case "type":
                    if (Enum.TryParse(list[i].value, out ActionCategory actionCategory))
                    {
                        category = actionCategory;
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid action name: {list[i].value}");
                        category = ActionCategory.Common;  // ���� ó��
                    }
                    break;
                case "hasOption":
                    if (bool.TryParse(list[i].value, out bool optionFlag))
                    {
                        hasOption = optionFlag;
                    }
                    else
                    {
                        // "TRUE"/"FALSE" �Ǵ� "1"/"0" ���ĵ� ó�� �����ϵ��� Ȯ��
                        hasOption = list[i].value.ToLower() == "true" || list[i].value == "1";
                        Debug.LogWarning($"hasOption parsed as {hasOption} from {list[i].value}");
                    }
                    break;
                case "options":
                    if (!string.IsNullOrEmpty(list[i].value))
                    {
                        options = list[i].value.Split(',')
                            .Select(opt => opt.Trim())
                            .Where(opt => !string.IsNullOrEmpty(opt))
                            .ToList();
                    }
                    break;

            }
        }

        DataList.Add(new ActionData(id, actionType, category,hasOption,options));
    }

    public ActionData GetActionDataById(int? actionID)
    {
        return DataList.Find(data => data.id == actionID);
    }

    public List<ActionData> GetActionDataList()
    {
        return DataList;
    }
}
