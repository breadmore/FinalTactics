using System;
using UnityEngine;

[Serializable]
public class ActionData
{
    public int id;
    public ActionType action;

    public ActionData(int id, ActionType action)
    {
        this.id = id;
        this.action = action;
    }
}

