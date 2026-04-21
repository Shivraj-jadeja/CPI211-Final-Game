using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.8f;
    public float lookSmoothDamp = 0.05f;
    public Transform cameraTransform;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Potion Settings")]
    public float potionSprintDuration = 15f;
    public float potionSprintSpeed = 12f;
    private bool isPotionActive = false;
    private float currentSpeed;
    private float originalWalkSpeed;

    private float xRotation = 0f;
    private float currentXRotation = 0f;
    private float xRotationVelocity = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentSpeed = walkSpeed;
        originalWalkSpeed = walkSpeed;
        transform.rotation = Quaternion.identity;

        Debug.Log("FPS Controller Started - Movement is camera-relative");
        Debug.Log("Use Potion (Hold 3 for 4 seconds) to gain sprint speed for 15 seconds!");
    }

    void Update()
    {
        // ----- INPUT -----
        float horizontal = Input.GetAxisRaw("Horizontal");  // A/D
        float vertical = Input.GetAxisRaw("Vertical");      // W/S

        // ----- GROUND CHECK -----
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }

        // ----- MOVEMENT (Camera Relative) -----
        if (horizontal != 0 || vertical != 0)
        {
            // Get camera's forward and right directions
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Remove vertical component to keep movement on ground plane
            cameraForward.y = 0;
            cameraRight.y = 0;

            // Normalize to maintain direction
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate movement direction based on camera orientation
            Vector3 move = (cameraForward * vertical) + (cameraRight * horizontal);

            // Apply movement with current speed (normal or potion-boosted)
            controller.Move(move * currentSpeed * Time.deltaTime);
        }

        // ----- JUMP -----
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("Jumped!");
        }

        // ----- GRAVITY -----
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LateUpdate()
    {
        // ----- SMOOTH MOUSE LOOK -----
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        currentXRotation = Mathf.SmoothDamp(currentXRotation, xRotation, ref xRotationVelocity, lookSmoothDamp);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * mouseX);
    }

    // Public method to activate potion buff (called from InventorySystem)
    public void ActivatePotionBuff()
    {
        if (!isPotionActive)
        {
            StartCoroutine(PotionBuffCoroutine());
        }
        else
        {
            Debug.Log("Potion effect already active!");
        }
    }

    private IEnumerator PotionBuffCoroutine()
    {
        isPotionActive = true;
        currentSpeed = potionSprintSpeed;

        Debug.Log($"🧪 Potion activated! Speed increased to {potionSprintSpeed} for {potionSprintDuration} seconds!");

        // Visual feedback - you can add effects here
        StartCoroutine(ScreenFlashEffect());

        float remainingTime = potionSprintDuration;
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        currentSpeed = walkSpeed;
        isPotionActive = false;
        Debug.Log("🏃 Potion effect wore off. Speed returned to normal.");
    }

    private IEnumerator ScreenFlashEffect()
    {
        // Optional: Add a visual effect when potion is used
        float flashDuration = 0.2f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            // Here you would update a UI image's alpha or color
            // For example: flashImage.color = new Color(1,1,1, 1 - (elapsed / flashDuration));
            yield return null;
        }

        Debug.Log("✨ Potion visual effect completed!");
    }

    // Optional: Method to check if potion is active (for UI)
    public bool IsPotionActive()
    {
        return isPotionActive;
    }

    // Optional: Get remaining potion time (for UI display)
    public float GetRemainingPotionTime()
    {
        if (isPotionActive)
        {
            // This would need to track the actual remaining time
            // For a complete implementation, store the start time
            return potionSprintDuration - (Time.time - potionStartTime);
        }
        return 0f;
    }

    // Add this variable for tracking potion start time
    private float potionStartTime;

    // Updated PotionBuffCoroutine with time tracking
    private IEnumerator PotionBuffCoroutineWithTimer()
    {
        isPotionActive = true;
        potionStartTime = Time.time;
        currentSpeed = potionSprintSpeed;

        Debug.Log($"🧪 Potion activated! Speed increased to {potionSprintSpeed} for {potionSprintDuration} seconds!");

        StartCoroutine(ScreenFlashEffect());

        yield return new WaitForSeconds(potionSprintDuration);

        currentSpeed = walkSpeed;
        isPotionActive = false;
        Debug.Log("🏃 Potion effect wore off. Speed returned to normal.");
    }
}