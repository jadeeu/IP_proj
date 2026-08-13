using UnityEngine;
using UnityEngine.AI;

public class BusStopNPCBehaviour : MonoBehaviour
{
    public enum NPCBehaviorType { StandingAtStop, SittingOnBench }
    public enum NPCState { WalkingToTarget, AligningDirection, Waiting }

    [Header("Behavior Setup")]
    public NPCBehaviorType behaviorType = NPCBehaviorType.StandingAtStop;
    public NPCState currentState = NPCState.WalkingToTarget;

    [Header("Targets")]
    public Transform destinationTarget;
    public Transform lookTarget;

    [Header("Seating Options (For Bench Sitting)")]
    public float seatingHeightOffset = -0.1f;

    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;
    public float stopDistance = 1.0f;
    public float turnSpeed = 5.0f;

    [Header("Animator Components")]
    public Animator animator;
    public string walkBoolParam = "IsWalking";
    public string sitBoolParam = "IsSitting";

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stopDistance;
            agent.updateRotation = false; // we’ll rotate manually
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case NPCState.WalkingToTarget:
                MoveToDestination();
                break;

            case NPCState.AligningDirection:
                AlignToFacingDirection();
                break;

            case NPCState.Waiting:
                break;
        }

        // Face movement direction while walking
        if (currentState == NPCState.WalkingToTarget && agent != null && agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
        }
    }

    private void MoveToDestination()
    {
        if (destinationTarget == null || agent == null) return;

        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
            animator.SetBool(walkBoolParam, true);

        agent.SetDestination(destinationTarget.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
                animator.SetBool(walkBoolParam, false);

            currentState = NPCState.AligningDirection;
        }
    }

    private void AlignToFacingDirection()
    {
        Vector3 targetFacingDir = transform.forward;
        if (lookTarget != null)
        {
            targetFacingDir = (lookTarget.position - transform.position).normalized;
            targetFacingDir.y = 0;
        }

        if (targetFacingDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetFacingDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 2.0f)
            {
                transform.rotation = targetRotation;

                if (behaviorType == NPCBehaviorType.SittingOnBench)
                {
                    Vector3 seatedPos = destinationTarget.position;
                    seatedPos.y += seatingHeightOffset;
                    transform.position = seatedPos;

                    if (animator != null && !string.IsNullOrEmpty(sitBoolParam))
                        animator.SetBool(sitBoolParam, true);
                }

                currentState = NPCState.Waiting;
            }
        }
    }
}
