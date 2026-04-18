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

    private float xRotation = 0f;
    private float currentXRotation = 0f;
    private float xRotationVelocity = 0f;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        currentSpeed = walkSpeed;
        transform.rotation = Quaternion.identity;

        Debug.Log("FPS Controller Started - Movement is now camera-relative");
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

            // Sprint check
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && vertical > 0;
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // Apply movement
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

    // Add this to your FPSController class
    public void ActivatePotionBuff()
    {
        Debug.Log("🧪 Potion buff activated! Sprint speed increased!");
        // Your potion logic here
        // For example:
        StartCoroutine(PotionBuffCoroutine());
    }

    private IEnumerator PotionBuffCoroutine()
    {
        float originalSprintSpeed = sprintSpeed;
        sprintSpeed = 12f; // Boosted speed

        yield return new WaitForSeconds(15f);

        sprintSpeed = originalSprintSpeed;
        Debug.Log("Potion buff wore off");
    }
}