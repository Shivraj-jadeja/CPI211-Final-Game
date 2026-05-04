using UnityEngine;
using UnityEngine.AI;

public class SightMonsterAI : MonoBehaviour
{
    public Transform player;
    public Camera playerCamera;
    public SightMonsterSpawner spawner;

    [Header("Movement")]
    public float chaseSpeed = 5f;
    public float spinSpeed = 720f;

    [Header("Sight Settings")]
    public float despawnAfterSeenTime = 7f;
    public float viewCheckDistance = 60f;
    public float centerViewAmount = 0.45f;

    [Header("Audio")]
    public AudioSource monsterAudio;
    public AudioClip monsterMusic;
    public float minVolume = 0.1f;
    public float maxVolume = 1f;
    public float maxAudioDistance = 25f;

    private NavMeshAgent agent;
    private Collider monsterCollider;
    private Renderer monsterRenderer;

    private bool isActive = false;
    private float seenTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        monsterCollider = GetComponent<Collider>();
        monsterRenderer = GetComponentInChildren<Renderer>();

        agent.speed = chaseSpeed;

        if (monsterAudio != null)
        {
            monsterAudio.playOnAwake = false;
            monsterAudio.loop = true;
            monsterAudio.spatialBlend = 1f;

            if (monsterMusic != null)
                monsterAudio.clip = monsterMusic;

            monsterAudio.Stop();
        }
    }

    void Update()
    {
        if (!isActive) return;
        if (player == null || playerCamera == null) return;

        if (agent != null && agent.enabled && !agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        bool playerSeesMonster = IsSeenByPlayer();

        if (playerSeesMonster)
        {
            StopMonster();
            seenTimer += Time.deltaTime;

            if (seenTimer >= despawnAfterSeenTime)
            {
                if (spawner != null)
                    spawner.DespawnMonster();
                else
                    gameObject.SetActive(false);
            }

            return;
        }

        seenTimer = 0f;

        ChasePlayer();
        SpinMonster();
        PlayAudio();
        UpdateAudioVolume();
    }

    void ChasePlayer()
    {
        if (!agent.enabled)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    void StopMonster()
    {
        if (agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        StopAudio();
    }

    void SpinMonster()
    {
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
    }

    bool IsSeenByPlayer()
    {
        Vector3 checkPoint = GetMonsterLookPoint();

        Vector3 viewportPos = playerCamera.WorldToViewportPoint(checkPoint);

        if (viewportPos.z <= 0f)
            return false;

        float distance = Vector3.Distance(playerCamera.transform.position, checkPoint);

        if (distance > viewCheckDistance)
            return false;

        bool nearCenter =
            viewportPos.x > 0.5f - centerViewAmount &&
            viewportPos.x < 0.5f + centerViewAmount &&
            viewportPos.y > 0.5f - centerViewAmount &&
            viewportPos.y < 0.5f + centerViewAmount;

        return nearCenter;
    }

    Vector3 GetMonsterLookPoint()
    {
        if (monsterCollider != null)
            return monsterCollider.bounds.center;

        if (monsterRenderer != null)
            return monsterRenderer.bounds.center;

        return transform.position + Vector3.up;
    }

    void PlayAudio()
    {
        if (monsterAudio == null || monsterAudio.clip == null) return;

        if (!monsterAudio.isPlaying)
            monsterAudio.Play();
    }

    void StopAudio()
    {
        if (monsterAudio != null && monsterAudio.isPlaying)
            monsterAudio.Stop();
    }

    void UpdateAudioVolume()
    {
        if (player == null || monsterAudio == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float t = 1f - Mathf.Clamp01(distance / maxAudioDistance);

        monsterAudio.volume = Mathf.Lerp(minVolume, maxVolume, t);
    }

    public void ActivateMonster()
    {
        isActive = true;
        seenTimer = 0f;

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        PlayAudio();
    }

    public void DeactivateMonster()
    {
        isActive = false;
        seenTimer = 0f;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        StopAudio();
    }
}