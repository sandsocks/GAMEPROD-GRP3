using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FirstPersonController : MonoBehaviour
{
    #region Camera Variables
    public Camera playerCamera;
    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;
    private Image crosshairObject;

    // Internal
    private float yaw = 0f;
    private float pitch = 0f;

    #region Zoom
    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;
    private bool isZoomed = false;
    #endregion
    #endregion

    #region Movement Variables
    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float sprintSpeed = 7f;

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    private CanvasGroup sprintBarCG;
    private float sprintRemaining;
    private float sprintCooldownReset;
    private bool isSprintCooldown = false;
    private bool isSprinting = false;

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = 0.75f;
    public float speedReduction = 0.5f;
    private Vector3 originalScale;
    private bool isCrouched = false;

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(0.15f, 0.05f, 0f);

    // Internal
    private Vector3 jointOriginalPos;
    private float timer = 0f;
    private bool isWalking = false;

    // CharacterController
    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = 9.81f;
    #endregion

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        crosshairObject = GetComponentInChildren<Image>();

        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        jointOriginalPos = joint.localPosition;

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    private void Start()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        // Sprint Bar
        sprintBarCG = GetComponentInChildren<CanvasGroup>();
        if (useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(screenWidth * 0.3f, screenHeight * 0.015f, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(screenWidth * 0.3f - 2, screenHeight * 0.015f - 2, 0f);

            if (hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HandleCamera();
        HandleZoom();
        HandleSprint();
        HandleJump();
        HandleCrouch();
        HandleHeadBob();
    }

    private void HandleCamera()
    {
        if (!cameraCanMove) return;

        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

        if (!invertCamera)
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        else
            pitch += Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        transform.localEulerAngles = new Vector3(0, yaw, 0);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }

    private void HandleZoom()
    {
        if (!enableZoom) return;

        if (!holdToZoom && Input.GetKeyDown(zoomKey) && !isSprinting)
            isZoomed = !isZoomed;

        if (holdToZoom && !isSprinting)
        {
            if (Input.GetKeyDown(zoomKey)) isZoomed = true;
            if (Input.GetKeyUp(zoomKey)) isZoomed = false;
        }

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, isZoomed ? zoomFOV : fov, zoomStepTime * Time.deltaTime);
    }

    private void HandleSprint()
    {
        if (!enableSprint) return;

        if (isSprinting)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);
        }

        if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown)
        {
            isSprinting = true;
            if (!unlimitedSprint)
            {
                sprintRemaining -= Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    isSprinting = false;
                    isSprintCooldown = true;
                }
            }
        }
        else
        {
            isSprinting = false;
            if (!unlimitedSprint)
                sprintRemaining = Mathf.Clamp(sprintRemaining + Time.deltaTime, 0, sprintDuration);
        }

        // Sprint bar scaling
        if (useSprintBar && !unlimitedSprint)
        {
            float sprintPercent = sprintRemaining / sprintDuration;
            sprintBar.transform.localScale = new Vector3(sprintPercent, 1f, 1f);
        }

        // Cooldown reset
        if (isSprintCooldown)
        {
            sprintCooldown -= Time.deltaTime;
            if (sprintCooldown <= 0) isSprintCooldown = false;
        }
        else
        {
            sprintCooldown = sprintCooldownReset;
        }
    }

    private void HandleJump()
    {
        // Jump handled in FixedUpdate
    }

    private void HandleCrouch()
    {
        if (!enableCrouch) return;

        if (Input.GetKeyDown(crouchKey) && !holdToCrouch)
            ToggleCrouch();

        if (holdToCrouch)
        {
            if (Input.GetKeyDown(crouchKey)) isCrouched = false;
            if (Input.GetKeyUp(crouchKey)) isCrouched = true;
            ToggleCrouch();
        }
    }

    private void ToggleCrouch()
    {
        if (isCrouched)
        {
            transform.localScale = originalScale;
            walkSpeed /= speedReduction;
            isCrouched = false;
        }
        else
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed *= speedReduction;
            isCrouched = true;
        }
    }

    private void HandleHeadBob()
    {
        if (!enableHeadBob) return;

        if (isWalking)
        {
            float bobFactor = bobSpeed;
            if (isSprinting) bobFactor += sprintSpeed;
            if (isCrouched) bobFactor *= speedReduction;

            timer += Time.deltaTime * bobFactor;
            joint.localPosition = new Vector3(
                jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x,
                jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y,
                jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z
            );
        }
        else
        {
            timer = 0f;
            joint.localPosition = Vector3.Lerp(joint.localPosition, jointOriginalPos, Time.deltaTime * bobSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (!playerCanMove) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0, z);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);
        inputDir = transform.TransformDirection(inputDir);

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = inputDir * currentSpeed;

        // Gravity
        if (controller.isGrounded)
        {
            velocity.y = 0f;
            if (enableJump && Input.GetKeyDown(jumpKey))
                velocity.y = jumpPower;
        }
        else
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        Vector3 finalMove = move + velocity;
        controller.Move(finalMove * Time.fixedDeltaTime);

        // Walking state for head bob
        isWalking = inputDir.magnitude > 0 && controller.isGrounded;
    }
}
