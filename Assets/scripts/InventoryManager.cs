using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public InventorySlot[] slots = new InventorySlot[8];
    public ItemDescriptionPanel descriptionPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ensures only one manager exists
        }
    }
    public void AddItem(Sprite icon, string itemName, string description)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].HasItem)
            {
                slots[i].SetItem(icon, itemName, description);
                break;
            }
        }
    }

    public void ShowItemDescription(string itemName, string description, Sprite icon)
    {
        descriptionPanel.Show(itemName, description, icon);
    }

    public bool HasItem(string itemName)
    {
        foreach (var slot in slots)
        {
            if (slot.HasItem && slot.ItemName == itemName)
                return true;
        }
        return false;
    }
}
