using UnityEngine;
using UnityEngine.AI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform player;
    public float sightRange = 15f;
    public float fieldOfView = 120f;
    //Didn't know what to call the layermask but this refers to any obstacle/wall
    public LayerMask obstructionMask;

    public float baseSpeed = 3.5f;
    public float maxSpeed = 8f;
    public float speedIncreaseRate = 1f;

    private NavMeshAgent agent;

    private float visibleTime = 0f;
    private bool canSeePlayer = false;

    private Vector3 lastSeenPosition;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = baseSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        CheckVision();
        if (canSeePlayer)
        {
            visibleTime += Time.deltaTime;

            // Increase speed based on how long player is visible
            float newSpeed = baseSpeed + (visibleTime * speedIncreaseRate);
            agent.speed = Mathf.Clamp(newSpeed, baseSpeed, maxSpeed);

            // Chase player
            lastSeenPosition = player.position;
            agent.SetDestination(player.position);
        }
        else
        {
            // Reset visibility timer
            visibleTime = 0f;
            agent.speed = baseSpeed;

            // Go to last seen position
            agent.SetDestination(lastSeenPosition);
        }
    }
    void CheckVision()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < sightRange)
        {
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle < fieldOfView / 2f)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstructionMask))
                {
                    canSeePlayer = true;
                    return;
                }
            }
        }

        canSeePlayer = false;
    }

}
