using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Icon References")]
    public Image iconImage;       // for Sprite-based icons
    public RawImage rawIconImage; // for Texture-based icons

    [Header("Panel Reference")]
    public GameObject panel; // assign the panel GameObject

    private void Start()
    {
        if (panel != null)
            panel.SetActive(false); // hidden by default
    }

    /// <summary>
    /// Show the description panel using a Sprite icon.
    /// </summary>
    public void Show(string itemName, string description, Sprite icon)
    {
        titleText.text = itemName;
        descriptionText.text = description;

        // Use Image if available
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        // Hide RawImage if Sprite is used
        if (rawIconImage != null)
            rawIconImage.enabled = false;

        panel.SetActive(true);
    }

    /// <summary>
    /// Show the description panel using a Texture icon (for RawImage).
    /// </summary>
    public void Show(string itemName, string description, Texture texture)
    {
        titleText.text = itemName;
        descriptionText.text = description;

        // Use RawImage if available
        if (rawIconImage != null)
        {
            rawIconImage.texture = texture;
            rawIconImage.enabled = texture != null;
        }

        // Hide Image if Texture is used
        if (iconImage != null)
            iconImage.enabled = false;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
