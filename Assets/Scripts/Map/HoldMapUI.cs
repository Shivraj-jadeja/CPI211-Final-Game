using UnityEngine;

public class HoldMapUI : MonoBehaviour
{
    public GameObject mapUI;
    public KeyCode mapKey = KeyCode.Tab;

    void Update()
    {
        if (Input.GetKey(mapKey))
        {
            mapUI.SetActive(true);
        }
        else
        {
            mapUI.SetActive(false);
        }
    }
}