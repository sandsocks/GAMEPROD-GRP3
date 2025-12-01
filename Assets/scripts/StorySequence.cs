using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorySequence : MonoBehaviour
{
    [Header("Story Steps")]
    public List<StoryStep> steps = new List<StoryStep>();

    [Header("Fade Settings")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 1f;
    [Range(0f, 1f)]
    public float targetAlpha = 0.5f; // transparency while visible

    private bool dialogueFinished = false;

    private void OnEnable()
    {
        DialogueEvents.OnDialogueEnded += OnDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueEvents.OnDialogueEnded -= OnDialogueEnded;
    }

    private void Start()
    {
        // Disable all models at start
        foreach (var step in steps)
        {
            if (step.models == null) continue;

            foreach (var model in step.models)
            {
                if (model != null)
                    model.SetActive(false);
            }
        }
    }

    public void BeginSequence()
    {
        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        foreach (StoryStep step in steps)
        {
            if (step.models != null)
            {
                // Enable models first
                foreach (var model in step.models)
                    if (model != null) model.SetActive(true);

                // Fade in to targetAlpha
                yield return FadeModels(step.models, 0f, targetAlpha, fadeInTime);
            }

            // Play Dialogue
            if (step.dialogue != null)
            {
                dialogueFinished = false;
                DialogueManager.Instance.StartDialogue(step.dialogue);

                while (!dialogueFinished)
                    yield return null;
            }

            // Fade out to 0
            if (step.models != null)
                yield return FadeModels(step.models, targetAlpha, 0f, fadeOutTime);

            // Disable models after fade-out
            if (step.models != null)
            {
                foreach (var model in step.models)
                    if (model != null) model.SetActive(false);
            }
        }
    }

    private IEnumerator FadeModels(GameObject[] models, float from, float to, float duration)
    {
        float t = 0f;
        Renderer[][] renderers = new Renderer[models.Length][];

        // Cache renderers
        for (int i = 0; i < models.Length; i++)
        {
            if (models[i] != null)
                renderers[i] = models[i].GetComponentsInChildren<Renderer>(true);
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);

            foreach (var group in renderers)
            {
                if (group == null) continue;

                foreach (var rend in group)
                {
                    foreach (var mat in rend.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color c = mat.color;
                            c.a = alpha;
                            mat.color = c;
                        }
                    }
                }
            }

            yield return null;
        }
    }

    private void OnDialogueEnded()
    {
        dialogueFinished = true;
    }
}
