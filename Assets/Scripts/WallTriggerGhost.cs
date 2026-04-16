using UnityEngine;

public class WallTriggerGhost : MonoBehaviour
{
    public WalkThroughWall wallScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            Debug.Log("Ghost entered wall trigger");

            GhostChase ghost = other.GetComponent<GhostChase>();
            if (ghost != null)
            {
                ghost.isPhasing = true;
            }

            if (wallScript != null)
            {
                wallScript.ghostMode = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ghost"))
        {
            Debug.Log("Ghost exited wall trigger");

            GhostChase ghost = other.GetComponent<GhostChase>();
            if (ghost != null)
            {
                ghost.isPhasing = false;
            }

            if (wallScript != null)
            {
                wallScript.ghostMode = false;
            }
        }
    }
}