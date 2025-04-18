using System;
using UnityEngine;

[Serializable]
public class ActionData
{
    public int id;
    public ActionType action;
    public ActionCategory category;

    public ActionData(int id, ActionType action, ActionCategory category)
    {
        this.id = id;
        this.action = action;
        this.category = category;
    }
}

