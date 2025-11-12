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
    public TextMeshProUGUI activeQuestsText; // assign in Inspector

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

    // 🟢 Start a quest
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

        if (quest.isActive)
        {
            Debug.Log($"Quest '{quest.questName}' is already active.");
            return;
        }

        quest.isActive = true;
        Debug.Log($"Started quest: {quest.questName}");
        UpdateQuestUI();
    }

    // 🟡 Complete quest
    public void CompleteQuest(string questId)
    {
        Quest quest = GetQuestById(questId);
        if (quest == null)
        {
            Debug.LogWarning($"Quest '{questId}' not found!");
            return;
        }

        if (!quest.isActive)
        {
            Debug.LogWarning($"Quest '{quest.questName}' hasn't been started.");
            return;
        }

        quest.isActive = false;
        quest.isCompleted = true;

        Debug.Log($"Completed quest: {quest.questName}");

        // 🔗 Unlock next quests
        foreach (string nextId in quest.nextQuestIds)
        {
            Quest next = GetQuestById(nextId);
            if (next != null && !next.isCompleted)
            {
                StartQuest(nextId);
            }
        }

        UpdateQuestUI();
    }

    // 🔍 Get a quest by ID
    public Quest GetQuestById(string questId)
    {
        return allQuests.Find(q => q.questId == questId);
    }

    // 🧾 Update the on-screen UI
    private void UpdateQuestUI()
    {
        if (activeQuestsText == null) return;

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("<b><size=120%>Active Quests</size></b>\n");

        foreach (var quest in allQuests)
        {
            if (quest.isActive)
            {
                sb.AppendLine($"• <b>{quest.questName}</b>");
                sb.AppendLine($"   {quest.description}\n");
            }
        }

        activeQuestsText.text = sb.ToString();
    }

    // Optional helper for debugging
    public void PrintAllQuests()
    {
        foreach (var quest in allQuests)
        {
            Debug.Log($"{quest.questName} | Active: {quest.isActive} | Completed: {quest.isCompleted}");
        }
    }
}
