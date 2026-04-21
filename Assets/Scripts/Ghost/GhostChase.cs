using UnityEngine;
using UnityEngine.AI;

public class GhostChase : MonoBehaviour
{
    public Transform player;
    public float chaseSpeed = 4.5f;
    public float phaseSpeed = 4.5f;
    public TrailRenderer trail;
    public float movementThreshold = 0.02f;

    [Header("Audio")]
    public AudioSource ghostAudio;
    public AudioClip ghostSound;
    public float minVolume = 0.1f;
    public float maxVolume = 1f;
    public float maxAudioDistance = 25f;

    [HideInInspector] public bool isPhasing = false;
    [HideInInspector] public bool isActive = false;

    private NavMeshAgent agent;
    private Vector3 lastPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = chaseSpeed;

        lastPosition = transform.position;

        if (trail != null)
        {
            trail.emitting = false;
        }

        if (ghostAudio != null)
        {
            ghostAudio.playOnAwake = false;
            ghostAudio.loop = true;
            ghostAudio.spatialBlend = 1f;

            if (ghostSound != null)
            {
                ghostAudio.clip = ghostSound;
            }

            ghostAudio.Stop();
        }
    }

    void Update()
    {
        if (!isActive)
        {
            StopAudio();

            if (trail != null)
            {
                trail.emitting = false;
            }

            if (agent != null && agent.enabled)
            {
                agent.ResetPath();
            }

            return;
        }

        PlayAudio();

        if (player == null) return;

        if (isPhasing)
        {
            if (agent.enabled)
            {
                agent.enabled = false;
            }

            Vector3 target = player.position;
            target.y = transform.position.y;

            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                phaseSpeed * Time.deltaTime
            );

            Vector3 lookDir = target - transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        else
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
                agent.Warp(transform.position);
            }

            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }

        UpdateTrail();
        UpdateAudioVolume();

        lastPosition = transform.position;
    }

    void UpdateTrail()
    {
        if (trail == null) return;

        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        trail.emitting = movedDistance > movementThreshold;
    }

    void PlayAudio()
    {
        if (ghostAudio == null || ghostAudio.clip == null) return;

        if (!ghostAudio.isPlaying)
        {
            ghostAudio.Play();
        }
    }

    void StopAudio()
    {
        if (ghostAudio != null && ghostAudio.isPlaying)
        {
            ghostAudio.Stop();
        }
    }

    void UpdateAudioVolume()
    {
        if (player == null || ghostAudio == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float t = 1f - Mathf.Clamp01(distance / maxAudioDistance);

        ghostAudio.volume = Mathf.Lerp(minVolume, maxVolume, t);
    }

    public void ActivateGhost()
    {
        isActive = true;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
    }

    public void DeactivateGhost()
    {
        isActive = false;
        isPhasing = false;

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
        }

        StopAudio();
    }
}