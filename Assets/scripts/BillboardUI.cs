using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [Tooltip("Drag the camera you want this canvas to face. If left empty, Camera.main will be used.")]
    public Camera targetCamera;

    [Tooltip("If true, object will only rotate around Y so it stays upright (good for nameplates).")]
    public bool lockY = true;

    [Tooltip("Smooth the rotation.")]
    public bool smooth = true;

    [Tooltip("Smoothing speed (higher = faster).")]
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        // Try to use assigned camera; fall back to Camera.main if nothing assigned.
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return; // still null: nothing to do
        }

        // Calculate direction from camera -> this object
        Vector3 camPos = targetCamera.transform.position;
        Vector3 lookDir;

        if (lockY)
        {
            // Keep the object upright by ignoring difference in Y
            camPos.y = transform.position.y;
            lookDir = transform.position - camPos;
        }
        else
        {
            lookDir = transform.position - camPos;
        }

        if (lookDir.sqrMagnitude <= 0.0001f) return; // avoid zero-length

        Quaternion targetRot = Quaternion.LookRotation(lookDir);

        if (smooth)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
        else
            transform.rotation = targetRot;
    }
}
