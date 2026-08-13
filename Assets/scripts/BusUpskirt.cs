using UnityEngine;
using UnityEngine.AI;

public class SuspiciousFollower : MonoBehaviour
{
    public enum FollowerState { WalkingToDestination, BendingDown, Finished }

    [Header("Destination")]
    public Transform destinationPoint;

    [Header("Head & Rigging Slots")]
    public Transform headBone;

    [Header("Movement Settings")]
    public float walkSpeed = 2.2f;
    public float stopDistance = 1.0f;
    public float turnSpeed = 5.0f;

    [Header("Suspicious Head Movement")]
    public float glanceAngleMax = 35.0f;
    public float glanceSpeed = 3.0f;
    public float glanceInterval = 2.5f;

    [Header("Bending Settings")]
    public int bendDownCount = 3;       // total bends
    public float bendInterval = 3.0f;   // seconds between bends

    [Header("Animator Controls (Optional)")]
    public Animator animator;
    public string walkBoolParam = "IsWalking";
    public string bendDownBoolParam = "IsBendingDown";

    private float glanceTimer;
    private Quaternion originalHeadRotation;
    private Quaternion targetHeadRotation;
    private NavMeshAgent agent;

    private int bendsDone = 0;
    private float bendTimer = 0f;

    public FollowerState currentState = FollowerState.WalkingToDestination;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = walkSpeed;
            agent.stoppingDistance = stopDistance;
            agent.updateRotation = false;
        }

        if (headBone != null)
        {
            originalHeadRotation = headBone.localRotation;
            targetHeadRotation = originalHeadRotation;
        }
    }

    private void Update()
    {
        HandleSuspiciousHeadGlance();

        switch (currentState)
        {
            case FollowerState.WalkingToDestination:
                MoveToDestination();
                break;

            case FollowerState.BendingDown:
                HandleBending();
                break;

            case FollowerState.Finished:
                break;
        }

        // Face movement direction while walking
        if (currentState == FollowerState.WalkingToDestination && agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion lookRot = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
        }
    }

    private void MoveToDestination()
    {
        if (destinationPoint == null || agent == null) return;

        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
            animator.SetBool(walkBoolParam, true);

        agent.SetDestination(destinationPoint.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
                animator.SetBool(walkBoolParam, false);

            agent.ResetPath();
            currentState = FollowerState.BendingDown;
        }
    }

    private void HandleBending()
    {
        bendTimer += Time.deltaTime;

        if (bendTimer >= bendInterval && bendsDone < bendDownCount)
        {
            bendTimer = 0f;
            bendsDone++;

            if (animator != null && !string.IsNullOrEmpty(bendDownBoolParam))
                animator.SetBool(bendDownBoolParam, true);

            // Reset after short delay so animation can play again
            Invoke(nameof(ResetBendAnim), 1.0f);
        }

        if (bendsDone >= bendDownCount)
        {
            currentState = FollowerState.Finished;
        }
    }

    private void ResetBendAnim()
    {
        if (animator != null && !string.IsNullOrEmpty(bendDownBoolParam))
            animator.SetBool(bendDownBoolParam, false);
    }

    private void HandleSuspiciousHeadGlance()
    {
        if (headBone == null) return;

        glanceTimer += Time.deltaTime;
        if (glanceTimer >= glanceInterval)
        {
            glanceTimer = 0f;
            float randomY = Random.Range(-glanceAngleMax, glanceAngleMax);
            targetHeadRotation = originalHeadRotation * Quaternion.Euler(0, randomY, 0);
        }

        headBone.localRotation = Quaternion.Slerp(headBone.localRotation, targetHeadRotation, Time.deltaTime * glanceSpeed);
    }
}
