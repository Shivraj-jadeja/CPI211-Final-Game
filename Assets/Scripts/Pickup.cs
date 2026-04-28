using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Item item;
    public void PickUp(PlayerInventory inventory)
    {
        if (inventory.AddItem(item))
        {
            Destroy(gameObject);
        }
    }
}
