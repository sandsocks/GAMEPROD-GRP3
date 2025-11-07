using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueData dialogueData;  // ✅ ScriptableObject reference
    public float typeSpeed = 0.03f;
    public float fadeSpeed = 1f;
    public float autoNextDelay = 2f;

    [Header("UI References")]
    public CanvasGroup dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image characterImageUI;

    [Header("Audio")]
    public AudioSource voiceSource;

    private static Queue<DialogueTrigger> dialogueQueue = new Queue<DialogueTrigger>();
    private static bool dialogueRunning = false;

    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.alpha = 0f;
            dialoguePanel.gameObject.SetActive(true);
        }

        if (voiceSource != null)
            voiceSource.playOnAwake = false;
    }

    public void StartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("DialogueTrigger: No DialogueData assigned!");
            return;
        }

        if (!dialogueQueue.Contains(this))
            dialogueQueue.Enqueue(this);

        TryStartNextDialogue();
    }

    private static void TryStartNextDialogue()
    {
        if (!dialogueRunning && dialogueQueue.Count > 0)
        {
            DialogueTrigger next = dialogueQueue.Dequeue();
            next.RunDialogue();
        }
    }

    private void RunDialogue()
    {
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
        if (dialogueData == null) return;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        dialogueText.text = "";

        var lineData = dialogueData.lines[currentLine];

        // Update UI
        if (nameText != null) nameText.text = lineData.speakerName;
        if (characterImageUI != null) characterImageUI.sprite = lineData.characterImage;

        // Play voice
        if (voiceSource != null && lineData.voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = lineData.voiceClip;
            voiceSource.Play();
        }

        typingCoroutine = StartCoroutine(TypeLine(lineData.text));
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
                dialogueText.text = dialogueData.lines[currentLine].text;
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
        if (currentLine < dialogueData.lines.Length)
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

        if (voiceSource != null) voiceSource.Stop();

        TryStartNextDialogue();
    }
}
