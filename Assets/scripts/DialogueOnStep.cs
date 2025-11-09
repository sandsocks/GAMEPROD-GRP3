using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DialogueOnStep : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("The dialogue data to play when the player steps on this object.")]
    public DialogueData dialogueData;

    [Header("Trigger Options")]
    [Tooltip("If true, this trigger can only be activated once.")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Reset()
    {
        // Ensure the collider is set as a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            if (dialogueData != null)
            {
                // Call the DialogueManager directly
                DialogueManager.Instance.StartDialogue(dialogueData);
                hasTriggered = true;
            }
            else
            {
                Debug.LogWarning($"{name} has no DialogueData assigned!", this);
            }
        }
    }
}
