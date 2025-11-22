using UnityEngine;
using TMPro;
using System.Collections;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { PromptOnly, Dialogue, Note, GiveItem }

    [Header("Interaction Types (Multiple Allowed)")]
    public InteractionType[] interactionTypes;

    [Header("UI References")]
    public TextMeshProUGUI promptText;
    public GameObject notePanel;
    public TextMeshProUGUI noteTextUI;

    [Header("Prompt Settings")]
    public string promptMessage = "Press E to interact";
    public string missingItemMessage = "You need a specific item!";
    public KeyCode interactKey = KeyCode.E;
    public bool requireKeyPress = true;
    public float promptFadeInDuration = 0.5f;
    public float promptFadeOutDuration = 0.5f;

    [Header("Dialogue Settings")]
    public DialogueData dialogueData;
    public bool dialogueRepeatable = false;

    [Header("Output")]
    public bool removeAfterInteraction = false;
    public float removeDelay = 0.3f;

    [Header("Required Item")]
    public string requiredItem;

    [Header("Give Item")]
    public bool givesItem = false;
    public bool giveItemRepeatable = false;
    public string itemName = "New Item";
    public string itemDescription = "Item description";
    public Sprite itemIcon;

    [Header("Note")]
    [TextArea(4, 10)] public string noteText = "This is the note text.";
    public bool noteRepeatable = true;
    public KeyCode closeNoteKey = KeyCode.E;

    [Header("Animation & Audio")]
    public bool triggerAnimation = false;
    public Animator animator;
    public string animationTriggerName = "Activate";

    public bool playSFX = false;
    public AudioSource audioSource;
    public AudioClip interactionSFX;

    [Header("Quest Integration")]
    public bool triggerQuestOnInteraction = false;
    public bool triggerQuestOnEnter = false;
    public string questToStart;
    public string questToComplete;

    [Header("Quest Objective Update")]
    public bool updateQuestObjective = false;
    public string objectiveQuestID;
    public string objectiveID;
    public int objectiveAmount = 1;

    // Internal states
    private bool dialogueUsed = false;
    private bool noteUsed = false;
    private bool giveItemUsed = false;
    private bool promptUsed = false;

    private bool playerInRange = false;
    private bool dialogueActive = false;
    private bool noteOpen = false;

    private void Start()
    {
        if (promptText != null) promptText.alpha = 0;
        if (notePanel != null) notePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        promptUsed = false;

        if (triggerQuestOnEnter)
            TriggerQuest();

        if (promptText != null)
        {
            promptText.text = promptMessage;
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

        if (noteOpen)
            CloseNote();
    }

    private void Update()
    {
        // Close note
        if (noteOpen && Input.GetKeyDown(closeNoteKey))
        {
            CloseNote();
            return;
        }

        // Interaction
        if (playerInRange && !dialogueActive && !noteOpen)
        {
            if (requireKeyPress && Input.GetKeyDown(interactKey))
                StartCoroutine(TryInteraction());
            else if (!requireKeyPress)
                StartCoroutine(TryInteraction());
        }
    }

    private IEnumerator TryInteraction()
    {
        // Missing item prompt
        if (!string.IsNullOrEmpty(requiredItem) &&
            !InventoryManager.Instance.HasItem(requiredItem))
        {
            if (promptText != null)
            {
                promptText.text = missingItemMessage;
                promptText.alpha = 1;
            }
            yield break;
        }

        TriggerExtras();

        bool dialogueRequested = false;

        // ---- Run all types in parallel but respecting "used" flags ----
        foreach (var type in interactionTypes)
        {
            if (type == InteractionType.Dialogue)
            {
                // Mark that dialogue is needed (but don't start yet)
                if (!dialogueUsed || dialogueRepeatable)
                    dialogueRequested = true;
            }
            else
            {
                // Non-dialogue interactions execute immediately
                RunInteractionType(type);
            }
        }

        // ---- Dialogue must run LAST because it blocks movement / input ----
        if (dialogueRequested)
            yield return StartCoroutine(RunDialogue());

        if (triggerQuestOnInteraction)
            TriggerQuest();

        if (updateQuestObjective)
            TriggerQuestObjective();

        if (removeAfterInteraction)
        {
            yield return new WaitForSeconds(removeDelay);
            Destroy(gameObject);
        }
    }



    private void RunInteractionType(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.PromptOnly:
                if (!promptUsed)
                {
                    HandlePromptOnly();
                    promptUsed = true;
                }
                break;

            case InteractionType.Note:
                if (!noteUsed || noteRepeatable)
                {
                    OpenNote();
                    noteUsed = true;
                }
                break;

            case InteractionType.GiveItem:
                if (givesItem && (!giveItemUsed || giveItemRepeatable))
                {
                    InventoryManager.Instance.AddItem(itemIcon, itemName, itemDescription);
                    giveItemUsed = true;
                }
                break;

                // Dialogue is handled in TryInteraction() to ensure correct timing
        }
    }



    private void TriggerExtras()
    {
        if (triggerAnimation && animator != null)
            animator.SetTrigger(animationTriggerName);

        if (playSFX && interactionSFX != null)
        {
            AudioSource src = audioSource != null ? audioSource : DialogueManager.Instance?.voiceSource;
            if (src != null)
                src.PlayOneShot(interactionSFX);
        }
    }

    private void TriggerQuest()
    {
        if (QuestManager.Instance == null) return;

        if (!string.IsNullOrEmpty(questToStart))
            QuestManager.Instance.StartQuest(questToStart);

        if (!string.IsNullOrEmpty(questToComplete))
            QuestManager.Instance.CompleteQuest(questToComplete);
    }

    private void TriggerQuestObjective()
    {
        if (QuestManager.Instance == null) return;

        if (!string.IsNullOrEmpty(objectiveQuestID) &&
            !string.IsNullOrEmpty(objectiveID))
        {
            QuestManager.Instance.AddObjectiveProgress(objectiveQuestID, objectiveID, objectiveAmount);
        }
    }

    private void HandlePromptOnly()
    {
        if (promptUsed || promptText == null) return;

        StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));
        promptUsed = true;
    }

    private void OpenNote()
    {
        if (notePanel == null || noteTextUI == null) return;

        notePanel.SetActive(true);
        noteTextUI.text = noteText;
        noteOpen = true;
        noteUsed = true;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));
    }

    private void CloseNote()
    {
        if (notePanel == null) return;

        notePanel.SetActive(false);
        noteOpen = false;

        if (noteRepeatable && playerInRange && promptText != null)
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));
    }

    private IEnumerator RunDialogue()
    {
        if (dialogueData == null || DialogueManager.Instance == null)
            yield break;

        dialogueActive = true;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

        DialogueManager.Instance.StartDialogue(dialogueData);

        while (DialogueManager.Instance.IsDialogueRunning)
            yield return null;

        dialogueActive = false;
        dialogueUsed = true;

        if (dialogueRepeatable && playerInRange && promptText != null)
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));
    }

    private IEnumerator FadeText(TextMeshProUGUI textElement, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            textElement.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        textElement.alpha = to;
    }
}
