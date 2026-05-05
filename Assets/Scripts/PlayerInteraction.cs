using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] public float range = 3f;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range))
            {
                Pickup pickup = hit.collider.GetComponent<Pickup>();
                if (pickup != null)
                {
                    Debug.Log("OH SHIT!!!");
                    pickup.PickUp(GetComponent<PlayerInventory>());
                }
            }
        }
    }
}
