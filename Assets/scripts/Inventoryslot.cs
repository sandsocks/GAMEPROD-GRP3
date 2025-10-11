using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;
    private string itemName;
    private string itemDescription;
    private Sprite itemIcon;

    public bool HasItem { get; private set; } = false;
    public string ItemName => itemName;

    public void SetItem(Sprite icon, string name, string description)
    {
        itemIcon = icon;
        itemName = name;
        itemDescription = description;
        iconImage.sprite = icon;
        iconImage.enabled = true;
        HasItem = true;
    }

    public void OnClick()
    {
        if (HasItem)
        {
            Debug.Log($"Slot clicked: {itemName}");
            InventoryManager.Instance.ShowItemDescription(itemName, itemDescription, itemIcon);
        }
        else
        {
            Debug.Log("Clicked empty slot");
        }
    }
}
