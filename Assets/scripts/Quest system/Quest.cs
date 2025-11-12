using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Quest
{
    public string questId;                   // Unique ID
    public string questName;
    [TextArea] public string description;

    public bool isActive;
    public bool isCompleted;

    [Header("Next Quest(s) after completion")]
    public List<string> nextQuestIds = new List<string>();

    public Quest(string id, string name, string desc)
    {
        questId = id;
        questName = name;
        description = desc;
        isActive = false;
        isCompleted = false;
    }
}
