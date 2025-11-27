using UnityEngine;

public class TintManager : MonoBehaviour
{
    public static TintManager Instance;

    public Color savedTint = Color.white;
    public bool tintApplied = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // KEEP THIS OBJECT across scenes
    }
}
