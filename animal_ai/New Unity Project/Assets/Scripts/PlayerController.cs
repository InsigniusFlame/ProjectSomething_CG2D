using UnityEngine;

/// <summary>
/// Simple first-person player controller with WASD movement and mouse look.
/// For testing the animal AI behavior.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Player movement speed")]
    public float moveSpeed = 6f;
    
    [Tooltip("Gravity applied to the player")]
    public float gravity = -20f;
    
    [Tooltip("Sprint speed multiplier")]
    public float sprintMultiplier = 1.5f;

    [Header("Look Settings")]
    [Tooltip("Mouse sensitivity for looking around")]
    public float mouseSensitivity = 2f;
    
    [Tooltip("Maximum vertical look angle")]
    public float maxLookAngle = 80f;

    [Header("References")]
    [Tooltip("Camera transform for vertical rotation (auto-assigned if child exists)")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool cursorLocked = true;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
            else
            {
                Debug.LogWarning("PlayerController: No camera found as child. Mouse look will only rotate horizontally.");
            }
        }

        // Lock cursor for FPS controls
        LockCursor();
    }

    private void Update()
    {
        HandleCursorLock();
        HandleMovement();
        HandleMouseLook();
    }

    /// <summary>
    /// Handles WASD movement input.
    /// </summary>
    private void HandleMovement()
    {
        // Ground check
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }

        // Get input
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        // Calculate move direction relative to player facing
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        
        // Apply sprint
        float currentSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        // Move the character
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Handles mouse look input for camera rotation.
    /// </summary>
    private void HandleMouseLook()
    {
        if (!cursorLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation (rotate player body)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (rotate camera only)
        if (cameraTransform != null)
        {
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    /// <summary>
    /// Handles cursor lock/unlock with Escape key.
    /// </summary>
    private void HandleCursorLock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        // Re-lock on click when unlocked
        if (!cursorLocked && Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }
}
