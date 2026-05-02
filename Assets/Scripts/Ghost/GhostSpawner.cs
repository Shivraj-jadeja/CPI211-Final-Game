using UnityEngine;
using UnityEngine.AI;

public class GhostSpawner : MonoBehaviour
{
    public Transform player;
    public GhostChase ghost;

    [Header("Spawn Timing")]
    public float spawnInterval = 40f;

    [Header("Spawn Distance")]
    public float minSpawnDistance = 8f;
    public float maxSpawnDistance = 15f;

    [Header("Spawn Attempts")]
    public int maxSpawnAttempts = 10;

    private float timer;

    void Start()
    {
        timer = spawnInterval;

        if (ghost != null)
        {
            ghost.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || ghost == null) return;

        if (ghost.gameObject.activeSelf)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnGhost();
            timer = spawnInterval;
        }
    }

    void SpawnGhost()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 spawnPos = new Vector3(
                player.position.x + randomCircle.x,
                player.position.y,
                player.position.z + randomCircle.y
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 3f, NavMesh.AllAreas))
            {
                ghost.transform.position = hit.position;
                ghost.transform.rotation = Quaternion.LookRotation(player.position - ghost.transform.position);

                ghost.gameObject.SetActive(true);
                ghost.ActivateGhost();

                return;
            }
        }

        Debug.Log("Ghost spawn failed - could not find valid NavMesh point.");
    }

    public void GhostDespawned()
    {
        if (ghost != null)
        {
            ghost.DeactivateGhost();
            ghost.gameObject.SetActive(false);
        }

        timer = spawnInterval;
    }
}