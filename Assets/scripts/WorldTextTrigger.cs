using UnityEngine;

public class WorldTextTrigger : MonoBehaviour
{
    public WorldTextRevealer revealer;
    public string textToShow = "The story begins here...";
    bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;
            revealer.StartReveal(textToShow);
        }
    }
}
