using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    public GameObject questPanel;
    public GameObject inventoryPanel;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            
            questPanel.SetActive(true);
            inventoryPanel.SetActive(true);

            
            gameObject.SetActive(false);
        }
    }
}
