using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180f, 0); // flips text to face camera correctly
    }
}
