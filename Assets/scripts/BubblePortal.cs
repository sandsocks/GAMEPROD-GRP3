using UnityEngine;
using UnityEngine.SceneManagement;

public class BubblePortal : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip portalSFX;

    [Header("Tint")]
    public Color tintColor = Color.blue;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        
        if (audioSource && portalSFX)
            audioSource.PlayOneShot(portalSFX);


        Invoke(nameof(LoadScene), 0.2f);
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(3);
    }
}
