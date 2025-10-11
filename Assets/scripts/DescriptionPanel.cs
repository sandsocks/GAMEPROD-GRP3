using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemDescriptionPanel : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image iconImage;

    public GameObject panel; // assign the panel GameObject

    private void Start()
    {
        panel.SetActive(false); // hidden by default
    }

    public void Show(string itemName, string description, Sprite icon)
    {
        titleText.text = itemName;
        descriptionText.text = description;
        iconImage.sprite = icon;
        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
