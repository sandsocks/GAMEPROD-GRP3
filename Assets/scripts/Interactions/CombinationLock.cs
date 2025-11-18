using UnityEngine;

public class CombinationLock : MonoBehaviour
{
    [Header("Lock Wheels")]
    public Transform[] wheels;
    [Range(0, 7)] public int[] currentValues = { 7, 7, 7 };
    public int[] correctCombination = { 1, 2, 3 };
    public float rotationPerStep = 45f;

    [Header("Interaction Settings")]
    public Camera interactionCamera;
    public Transform cameraFocusPoint;
    public Transform cameraOriginalPoint;
    public float cameraMoveSpeed = 5f;
    public float cameraRotateSpeed = 5f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Reward Settings")]
    public bool givesItem = false;
    public Sprite rewardIcon;
    public string rewardItemName;
    [TextArea] public string rewardDescription;

    [Header("Reward Animation")]
    public bool playsUnlockAnimation = false;
    public Animation unlockAnimationComponent;
    public string unlockAnimationClipName;

    [Header("Door Animator")]
    public Animator doorAnimator;                     // <-- NEW
    public string doorBoolParameter = "OpenDoor";     // <-- NEW bool parameter

    [Header("Quest Integration")]
    public string questToStart;
    public string questToComplete;
    public string questObjectiveQuestId;
    public string questObjectiveId;

    [Header("Lock State")]
    public bool openDoor = false; // internal bool

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

            initialRotations[i] =
                wheels[i].localRotation *
                Quaternion.Inverse(
                    Quaternion.Euler(currentValues[i] * rotationPerStep, 0f, 0f)
                );
        }

        if (interactionCamera != null && cameraOriginalPoint != null)
        {
            interactionCamera.transform.position = cameraOriginalPoint.position;
            interactionCamera.transform.rotation = cameraOriginalPoint.rotation;
        }
    }

    void Update()
    {
        if (interactionCamera == null || player == null) return;

        if (playerInTrigger && Input.GetKeyDown(interactKey) && !isUnlocked)
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
            Time.deltaTime * cameraMoveSpeed);

        interactionCamera.transform.rotation = Quaternion.Slerp(
            interactionCamera.transform.rotation,
            cameraFocusPoint.rotation,
            Time.deltaTime * cameraRotateSpeed);
    }

    void ReturnCameraToOriginal()
    {
        if (cameraOriginalPoint == null) return;

        interactionCamera.transform.position = Vector3.Lerp(
            interactionCamera.transform.position,
            cameraOriginalPoint.position,
            Time.deltaTime * cameraMoveSpeed);

        interactionCamera.transform.rotation = Quaternion.Slerp(
            interactionCamera.transform.rotation,
            cameraOriginalPoint.rotation,
            Time.deltaTime * cameraRotateSpeed);
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
                        return;
                    }
                }
            }
        }
    }

    void IncrementWheel(int index)
    {
        if (isUnlocked) return;

        currentValues[index] = (currentValues[index] + 1) % 8;
        ApplyWheelRotation(index);
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

        Unlock();
    }

    void Unlock()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        openDoor = true; // internal state

        StopInteraction();
        hasInteractedOnce = true;

        // QUEST START / COMPLETE
        if (!string.IsNullOrEmpty(questToStart))
            QuestManager.Instance.StartQuest(questToStart);

        if (!string.IsNullOrEmpty(questToComplete))
            QuestManager.Instance.CompleteQuest(questToComplete);

        if (!string.IsNullOrEmpty(questObjectiveQuestId) &&
            !string.IsNullOrEmpty(questObjectiveId))
        {
            QuestManager.Instance.AddObjectiveProgress(
                questObjectiveQuestId,
                questObjectiveId,
                1);
        }

        // ITEM REWARD
        if (givesItem && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(rewardIcon, rewardItemName, rewardDescription);
        }

        // PLAY CLIP ANIMATION
        if (playsUnlockAnimation && unlockAnimationComponent != null &&
            !string.IsNullOrEmpty(unlockAnimationClipName))
        {
            unlockAnimationComponent.Play(unlockAnimationClipName);
        }

        // SET ANIMATOR BOOL
        if (doorAnimator != null && !string.IsNullOrEmpty(doorBoolParameter))
        {
            doorAnimator.SetBool(doorBoolParameter, true);
            Debug.Log("Door Animator bool set TRUE.");
        }
    }

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
