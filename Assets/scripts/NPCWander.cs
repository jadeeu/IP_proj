using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleSidewalkWalk : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1.3f;
    public float turnSpeed = 4f;
    public float checkDistance = 6f;
    public float pauseTime = 1.5f;

    private NavMeshAgent agent;
    private bool isTurning = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        
        // Turn off automatic rotation so we can blend turns smoothly
        agent.updateRotation = false;

        SetNextPath();
    }

    void Update()
    {
        // Smoothly rotate character toward actual movement direction
        if (agent.velocity.sqrMagnitude > 0.05f)
        {
            Vector3 dir = agent.velocity.normalized;
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        // Detect reaching the end of the path OR hitting a boundary edge (walking on spot)
        if (!isTurning && (ReachedEnd() || IsStuck()))
        {
            StartCoroutine(TurnAround());
        }
    }

    bool ReachedEnd()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f;
    }

    bool IsStuck()
    {
        // Detects if character stopped moving forward while trying to walk
        return !agent.pathPending && agent.hasPath && agent.velocity.sqrMagnitude < 0.01f;
    }

    IEnumerator TurnAround()
    {
        isTurning = true;

        // Stop instantly so they don't run against the wall
        agent.isStopped = true;
        agent.ResetPath();

        // Human pause before turning
        yield return new WaitForSeconds(pauseTime);

        // Turn back onto the sidewalk
        SetNextPath();

        agent.isStopped = false;
        isTurning = false;
    }

    void SetNextPath()
    {
        // Raycast ahead on the NavMesh to check if the path hits an unwalkable edge
        Vector3 forwardTarget = transform.position + (transform.forward * checkDistance);
        NavMeshHit hit;

        if (!NavMesh.Raycast(transform.position, forwardTarget, out hit, NavMesh.AllAreas))
        {
            // Clear path ahead, keep walking forward
            agent.SetDestination(forwardTarget);
        }
        else
        {
            // Hitting an edge/wall ahead: turn 180 degrees back into open space
            Vector3 backTarget = transform.position - (transform.forward * checkDistance);
            if (NavMesh.SamplePosition(backTarget, out hit, checkDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }
}