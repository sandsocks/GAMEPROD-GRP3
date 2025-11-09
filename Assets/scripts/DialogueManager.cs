using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image characterImage;

    [Header("Audio")]
    public AudioSource voiceSource;

    [Header("Settings")]
    public float typingSpeed = 0.03f;
    public float fadeOutDelay = 1.5f;

    private DialogueData currentDialogue;
    private int currentLineIndex;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private bool isRunning;

    public bool IsDialogueRunning => isRunning;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (voiceSource == null)
            voiceSource = gameObject.AddComponent<AudioSource>();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // ------------------- Dialogue -------------------
    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null || dialogueData.lines.Length == 0 || isRunning) return;

        currentDialogue = dialogueData;
        currentLineIndex = 0;
        isRunning = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowLine();
    }

    private void ShowLine()
    {
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentDialogue.lines[currentLineIndex];

        // UI updates
        if (nameText != null) nameText.text = string.IsNullOrEmpty(line.speakerName) ? "" : line.speakerName;
        if (characterImage != null)
        {
            characterImage.sprite = line.characterImage;
            characterImage.enabled = line.characterImage != null;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeLine(line.text));

        // Play voice
        if (voiceSource != null && line.voiceClip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = line.voiceClip;
            voiceSource.Play();
        }
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void Update()
    {
        if (!isRunning) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                dialogueText.text = currentDialogue.lines[currentLineIndex].text;
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    public void NextLine()
    {
        if (!isRunning) return;

        currentLineIndex++;
        if (currentDialogue == null || currentLineIndex >= currentDialogue.lines.Length)
            EndDialogue();
        else
            ShowLine();
    }

    private void EndDialogue()
    {
        StartCoroutine(FadeOutAndFinish());
    }

    private IEnumerator FadeOutAndFinish()
    {
        yield return new WaitForSeconds(fadeOutDelay);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (voiceSource != null && voiceSource.isPlaying)
            voiceSource.Stop();

        currentDialogue = null;
        currentLineIndex = 0;
        isRunning = false;
    }

    // ------------------- Temporary Message -------------------
    public void ShowTemporaryMessage(string message, float displayTime = 2f)
    {
        if (isRunning) return;
        StartCoroutine(RunTemporaryMessage(message, displayTime));
    }

    private IEnumerator RunTemporaryMessage(string message, float displayTime)
    {
        isRunning = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = message;
        }

        yield return new WaitForSeconds(displayTime);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        isRunning = false;
    }
}
