using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 8f;
    [SerializeField] private float minWaitTime = 2f;
    [SerializeField] private float maxWaitTime = 5f;

    [Header("Stuck Prevention")]
    [SerializeField] private float maxStuckTime = 4f; // Reset target if blocked too long

    private NavMeshAgent agent;
    private Animator animator;
    private float waitTimer;
    private float currentWaitTime;
    private float stuckTimer;
    private Vector3 lastPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Enable auto-braking so they slow down smoothly near destination
        agent.autoBraking = true;
        
        SetNewDestination();
    }

    void Update()
    {
        // 1. Check if NPC reached destination
        if (!agent.pathPending && agent.hasPath)
        {
            if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            {
                waitTimer += Time.deltaTime;

                if (waitTimer >= currentWaitTime)
                {
                    SetNewDestination();
                    waitTimer = 0f;
                }
            }
        }

        // 2. Prevent walking into walls forever (Stuck Detection)
        if (Vector3.Distance(transform.position, lastPosition) < 0.05f && agent.hasPath)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= maxStuckTime)
            {
                SetNewDestination(); // Reset destination if stuck at a fence/wall
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        lastPosition = transform.position;

        // 3. Smooth animation speed transfer
        if (animator != null)
        {
            // Read agent's real movement speed
            float targetSpeed = agent.velocity.magnitude;
            
            // Send to animator (use DampTime for smooth acceleration/deceleration)
            animator.SetFloat("Speed", targetSpeed, 0.15f, Time.deltaTime);
        }
    }

    void SetNewDestination()
    {
        for (int i = 0; i < 30; i++) // Try up to 30 times to find a valid open spot
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                // Ensure there is a clear line of sight to the destination (no walls in between)
                if (!NavMesh.Raycast(transform.position, hit.position, out NavMeshHit rayHit, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                    currentWaitTime = Random.Range(minWaitTime, maxWaitTime);
                    return;
                }
            }
        }
    }
}