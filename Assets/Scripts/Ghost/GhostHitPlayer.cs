using UnityEngine;

public class GhostHitPlayer : MonoBehaviour
{
    public GhostSpawner spawner;
    public Transform player;
    public float hitDistance = 1.5f;

    void Update()
    {
        if (spawner == null || player == null) return;
        if (!gameObject.activeInHierarchy) return;

        if (GhostManager.Instance != null && GhostManager.Instance.isPlayerHidden)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= hitDistance)
        {
            Debug.Log("Ghost reached player");
            spawner.GhostDespawned();
        }
    }
}