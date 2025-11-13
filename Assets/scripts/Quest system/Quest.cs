using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Quest
{
    public string questId;
    public string questName;
    [TextArea] public string description;

    public bool isActive;
    public bool isCompleted;

    [Header("Objectives")]
    public List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Next Quest(s) after completion")]
    public List<string> nextQuestIds = new List<string>();

    public bool AreAllObjectivesCompleted()
    {
        foreach (var obj in objectives)
        {
            if (!obj.isCompleted)
                return false;
        }
        return true;
    }
}
