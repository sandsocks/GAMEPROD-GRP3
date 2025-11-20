using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Setup")]
    public List<Quest> allQuests = new List<Quest>();

    [Header("UI")]
    public TextMeshProUGUI activeQuestsText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateQuestUI();
    }

    // 🟢 Start quest
    public void StartQuest(string questId)
    {
        Quest quest = GetQuestById(questId);
        if (quest == null)
        {
            Debug.LogWarning($"Quest '{questId}' not found!");
            return;
        }

        if (quest.isCompleted)
        {
            Debug.Log($"Quest '{quest.questName}' already completed.");
            return;
        }

        quest.isActive = true;
        Debug.Log($"Started quest: {quest.questName}");
        UpdateQuestUI();
    }

    // 🟠 Add progress to an objective
    public void AddObjectiveProgress(string questId, string objectiveId, int amount = 1)
    {
        Quest quest = GetQuestById(questId);
        if (quest == null || !quest.isActive)
        {
            Debug.LogWarning($"Quest '{questId}' is not active or doesn't exist!");
            return;
        }

        QuestObjective objective = quest.objectives.Find(o => o.objectiveId == objectiveId);
        if (objective == null)
        {
            Debug.LogWarning($"Objective '{objectiveId}' not found in quest '{questId}'.");
            return;
        }

        objective.AddProgress(amount);
        Debug.Log($"Objective progress: {objective.description} ({objective.currentCount}/{objective.targetCount})");

        if (quest.AreAllObjectivesCompleted())
        {
            CompleteQuest(questId);
        }

        UpdateQuestUI();
    }

    // 🟡 Complete a quest (only called internally when all objectives are done)
    public void CompleteQuest(string questId)
    {
        Quest quest = GetQuestById(questId);
        if (quest == null) return;

        quest.isActive = false;
        quest.isCompleted = true;

        Debug.Log($"Quest completed: {quest.questName}");

        // Start next quest(s)
        foreach (var nextId in quest.nextQuestIds)
        {
            Quest next = GetQuestById(nextId);
            if (next != null && !next.isCompleted)
            {
                StartQuest(nextId);
            }
        }

        UpdateQuestUI();
    }

    // 🔍 Get quest by ID
    public Quest GetQuestById(string questId)
    {
        return allQuests.Find(q => q.questId == questId);
    }

    // 🧾 Update UI text
    private void UpdateQuestUI()
    {
        if (activeQuestsText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b><size=120%>Active Quests</size></b>\n");

        foreach (var quest in allQuests)
        {
            if (!quest.isActive) continue;

            sb.AppendLine($"<b>{quest.questName}</b>");
            sb.AppendLine($"{quest.description}");

            foreach (var obj in quest.objectives)
            {
                string checkbox = obj.isCompleted ? "/" : "O";
                sb.AppendLine($"   {checkbox} {obj.description} ({obj.currentCount}/{obj.targetCount})");
            }

            sb.AppendLine();
        }

        activeQuestsText.text = sb.ToString();
    }
}
