using System;
using UnityEngine;

[Serializable]
public class ActionCounter
{
    public int shootCount;
    public int passCount;
    public int dribbleCount;
    public int blockCount;
    public int tackleCount;
    public int interceptCount;
    public int saveCount;

    public void ResetCounts()
    {
        shootCount = 0;
        passCount = 0;
        dribbleCount = 0;
        blockCount = 0;
        tackleCount = 0;
        interceptCount = 0;
        saveCount = 0;
    }

    public void IncrementCount(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Shoot: shootCount++; break;
            case ActionType.Pass: passCount++; break;
            case ActionType.Dribble: dribbleCount++; break;
            case ActionType.Block: blockCount++; break;
            case ActionType.Tackle: tackleCount++; break;
            case ActionType.Intercept: interceptCount++; break;
            case ActionType.Save: saveCount++; break;
        }
    }
}