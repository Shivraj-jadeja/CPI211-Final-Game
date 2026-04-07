using UnityEngine;
using UnityEngine.AI;

public class GhostChase : MonoBehaviour
{
    public Transform player;
    public float chaseDistance = 20f;
    public float phaseSpeed = 4.5f;
    public TrailRenderer trail;
    public float movementThreshold = 0.05f;

    [HideInInspector] public bool isPhasing = false;

    private NavMeshAgent agent;
    private Vector3 lastPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;

        if (trail != null)
        {
            trail.emitting = false;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > chaseDistance)
        {
            if (agent.enabled)
            {
                agent.ResetPath();
            }

            UpdateTrail();
            lastPosition = transform.position;
            return;
        }

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
            }

            agent.SetDestination(player.position);
        }

        UpdateTrail();
        lastPosition = transform.position;
    }

    void UpdateTrail()
    {
        if (trail == null) return;

        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        trail.emitting = movedDistance > movementThreshold;
    }
}