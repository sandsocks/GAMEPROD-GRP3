using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public string objectiveId;
    public string description;
    public int currentCount;
    public int targetCount = 1;
    public bool isCompleted;

    public QuestObjective(string id, string desc, int target = 1)
    {
        objectiveId = id;
        description = desc;
        targetCount = target;
        currentCount = 0;
        isCompleted = false;
    }

    public void AddProgress(int amount = 1)
    {
        if (isCompleted) return;

        currentCount += amount;
        if (currentCount >= targetCount)
        {
            currentCount = targetCount;
            isCompleted = true;
        }
    }
}
