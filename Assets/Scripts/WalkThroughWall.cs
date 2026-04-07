using UnityEngine;

public class WalkThroughWall : MonoBehaviour
{
    public bool ghostMode = false;
    private BoxCollider wallCollider;

    void Start()
    {
        wallCollider = GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (wallCollider == null) return;

        wallCollider.enabled = !ghostMode;
    }
}