using UnityEngine;

public class GrabItem : MonoBehaviour
{
    public float grabRange = 3f;
    public Transform holdPoint;

    private GameObject heldItem;

    public GameObject interactUI;

    public GameObject GetHeldItem()
    {
        return heldItem;
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem == null)
                TryGrab();
            else
                DropItem();
        }

        if (heldItem != null)
        {
            heldItem.transform.position = holdPoint.position;
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabRange))
        {
            if (hit.collider.CompareTag("Grabbable"))
            {
                interactUI.SetActive(true);
                return;
            }
        }

        // If no valid object → hide UI
        interactUI.SetActive(false);
    }

    void TryGrab()
    {
        if (Camera.main == null)
        {
            Debug.LogError("No MainCamera found. Tag your player camera as MainCamera.");
            return;
        }

        if (holdPoint == null)
        {
            Debug.LogError("HoldPoint is not assigned in GrabItem.");
            return;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, grabRange))
        {
            if (hit.collider.CompareTag("Grabbable"))
            {
                heldItem = hit.collider.gameObject;

                Rigidbody rb = heldItem.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                Collider col = heldItem.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }

                heldItem.transform.position = holdPoint.position;
                heldItem.transform.parent = holdPoint;
            }
        }
    }

    void DropItem()
    {
        if (heldItem == null)
            return;

        Rigidbody rb = heldItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider col = heldItem.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }

        heldItem.transform.parent = null;
        heldItem = null;
    }
}