using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [TextArea(2, 5)] public string[] dialogueLines;
    public float typeSpeed = 0.03f;
    public float fadeSpeed = 1f;
    public float autoNextDelay = 2f;
    public bool canRetrigger = false; // allow re-trigger after finish

    [Header("UI References")]
    public CanvasGroup dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private static Queue<DialogueTrigger> dialogueQueue = new Queue<DialogueTrigger>();
    private static bool dialogueRunning = false;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool hasTriggered = false; // ✅ prevents multi-trigger spam
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.gameObject.SetActive(true);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered && !canRetrigger) return; // ✅ stop duplicates
        hasTriggered = true;

        if (!dialogueQueue.Contains(this))
            dialogueQueue.Enqueue(this);

        TryStartNextDialogue();
    }

    private static void TryStartNextDialogue()
    {
        if (!dialogueRunning && dialogueQueue.Count > 0)
        {
            DialogueTrigger next = dialogueQueue.Dequeue();
            next.StartDialogue();
        }
    }

    private void StartDialogue()
    {
        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("DialogueTrigger: Missing UI references!");
            return;
        }

        dialogueRunning = true;
        currentLine = 0;
        StartCoroutine(FadeInAndStart());
    }

    private IEnumerator FadeInAndStart()
    {
        dialoguePanel.gameObject.SetActive(true);
        float a = 0f;
        while (a < 1f)
        {
            a += Time.deltaTime * fadeSpeed;
            dialoguePanel.alpha = a;
            yield return null;
        }
        dialoguePanel.alpha = 1f;
        DisplayLine();
    }

    private void DisplayLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = "";
        typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLine]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        yield return new WaitForSeconds(autoNextDelay);
        NextLine();
    }

    private void Update()
    {
        if (!dialogueRunning) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueLines[currentLine];
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    private void NextLine()
    {
        currentLine++;
        if (currentLine < dialogueLines.Length)
        {
            DisplayLine();
        }
        else
        {
            StartCoroutine(FadeOutAndFinish());
        }
    }

    private IEnumerator FadeOutAndFinish()
    {
        float a = 1f;
        while (a > 0f)
        {
            a -= Time.deltaTime * fadeSpeed;
            dialoguePanel.alpha = a;
            yield return null;
        }

        dialoguePanel.alpha = 0f;
        dialogueRunning = false;

        if (canRetrigger) hasTriggered = false; // reset for reuse

        TryStartNextDialogue();
    }
}
