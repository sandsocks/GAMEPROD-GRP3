using UnityEngine;

public class CombinationLock : MonoBehaviour
{
    [Header("Lock Wheels")]
    public Transform[] wheels;
    [Range(0, 7)] public int[] currentValues = { 7, 7, 7 };  // Default logical = 7
    public int[] correctCombination = { 1, 2, 3 };
    public float rotationPerStep = 45f; // 360 / 8 = 45° per step

    [Header("Interaction Settings")]
    public Camera interactionCamera;
    public Transform cameraFocusPoint;
    public float cameraMoveSpeed = 5f;
    public float cameraRotateSpeed = 5f;
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    private bool isInteracting = false;
    private Transform player;
    private MonoBehaviour playerController;

    private Quaternion[] initialRotations; // stores wheel baselines

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<MonoBehaviour>();

        initialRotations = new Quaternion[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null) continue;

            // Clamp value and save current rotation as baseline for that value
            currentValues[i] = Mathf.Clamp(currentValues[i], 0, 7);
            initialRotations[i] = wheels[i].localRotation *
                                  Quaternion.Inverse(Quaternion.Euler(currentValues[i] * rotationPerStep, 0f, 0f));
        }

        Debug.Log("Lock initialized — baseline synced to visual model.");
    }

    void Update()
    {
        if (interactionCamera == null || player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        // Press E to start/stop interaction within range
        if (distance < interactDistance && Input.GetKeyDown(interactKey))
        {
            if (!isInteracting) StartInteraction();
            else StopInteraction();
        }

        if (isInteracting)
        {
            MoveCameraToFocus();
            HandleWheelClick();
        }
    }

    void StartInteraction()
    {
        isInteracting = true;

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void StopInteraction()
    {
        isInteracting = false;

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void MoveCameraToFocus()
    {
        if (cameraFocusPoint == null) return;

        interactionCamera.transform.position = Vector3.Lerp(
            interactionCamera.transform.position,
            cameraFocusPoint.position,
            Time.deltaTime * cameraMoveSpeed
        );

        interactionCamera.transform.rotation = Quaternion.Slerp(
            interactionCamera.transform.rotation,
            cameraFocusPoint.rotation,
            Time.deltaTime * cameraRotateSpeed
        );
    }

    void HandleWheelClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = interactionCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 5f))
            {
                for (int i = 0; i < wheels.Length; i++)
                {
                    if (hit.transform == wheels[i])
                    {
                        IncrementWheel(i);
                        break;
                    }
                }
            }
        }
    }

    void IncrementWheel(int index)
    {
        currentValues[index] = (currentValues[index] + 1) % 8;
        ApplyWheelRotation(index);
        Debug.Log($"Wheel {index + 1} → {currentValues[index]}");
        CheckCombination();
    }

    void ApplyWheelRotation(int i)
    {
        if (wheels[i] == null) return;

        float angle = currentValues[i] * rotationPerStep;
        wheels[i].localRotation = initialRotations[i] * Quaternion.Euler(angle, 0f, 0f);
    }

    void CheckCombination()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (currentValues[i] != correctCombination[i])
                return;
        }

        Debug.Log("✅ Correct Combination! Lock opened!");
    }

    // External proxy bridge (for LockRenderClickProxy)
    public bool GetIsInteracting() => isInteracting;

    public void HandleExternalWheelHit(RaycastHit hit)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (hit.transform == wheels[i])
            {
                IncrementWheel(i);
                break;
            }
        }
    }
}
