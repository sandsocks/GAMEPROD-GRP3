using System.Collections;
using UnityEngine;
using TMPro;

public class WorldTextRevealer : MonoBehaviour
{
    public TextMeshProUGUI textTMP;
    public float letterFadeDuration = 0.07f;
    public float delayBetweenLetters = 0.02f;
    public float visibleDuration = 5f;
    public float fadeOutDuration = 5f;

    CanvasGroup cg;

    void Awake()
    {
        cg = GetComponentInParent<CanvasGroup>();
        if (cg != null)
            cg.alpha = 0;
    }

    public void StartReveal(string sentence)
    {
        StopAllCoroutines();
        StartCoroutine(RevealSequence(sentence));
    }

    IEnumerator RevealSequence(string sentence)
    {
        if (cg != null) cg.alpha = 1f;

        yield return StartCoroutine(RevealLetters(sentence));
        yield return new WaitForSeconds(visibleDuration);
        yield return StartCoroutine(FadeOut());
    }

    IEnumerator RevealLetters(string sentence)
    {
        textTMP.text = sentence;
        textTMP.ForceMeshUpdate();

        TMP_TextInfo textInfo = textTMP.textInfo;

 
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var c = textInfo.characterInfo[i];
            if (!c.isVisible) continue;

            int matIndex = c.materialReferenceIndex;
            int vertIndex = c.vertexIndex;

            Color32[] colors = textInfo.meshInfo[matIndex].colors32;

            for (int v = 0; v < 4; v++)
                colors[vertIndex + v].a = 0;
        }

        textTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);


        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var c = textInfo.characterInfo[i];
            if (!c.isVisible) continue;

            StartCoroutine(FadeLetterIn(i));
            yield return new WaitForSeconds(delayBetweenLetters);
        }
    }

    IEnumerator FadeLetterIn(int index)
    {
        float t = 0f;

        TMP_TextInfo textInfo = textTMP.textInfo;
        var c = textInfo.characterInfo[index];
        int matIndex = c.materialReferenceIndex;
        int vertIndex = c.vertexIndex;

        Color32[] colors = textInfo.meshInfo[matIndex].colors32;

        while (t < letterFadeDuration)
        {
            t += Time.deltaTime;
            byte a = (byte)(Mathf.Lerp(0, 255, t / letterFadeDuration));

            for (int v = 0; v < 4; v++)
                colors[vertIndex + v].a = a;

            textTMP.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0f;

        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            if (cg != null)
                cg.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);

            yield return null;
        }

        cg.alpha = 0f;
    }
}
