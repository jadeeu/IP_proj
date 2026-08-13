using UnityEngine;
using UnityEngine.AI;

public class BenchSitNPC : MonoBehaviour
{
    public enum NPCBenchState { WalkingToBench, RotatingToSit, Sitting }

    [Header("State Settings")]
    public NPCBenchState currentState = NPCBenchState.WalkingToBench;

    [Header("Bench Setup")]
    public Transform benchSeatTarget;
    public Transform benchFacingDirection;

    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float stopDistance = 0.5f;
    public float rotateSpeed = 6.0f;
    public float seatingHeightOffset = 0.1f;

    [Tooltip("Extra rotation offset in degrees (Y axis). For example, 180 makes them face opposite.")]
    public float rotationOffsetY = 0f;

    [Header("Optional Animation")]
    public Animator animator;
    public string sitAnimationBool = "IsSitting";
    public string walkAnimationBool = "IsWalking";

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = stopDistance;
            agent.updateRotation = false; // manual rotation
        }
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCBenchState.WalkingToBench:
                MoveToBench();
                break;

            case NPCBenchState.RotatingToSit:
                AlignWithBench();
                break;

            case NPCBenchState.Sitting:
                break;
        }

        // Face movement direction while walking
        if (currentState == NPCBenchState.WalkingToBench && agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(agent.velocity.normalized);
            lookRot *= Quaternion.Euler(0, rotationOffsetY, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }
    }

    private void MoveToBench()
    {
        if (benchSeatTarget == null || agent == null) return;

        if (animator != null && !string.IsNullOrEmpty(walkAnimationBool))
            animator.SetBool(walkAnimationBool, true);

        agent.SetDestination(benchSeatTarget.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (animator != null && !string.IsNullOrEmpty(walkAnimationBool))
                animator.SetBool(walkAnimationBool, false);

            agent.ResetPath(); // stop moving
            currentState = NPCBenchState.RotatingToSit;
        }
    }

    private void AlignWithBench()
    {
        Vector3 targetForward = benchFacingDirection != null ? benchFacingDirection.forward : benchSeatTarget.forward;
        Quaternion targetRotation = Quaternion.LookRotation(targetForward);
        targetRotation *= Quaternion.Euler(0, rotationOffsetY, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 2.0f)
        {
            transform.rotation = targetRotation;

            Vector3 seatedPos = benchSeatTarget.position;
            seatedPos.y += seatingHeightOffset;
            transform.position = seatedPos;

            currentState = NPCBenchState.Sitting;

            if (animator != null && !string.IsNullOrEmpty(sitAnimationBool))
                animator.SetBool(sitAnimationBool, true);
        }
    }
}
