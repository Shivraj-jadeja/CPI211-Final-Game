using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Item[] slots = new Item[4];
    public int selectedSlot = 0;
    public Transform holdPoint;
    private GameObject currentHeldObject;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectSlot(3);
    }
    public bool AddItem(Item item)
    {
        if(item.itemName == "Medkit")
        {
            slots[1] = item;
            Debug.Log("YIIIIPPPEEEEE!!!");
            return true;
        }else if(item.itemName == "Potion")
        {
            slots[2] = item;
            return true;
        }else if(item.itemName == "Key")
        {
            slots[3] = item;
            return true;
        }

            Debug.Log("Shit aint work");
        return false;
    }
    void SelectSlot(int index)
    {
        selectedSlot = index;
        UpdateHeldItem();
        Debug.Log("Selected slot: " + (index + 1));
    }

    public Item GetSelectedItem()
    {
        return slots[selectedSlot];
    }
    void UpdateHeldItem()
    {
        // destroy old one
        if (currentHeldObject != null)
        {
            Destroy(currentHeldObject);
        }

        Item selectedItem = slots[selectedSlot];

        if (selectedItem != null && selectedItem.heldPrefab != null)
        {
            currentHeldObject = Instantiate(
                selectedItem.heldPrefab,
                holdPoint.position,
                holdPoint.rotation,
                holdPoint
            );
        }
    }
}
