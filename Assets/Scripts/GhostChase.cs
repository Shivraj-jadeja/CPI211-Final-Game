using UnityEngine;
using UnityEngine.AI;

public class GhostChase : MonoBehaviour
{
    public Transform player;
    public float chaseDistance = 30f;
    public float phaseSpeed = 5.8f;

    [HideInInspector] public bool isPhasing = false;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > chaseDistance)
        {
            if (agent.enabled) agent.ResetPath();
            return;
        }

        if (isPhasing)
        {
            if (agent.enabled)
                agent.enabled = false;

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
                transform.rotation = Quaternion.LookRotation(lookDir);
        }
        else
        {
            if (!agent.enabled)
                agent.enabled = true;

            agent.SetDestination(player.position);
        }
    }
}