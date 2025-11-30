using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorySequence : MonoBehaviour
{
    [System.Serializable]
    public class GhostEvent
    {
        public GameObject ghostObject;      // The ghost model to show
        public int lineIndexToAppear;       // Dialogue line when it appears
        public float fadeInTime = 1f;
        public float fadeOutTime = 1f;

        [HideInInspector] public bool hasAppeared;
    }

    [Header("Story Timeline")]
    public List<GhostEvent> ghostEvents = new List<GhostEvent>();

    private int lastLineIndex = -1;

    private void OnEnable()
    {
        DialogueEvents.OnDialogueLineChanged += HandleLineChanged;
        DialogueEvents.OnDialogueEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueEvents.OnDialogueLineChanged -= HandleLineChanged;
        DialogueEvents.OnDialogueEnded -= HandleDialogueEnded;
    }

    private void Start()
    {
        // Hide all ghosts on start
        foreach (var ghost in ghostEvents)
        {
            if (ghost.ghostObject != null)
                SetGhostAlpha(ghost.ghostObject, 0f);
        }
    }

    private void HandleLineChanged(int newLineIndex)
    {
        lastLineIndex = newLineIndex;

        foreach (var ghost in ghostEvents)
        {
            // Appear when reaching the right line
            if (ghost.lineIndexToAppear == newLineIndex && !ghost.hasAppeared)
            {
                ghost.hasAppeared = true;
                StartCoroutine(FadeGhost(ghost.ghostObject, 0f, 1f, ghost.fadeInTime));
            }

            // If we passed the ghost's moment, fade it out
            if (ghost.lineIndexToAppear < newLineIndex)
            {
                StartCoroutine(FadeGhost(ghost.ghostObject, 1f, 0f, ghost.fadeOutTime));
            }
        }
    }

    private void HandleDialogueEnded()
    {
        // Fade out all ghosts when dialogue ends
        foreach (var ghost in ghostEvents)
        {
            if (ghost.ghostObject != null)
                StartCoroutine(FadeGhost(ghost.ghostObject, 1f, 0f, ghost.fadeOutTime));
        }
    }

    // ---------------------- Fading Helpers ----------------------
    private IEnumerator FadeGhost(GameObject ghost, float from, float to, float duration)
    {
        if (ghost == null) yield break;

        Renderer[] rends = ghost.GetComponentsInChildren<Renderer>();
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);

            foreach (var r in rends)
            {
                foreach (var mat in r.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                }
            }

            yield return null;
        }
    }

    private void SetGhostAlpha(GameObject ghost, float alpha)
    {
        Renderer[] rends = ghost.GetComponentsInChildren<Renderer>();

        foreach (var r in rends)
        {
            foreach (var mat in r.materials)
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
}
