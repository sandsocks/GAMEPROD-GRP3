using UnityEngine;
using TMPro;
using System.Collections;

public class Interactable : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI promptText;
    public TextMeshProUGUI dialogueText;

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

    [Header("Output Settings (Object Removal)")]
    public bool removeAfterInteraction = false;


    [Header("Required Item")]
    public string requiredItem; // leave blank if no requirement

    [Header("Gives Item On Interaction")]
    public bool givesItem = false;
    public string itemName = "New Item";
    [TextArea] public string itemDescription = "Item description goes here.";
    public Sprite itemIcon;

    private bool playerInRange = false;
    private bool dialogueActive = false;
    private bool dialogueFinished = false;

    private void Start()
    {
        if (promptText != null) promptText.alpha = 0;
        if (dialogueText != null) dialogueText.alpha = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
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
            if (promptText != null)
                StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));
        }
    }

    private void Update()
    {
        if (playerInRange && !dialogueActive && (!dialogueFinished || repeatable))
        {
            if (requireKeyPress && Input.GetKeyDown(interactKey))
            {
                TryInteraction();
            }
            else if (!requireKeyPress)
            {
                TryInteraction();
            }
        }
    }

    private void TryInteraction()
    {
        // Requirement check
        if (!string.IsNullOrEmpty(requiredItem) && !InventoryManager.Instance.HasItem(requiredItem))
        {
            // Show "missing item" feedback only, no outputs
            if (dialogueText != null)
            {
                dialogueText.text = missingItemMessage;
                StartCoroutine(TemporaryMessage());
            }
            return;
        }

        // Give item if configured
        if (givesItem)
        {
            InventoryManager.Instance.AddItem(itemIcon, itemName, itemDescription);

            // Auto-generate pickup dialogue if none was provided
            if (string.IsNullOrEmpty(dialogueMessage) || dialogueMessage == "Hello, this is the dialogue.")
            {
                dialogueMessage = $"You picked up {itemName}!";
            }
        }

        // Run full output (dialogue + removal, etc.)
        StartCoroutine(Output());
    }

    /// <summary>
    /// Handles dialogue + object removal (the "output" block).
    /// </summary>
    private IEnumerator Output()
    {
        dialogueActive = true;

        if (promptText != null)
            StartCoroutine(FadeText(promptText, promptText.alpha, 0, promptFadeOutDuration));

        if (dialogueText != null)
        {
            dialogueText.text = dialogueMessage;

            // Dialogue fade in
            yield return StartCoroutine(FadeText(dialogueText, 0, 1, dialogueFadeInDuration));

            // Display duration
            yield return new WaitForSeconds(dialogueDisplayDuration);

            // Dialogue fade out
            yield return StartCoroutine(FadeText(dialogueText, dialogueText.alpha, 0, dialogueFadeOutDuration));
        }

        dialogueActive = false;
        dialogueFinished = true;

        // Show prompt again if repeatable
        if (repeatable && playerInRange && promptText != null)
        {
            StartCoroutine(FadeText(promptText, 0, 1, promptFadeInDuration));
        }

        // Handle removal
        if (removeAfterInteraction && (!repeatable || !playerInRange))
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Shows a quick "missing item" message but does not unlock outputs.
    /// </summary>
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
