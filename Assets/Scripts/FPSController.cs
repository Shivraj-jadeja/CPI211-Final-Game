using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public bool canMove = true;

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
    }

    void Update()
    {
        if (!canMove)
        {
            velocity = Vector3.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;
        }

        if (horizontal != 0 || vertical != 0)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;

            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 move = (cameraForward * vertical) + (cameraRight * horizontal);

            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && vertical > 0;
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            controller.Move(move * currentSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        currentXRotation = Mathf.SmoothDamp(currentXRotation, xRotation, ref xRotationVelocity, lookSmoothDamp);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(currentXRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }
}