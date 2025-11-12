using UnityEngine;
using TMPro;
using System.Collections;

public class Interactable : MonoBehaviour
{
    public enum InteractionType { PromptOnly, Dialogue, Note, GiveItem }

    [Header("Interaction Type")]
    public InteractionType interactionType = InteractionType.Dialogue;

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
    public DialogueData dialogueData; // 🎯 ScriptableObject assigned here
    public bool repeatable = true;

    [Header("Output Settings")]
    public bool removeAfterInteraction = false;

    [Header("Required Item")]
    public string requiredItem;

    [Header("Gives Item On Interaction")]
    public bool givesItem = false;
    public string itemName = "New Item";
    [TextArea] public string itemDescription = "Item description goes here.";
    public Sprite itemIcon;

    [Header("Note Interaction")]
    [TextArea(4, 10)] public string noteText = "This is the note text.";
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

    private bool playerInRange = false;
    private bool dialogueActive = false;
    private bool dialogueFinished = false;
    private bool noteOpen = false;
    private bool promptDismissed = false;

    private void Start()
    {
        if (promptText != null) promptText.alpha = 0;
        if (notePanel != null) notePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        promptDismissed = false;

        // 🎯 Auto quest trigger
        if (triggerQuestOnEnter)
            TriggerQuest();

        if (promptText != null && (!dialogueFinished || repeatable))
        {
            promptText.text = promptMessage;
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        promptDismissed = false;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

        if (interactionType == InteractionType.Note && noteOpen)
            CloseNote();
    }

    private void Update()
    {
        if (noteOpen && Input.GetKeyDown(closeNoteKey))
        {
            CloseNote();
            return;
        }

        if (playerInRange && !dialogueActive && (!dialogueFinished || repeatable) && !noteOpen)
        {
            if (requireKeyPress && Input.GetKeyDown(interactKey))
                TryInteraction();
            else if (!requireKeyPress)
                TryInteraction();
        }
    }

    private void TryInteraction()
    {
        // Item check
        if (!string.IsNullOrEmpty(requiredItem) && !InventoryManager.Instance.HasItem(requiredItem))
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.ShowTemporaryMessage(missingItemMessage);
            return;
        }

        TriggerExtras();

        switch (interactionType)
        {
            case InteractionType.PromptOnly:
                HandlePromptOnly();
                break;

            case InteractionType.Dialogue:
                StartCoroutine(StartDialogue());
                break;

            case InteractionType.Note:
                OpenNote();
                break;

            case InteractionType.GiveItem:
                if (givesItem)
                {
                    InventoryManager.Instance.AddItem(itemIcon, itemName, itemDescription);
                    StartCoroutine(StartDialogue());
                }
                break;
        }

        // 🎯 Quest trigger (after interaction)
        if (triggerQuestOnInteraction)
            TriggerQuest();
    }

    private void TriggerExtras()
    {
        // Animation
        if (triggerAnimation && animator != null && !string.IsNullOrEmpty(animationTriggerName))
            animator.SetTrigger(animationTriggerName);

        // SFX
        if (playSFX && interactionSFX != null)
        {
            AudioSource source = audioSource != null ? audioSource : DialogueManager.Instance?.voiceSource;
            if (source != null)
                source.PlayOneShot(interactionSFX);
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

    private void HandlePromptOnly()
    {
        if (promptDismissed || promptText == null) return;
        StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));
        promptDismissed = true;
    }

    private void OpenNote()
    {
        if (notePanel != null && noteTextUI != null)
        {
            notePanel.SetActive(true);
            noteTextUI.text = noteText;
            noteOpen = true;

            if (promptText != null)
                StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));
        }
    }

    private void CloseNote()
    {
        if (notePanel != null)
        {
            notePanel.SetActive(false);
            noteOpen = false;
            dialogueFinished = true;

            if (repeatable && playerInRange && promptText != null)
                StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));

            if (removeAfterInteraction && (!repeatable || !playerInRange))
                Destroy(gameObject);
        }
    }

    private IEnumerator StartDialogue()
    {
        if (dialogueData == null || DialogueManager.Instance == null)
        {
            Debug.LogWarning($"Interactable '{name}' has no DialogueData or DialogueManager assigned!");
            yield break;
        }

        dialogueActive = true;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

        DialogueManager.Instance.StartDialogue(dialogueData);

        while (DialogueManager.Instance.IsDialogueRunning)
            yield return null;

        dialogueActive = false;
        dialogueFinished = true;

        if (repeatable && playerInRange && promptText != null)
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));

        if (removeAfterInteraction && (!repeatable || !playerInRange))
            Destroy(gameObject);
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
