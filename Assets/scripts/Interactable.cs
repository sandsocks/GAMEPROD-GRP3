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
    public TextMeshProUGUI dialogueText;
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
    [TextArea] public string dialogueMessage = "Hello, this is the dialogue.";
    public float dialogueFadeInDuration = 1f;
    public float dialogueFadeOutDuration = 1f;
    public float dialogueDisplayDuration = 3f;
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

    [Header("Dialogue Trigger")]
    public bool triggerDialogue = false;
    public DialogueTrigger dialogueTrigger; // External dialogue trigger reference

    private bool playerInRange = false;
    private bool dialogueActive = false;
    private bool dialogueFinished = false;
    private bool noteOpen = false;
    private bool promptDismissed = false;

    private void Start()
    {
        if (promptText != null) promptText.alpha = 0;
        if (dialogueText != null) dialogueText.alpha = 0;
        if (notePanel != null) notePanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            promptDismissed = false;

            if (promptText != null && (!dialogueFinished || repeatable))
            {
                promptText.text = promptMessage;
                StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            promptDismissed = false;

            if (promptText != null)
                StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

            if (interactionType == InteractionType.Note && noteOpen)
                CloseNote();
        }
    }

    private void Update()
    {
        // ✅ Close note first if open
        if (noteOpen && Input.GetKeyDown(closeNoteKey))
        {
            CloseNote();
            return;
        }

        // ✅ Handle interaction
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
        // ✅ Requirement check
        if (!string.IsNullOrEmpty(requiredItem) && !InventoryManager.Instance.HasItem(requiredItem))
        {
            if (dialogueText != null)
            {
                dialogueText.text = missingItemMessage;
                StartCoroutine(TemporaryMessage());
            }
            return;
        }

        // ✅ Animation / Audio / Dialogue trigger (works with any type)
        TriggerExtras();

        // ✅ Perform interaction type
        switch (interactionType)
        {
            case InteractionType.PromptOnly:
                HandlePromptOnly();
                break;

            case InteractionType.Dialogue:
                StartCoroutine(Output());
                break;

            case InteractionType.Note:
                OpenNote();
                break;

            case InteractionType.GiveItem:
                if (givesItem)
                {
                    InventoryManager.Instance.AddItem(itemIcon, itemName, itemDescription);
                    dialogueMessage = $"You picked up {itemName}!";
                    StartCoroutine(Output());
                }
                break;
        }
    }

    // ---------------- EXTRAS ----------------
    private void TriggerExtras()
    {
        // 🎬 Animation
        if (triggerAnimation && animator != null && !string.IsNullOrEmpty(animationTriggerName))
            animator.SetTrigger(animationTriggerName);

        // 🔊 Sound effect
        if (playSFX && audioSource != null && interactionSFX != null)
            audioSource.PlayOneShot(interactionSFX);

        // 💬 Dialogue trigger
        if (triggerDialogue && dialogueTrigger != null)
            dialogueTrigger.StartDialogue();
    }

    // ---------------- PROMPT ONLY ----------------
    private void HandlePromptOnly()
    {
        if (promptDismissed || promptText == null) return;
        StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));
        promptDismissed = true;
    }

    // ---------------- NOTE ----------------
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

    // ---------------- DIALOGUE ----------------
    private IEnumerator Output()
    {
        dialogueActive = true;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

        if (dialogueText != null)
        {
            dialogueText.text = dialogueMessage;
            yield return StartCoroutine(FadeText(dialogueText, 0, 1, dialogueFadeInDuration));
            yield return new WaitForSeconds(dialogueDisplayDuration);
            yield return StartCoroutine(FadeText(dialogueText, dialogueText.alpha, 0, dialogueFadeOutDuration));
        }

        dialogueActive = false;
        dialogueFinished = true;

        if (repeatable && playerInRange && promptText != null)
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));

        if (removeAfterInteraction && (!repeatable || !playerInRange))
            Destroy(gameObject);
    }

    private IEnumerator TemporaryMessage()
    {
        yield return StartCoroutine(FadeText(dialogueText, 0, 1, 0.5f));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeText(dialogueText, dialogueText.alpha, 0, 0.5f));
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
