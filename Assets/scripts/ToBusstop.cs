using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class WalkToBusStop : MonoBehaviour
{
    [Header("Movement Targets")]
    [Tooltip("Drag the transform/location of the bus stop here.")]
    public Transform busStopTarget;

    [Tooltip("Drag a target object (e.g. an empty object placed on the road) for the character to face when waiting.")]
    public Transform facingTarget;

    [Header("Settings")]
    [Tooltip("Distance from the bus stop to stop walking.")]
    public float stopDistance = 1.2f;

    [Tooltip("How smoothly they rotate to look at the facing target.")]
    public float turnSpeed = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private bool hasArrived = false;

    private int speedHash;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        speedHash = Animator.StringToHash("Speed");

        agent.autoTraverseOffMeshLink = true;

        // Set destination to the bus stop
        if (busStopTarget != null)
        {
            agent.SetDestination(busStopTarget.position);
        }
    }

    void Update()
    {
        if (!hasArrived)
        {
            // Update animator speed for locomotion/walking animation
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat(speedHash, currentSpeed);

            // Check distance to bus stop
            if (busStopTarget != null)
            {
                float distance = Vector3.Distance(transform.position, busStopTarget.position);
                if (distance <= stopDistance)
                {
                    ArriveAtStop();
                }
            }
        }
        else
        {
            // Smoothly turn to look at the facing target once arrived
            if (facingTarget != null)
            {
                Vector3 directionToTarget = facingTarget.position - transform.position;
                directionToTarget.y = 0; // Keep rotation level on the flat ground

                if (directionToTarget != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                }
            }
        }
    }

    void ArriveAtStop()
    {
        hasArrived = true;

        // Completely stop the NavMeshAgent
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        // Return animator speed to 0 (Idle animation)
        animator.SetFloat(speedHash, 0f);
    }
}