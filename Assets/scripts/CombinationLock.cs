using UnityEngine;

public class CombinationLock : MonoBehaviour
{
    [Header("Lock Wheels")]
    public Transform[] wheels;
    [Range(0, 7)] public int[] currentValues = { 7, 7, 7 }; // Default logical = 7
    public int[] correctCombination = { 1, 2, 3 };
    public float rotationPerStep = 45f; // 360 / 8 = 45° per step

    [Header("Interaction Settings")]
    public Camera interactionCamera;
    public Transform cameraFocusPoint;
    public Transform cameraOriginalPoint;
    public float cameraMoveSpeed = 5f;
    public float cameraRotateSpeed = 5f;
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Reward Settings")]
    public bool giveRewardItem = true;
    public Sprite rewardItemIcon;
    public string rewardItemName = "Vault Key";
    [TextArea(2, 4)] public string rewardItemDescription = "A heavy metal key used to open the vault door.";

    [Header("Optional Animation")]
    public Animator unlockAnimator;
    public string unlockTriggerName = "Unlock";

    private bool isInteracting = false;
    private bool playerInTrigger = false;
    private bool hasInteractedOnce = false;
    private bool isUnlocked = false;

    private Transform player;
    private MonoBehaviour playerController;

    private Quaternion[] initialRotations;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerController = player?.GetComponent<MonoBehaviour>();

        initialRotations = new Quaternion[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == null) continue;

           
            currentValues[i] = Mathf.Clamp(currentValues[i], 0, 7);
            initialRotations[i] = wheels[i].localRotation *
                                  Quaternion.Inverse(Quaternion.Euler(currentValues[i] * rotationPerStep, 0f, 0f));
        }

       
        if (interactionCamera != null && cameraOriginalPoint != null)
        {
            interactionCamera.transform.position = cameraOriginalPoint.position;
            interactionCamera.transform.rotation = cameraOriginalPoint.rotation;
        }

        Debug.Log("Combination Lock initialized.");
    }

    void Update()
    {
        if (interactionCamera == null || player == null) return;

   
        if (playerInTrigger && Input.GetKeyDown(interactKey))
        {
            if (!isInteracting)
                StartInteraction();
            else
                StopInteraction();

            hasInteractedOnce = true;
        }

        if (isInteracting)
        {
            MoveCameraToFocus();
            HandleWheelClick();
        }
        else if (hasInteractedOnce)
        {
            ReturnCameraToOriginal();
        }
    }

    void StartInteraction()
    {
        isInteracting = true;

        if (playerController != null)
            playerController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("🟢 Player is now interacting with the lock.");
    }

    void StopInteraction()
    {
        isInteracting = false;

        if (playerController != null)
            playerController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("🔴 Player stopped interacting with the lock.");
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

    void ReturnCameraToOriginal()
    {
        if (cameraOriginalPoint == null) return;

        interactionCamera.transform.position = Vector3.Lerp(
            interactionCamera.transform.position,
            cameraOriginalPoint.position,
            Time.deltaTime * cameraMoveSpeed
        );

        interactionCamera.transform.rotation = Quaternion.Slerp(
            interactionCamera.transform.rotation,
            cameraOriginalPoint.rotation,
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
        if (isUnlocked) return;

        for (int i = 0; i < wheels.Length; i++)
        {
            if (currentValues[i] != correctCombination[i])
                return;
        }

        Debug.Log("✅ Correct Combination! Lock opened!");
        isUnlocked = true;

       
        if (giveRewardItem && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(rewardItemIcon, rewardItemName, rewardItemDescription);
            Debug.Log($"🎁 Player received: {rewardItemName}");
        }

    
        if (unlockAnimator != null && !string.IsNullOrEmpty(unlockTriggerName))
        {
            unlockAnimator.SetTrigger(unlockTriggerName);
            Debug.Log("🔓 Unlock animation triggered!");
        }

       
        StopInteraction();
    }

    // === Trigger Detection ===
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
    }

    // === External Proxy Support ===
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
