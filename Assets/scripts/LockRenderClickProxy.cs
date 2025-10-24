using UnityEngine;

public class LockRenderClickProxy : MonoBehaviour
{
    [Header("Render Texture Setup")]
    public CombinationLock combinationLock;   // Main CombinationLock script
    public Camera interactionCamera;          // Camera rendering the lock
    public RenderTexture renderTexture;       // The camera's RenderTexture output

    [Header("Optional Debug")]
    public bool showDebugRay = false;

    private void Update()
    {
        // Only handle clicks when player is actively interacting with the lock
        if (combinationLock == null || !combinationLock.GetIsInteracting())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            // Step 1: Raycast from MAIN camera into the world (to the quad)
            Ray mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mainRay, out RaycastHit hit))
            {
                // Make sure the hit object is THIS quad
                if (hit.collider.gameObject == gameObject)
                {
                    // Step 2: Convert UV (0–1) coordinates to RenderTexture pixels
                    Vector2 uv = hit.textureCoord;
                    Vector2 pixel = new Vector2(
                        uv.x * renderTexture.width,
                        uv.y * renderTexture.height
                    );

                    // Step 3: Create a ray from the interaction camera
                    Ray interactionRay = interactionCamera.ScreenPointToRay(pixel);

                    // Step 4: Raycast into the actual lock scene
                    if (Physics.Raycast(interactionRay, out RaycastHit lockHit, 10f))
                    {
                        // Forward hit to CombinationLock
                        combinationLock.HandleExternalWheelHit(lockHit);

                        if (showDebugRay)
                        {
                            Debug.DrawRay(interactionRay.origin, interactionRay.direction * 3f, Color.green, 2f);
                            Debug.Log("Hit lock object via RenderTexture: " + lockHit.transform.name);
                        }
                    }
                }
            }
        }
    }
}
