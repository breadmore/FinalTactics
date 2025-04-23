using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class OptionSlotParent : BaseLayoutGroupParent<OptionSlotChild>
{

    public void InitChildOption(List<ActionOptionData> actionOptionDataList)
    {
        CreateChild(actionOptionDataList.Count);

        for(int i=0; i<actionOptionDataList.Count; i++)
        {
            InitChild(i, actionOptionDataList[i]);
        }

    }

    private void InitChild(int index, ActionOptionData actionOptionData)
    {
        var sprite = LoadDataManager.Instance.optionSlotBackgrounds.GetBackground(actionOptionData.id);
        if (sprite == null) return;

        childList[index].SetOption(actionOptionData);
        childList[index].SetOptionSprite(sprite);

    }
}
