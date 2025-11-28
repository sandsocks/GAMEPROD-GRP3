using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class UIEffects :
    MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Texts to affect")]
    public List<TMP_Text> texts = new List<TMP_Text>();   // assign multiple

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.red;

    [Header("Hover Scaling (%)")]
    [Range(0.05f, 0.20f)]
    public float hoverScalePercent = 0.10f; // 10% increase

    // internal storage
    private List<float> originalSizes = new List<float>();

    void Start()
    {
        // store original font sizes
        originalSizes.Clear();
        foreach (var t in texts)
        {
            originalSizes.Add(t.fontSize);
            t.color = normalColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].color = hoverColor;
            texts[i].fontSize = originalSizes[i] * (1f + hoverScalePercent);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetToNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        foreach (var t in texts)
            t.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // return to hover state if the pointer is still inside the button
        foreach (var t in texts)
            t.color = hoverColor;
    }

    private void ResetToNormal()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].fontSize = originalSizes[i];
            texts[i].color = normalColor;
        }
    }
}
