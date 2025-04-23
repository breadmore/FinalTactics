using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActionData
{
    public int id;
    public ActionType action;
    public ActionCategory category;
    public bool hasOption;
    public List<string> options = new List<string>();

    public ActionData(int id, ActionType action, ActionCategory category, bool hasOption, List<string> options)
    {
        this.id = id;
        this.action = action;
        this.category = category;
        this.hasOption = hasOption;
        this.options = options;
    }
}

[Serializable]
public class ActionOptionData 
{
    public int id;
    public int actionId;
    public string name;

    public ActionOptionData(int id, int actionId, string name)
    {
        this.id = id;
        this.actionId = actionId;
        this.name = name;
    }
}