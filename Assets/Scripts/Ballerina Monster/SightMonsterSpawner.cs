using UnityEngine;
using UnityEngine.AI;

public class SightMonsterSpawner : MonoBehaviour
{
    public Transform player;
    public SightMonsterAI monster;

    [Header("Spawn Settings")]
    public float spawnInterval = 20f;
    public float minSpawnDistance = 8f;
    public float maxSpawnDistance = 15f;
    public int maxSpawnAttempts = 10;

    private float timer;

    void Start()
    {
        timer = spawnInterval;

        if (monster != null)
            monster.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || monster == null) return;

        if (monster.gameObject.activeSelf)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnMonster();
            timer = spawnInterval;
        }
    }

    void SpawnMonster()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized *
                                   Random.Range(minSpawnDistance, maxSpawnDistance);

            Vector3 spawnPos = new Vector3(
                player.position.x + randomCircle.x,
                player.position.y,
                player.position.z + randomCircle.y
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 4f, NavMesh.AllAreas))
            {
                monster.transform.position = hit.position;

                Vector3 lookDir = player.position - monster.transform.position;
                lookDir.y = 0f;

                if (lookDir != Vector3.zero)
                    monster.transform.rotation = Quaternion.LookRotation(lookDir);

                monster.gameObject.SetActive(true);
                monster.ActivateMonster();

                return;
            }
        }

        Debug.Log("Sight monster spawn failed.");
    }

    public void DespawnMonster()
    {
        if (monster != null)
        {
            monster.DeactivateMonster();
            monster.gameObject.SetActive(false);
        }

        timer = spawnInterval;
    }
}