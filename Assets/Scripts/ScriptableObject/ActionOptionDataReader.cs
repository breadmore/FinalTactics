using GoogleSheetsToUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Reader", menuName = "Data Reader/ActionOptionDataReader", order = int.MaxValue)]
public class ActionOptionDataReader : DataReaderBase
{
    [Header("스프레드시트에서 읽혀져 직렬화 된 오브젝트")]
    [SerializeField] public List<ActionOptionData> DataList = new List<ActionOptionData>();

    public void UpdateOptionData(List<GSTU_Cell> list)
    {
        int id = 0;
        int actionId = 0;
        string name = string.Empty;
        bool isValid = true;

        // 데이터 파싱
        for (int i = 0; i < list.Count; i++)
        {
            try
            {
                switch (list[i].columnId.ToLower()) // 대소문자 구분 없이 처리
                {
                    case "id":
                        if (!int.TryParse(list[i].value, out id))
                        {
                            Debug.LogError($"Invalid ID format: {list[i].value}");
                            isValid = false;
                        }
                        break;
                    case "actionid":
                        if (!int.TryParse(list[i].value, out actionId))
                        {
                            Debug.LogError($"Invalid ActionID format: {list[i].value}");
                            isValid = false;
                        }
                        break;
                    case "name":
                        name = list[i].value?.Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            Debug.LogError("Option name cannot be empty");
                            isValid = false;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing cell {list[i].columnId}: {e.Message}");
                isValid = false;
            }
        }

        // 데이터 유효성 검사
        if (!isValid)
        {
            Debug.LogError("Failed to parse action option data due to invalid values");
            return;
        }

        // 중복 ID 검사
        if (DataList.Any(x => x.id == id))
        {
            Debug.LogWarning($"Duplicate option ID detected: {id}. Updating existing entry.");
            var existing = DataList.FirstOrDefault(x => x.id == id);
            if (existing != null)
            {
                existing.actionId = actionId;
                existing.name = name;
            }
            return;
        }

        // 새 데이터 추가
        DataList.Add(new ActionOptionData(id, actionId, name));
        Debug.Log($"Successfully added/updated option: ID={id}, ActionID={actionId}, Name={name}");
    }

    public ActionOptionData GetActionOptionById(int optionId)
    {
        var data = DataList.Find(data => data.id == optionId);
        if (data == null)
        {
            Debug.LogWarning($"ActionOption with ID {optionId} not found");
        }
        return data;
    }

    public List<ActionOptionData> GetActionOptionsByActionId(int? actionId)
    {
        var options = DataList.Where(data => data.actionId == actionId).ToList();
        if (!options.Any())
        {
            Debug.LogWarning($"No options found for ActionID {actionId}");
        }
        return options;
    }

    public List<ActionOptionData> GetAllActionOptions()
    {
        return DataList;
    }
}