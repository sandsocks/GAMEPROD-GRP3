using UnityEngine;

public class ApplyPersistentTint : MonoBehaviour
{
    public Renderer rendererToTint;

    void Start()
    {
        if (TintManager.Instance.tintApplied)
        {
            rendererToTint.material.color = TintManager.Instance.savedTint;
        }
    }
}
