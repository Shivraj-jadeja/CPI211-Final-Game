using UnityEngine;

public class HideZone : MonoBehaviour
{
    public Transform hidePoint;
    public Transform playerCamera;

    [Header("Player Control")]
    public FPSController fpsController;

    [Header("Hide View")]
    public float hiddenCameraY = 0.7f;

    [Header("Ghost Pass Settings")]
    public float ghostPassDistance = 10f;

    private Transform player;
    private CharacterController controller;
    private bool playerInside = false;
    private bool isHiding = false;

    private Vector3 originalCameraLocalPos;
    private Vector3 enterPosition;
    private Quaternion enterRotation;
    private Vector3 lockedGhostPassTarget;

    public Vector3 GhostPassTarget => lockedGhostPassTarget;

    void Update()
    {
        if (!playerInside || player == null) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isHiding)
                EnterHide();
            else
                ExitHide();
        }

        if (isHiding && hidePoint != null)
        {
            player.position = hidePoint.position;
        }
    }

    private void EnterHide()
    {
        isHiding = true;

        enterPosition = player.position;
        enterRotation = player.rotation;

        // Auto-find needed player references if they were not assigned in Inspector.
        if (controller == null)
            controller = player.GetComponent<CharacterController>();

        if (fpsController == null)
            fpsController = player.GetComponent<FPSController>();

        if (playerCamera == null)
        {
            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
                playerCamera = cam.transform;
        }

        if (fpsController != null)
            fpsController.canMove = false;

        if (controller != null)
            controller.enabled = false;

        if (hidePoint != null)
        {
            player.position = hidePoint.position;

            GhostChase ghost = FindObjectOfType<GhostChase>();

            if (ghost != null && ghost.gameObject.activeInHierarchy)
            {
                Vector3 lookDirection = ghost.transform.position - player.position;
                lookDirection.y = 0f;

                if (lookDirection != Vector3.zero)
                    player.rotation = Quaternion.LookRotation(lookDirection);

                Vector3 passDirection = transform.position - ghost.transform.position;
                passDirection.y = 0f;

                if (passDirection == Vector3.zero)
                    passDirection = transform.forward;

                passDirection.Normalize();
                lockedGhostPassTarget = transform.position + passDirection * ghostPassDistance;
            }
            else
            {
                player.rotation = hidePoint.rotation;
                lockedGhostPassTarget = transform.position + transform.forward * ghostPassDistance;
            }
        }

        if (playerCamera != null)
        {
            originalCameraLocalPos = playerCamera.localPosition;
            playerCamera.localPosition = new Vector3(
                originalCameraLocalPos.x,
                hiddenCameraY,
                originalCameraLocalPos.z
            );
        }

        GhostManager.Instance.SetPlayerHidden(true, this);
        Debug.Log("Player hiding in bush");
    }

    private void ExitHide()
    {
        isHiding = false;

        player.position = enterPosition;
        player.rotation = enterRotation;

        if (controller != null)
            controller.enabled = true;

        if (fpsController != null)
            fpsController.canMove = true;

        if (playerCamera != null)
            playerCamera.localPosition = originalCameraLocalPos;

        GhostManager.Instance.SetPlayerHidden(false, null);
        Debug.Log("Player exited bush");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            player = other.transform;

            if (fpsController == null)
                fpsController = other.GetComponent<FPSController>();

            if (controller == null)
                controller = other.GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                Camera cam = other.GetComponentInChildren<Camera>();
                if (cam != null)
                    playerCamera = cam.transform;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isHiding)
        {
            playerInside = false;
            player = null;
        }
    }
}