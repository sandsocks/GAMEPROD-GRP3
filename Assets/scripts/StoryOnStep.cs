using UnityEngine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StoryOnStep : MonoBehaviour
{
    public StorySequence storySequence;
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (storySequence != null)
        {
            storySequence.BeginSequence();
            hasTriggered = true;
        }
        else
        {
            Debug.LogWarning("StoryOnStep has no StorySequence assigned.");
        }
    }
}
