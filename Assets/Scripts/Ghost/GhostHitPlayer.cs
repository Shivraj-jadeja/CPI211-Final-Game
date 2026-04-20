using UnityEngine;

public class GhostHitPlayer : MonoBehaviour
{
    public GhostSpawner spawner;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ghost trigger touched: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Ghost touched player");

            if (spawner != null)
            {
                spawner.GhostDespawned();
            }
            else
            {
                Debug.Log("Spawner reference is missing!");
            }
        }
    }
}