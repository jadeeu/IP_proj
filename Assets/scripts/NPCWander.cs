using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NaturalNavMeshWanderer : MonoBehaviour
{
    [Header("Wander Settings")]
    [Tooltip("How far forward the NPC looks for a next target location.")]
    public float forwardDistance = 12f;

    [Tooltip("Maximum angle (in degrees) to turn left or right when picking the next point.")]
    public float maxTurnAngle = 45f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Enforce smooth forward-facing rotation
        agent.updateRotation = true;
        
        // High quality avoidance so NPCs smoothly step around each other
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Pick the first forward destination
        SetNextForwardDestination();
    }

    void Update()
    {
        // When the NPC gets close to its current goal, pick a new forward point immediately
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            SetNextForwardDestination();
        }
    }

    void SetNextForwardDestination()
    {
        // 1. Pick a random angle within a forward-facing cone (e.g. -45° to +45°)
        float randomAngle = Random.Range(-maxTurnAngle, maxTurnAngle);
        
        // 2. Calculate direction vector relative to where the character is currently facing
        Vector3 forwardDir = Quaternion.Euler(0, randomAngle, 0) * transform.forward;
        Vector3 targetPosition = transform.position + forwardDir * forwardDistance;

        // 3. Find a valid NavMesh point near that forward position
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, forwardDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
        else
        {
            // If hitting an edge or boundary, turn 180° around and continue forward
            transform.Rotate(0, 180f, 0);
            agent.SetDestination(transform.position + transform.forward * forwardDistance);
        }
    }
}