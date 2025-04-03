using System;
using UnityEngine;

[Serializable]
public class ActionData
{
    public int id;
    public ActionType action;
    public int type;

    public ActionData(int id, ActionType action, int type)
    {
        this.id = id;
        this.action = action;
        this.type = type;
    }
}

