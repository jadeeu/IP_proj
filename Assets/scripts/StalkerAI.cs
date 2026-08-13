using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Attach to the stalker NPC. Requires a NavMeshAgent + baked NavMesh + a Suspect marker.
// He loiters between points near the girl and keeps creepily turning to watch her.
public class StalkerAI : MonoBehaviour
{
    [Header("The person he is watching (the girl)")]
    public Transform watchTarget;

    [Header("Points he loiters between near the bus stop")]
    public Transform[] loiterPoints;

    [Header("How long he lingers at each point (seconds)")]
    public float minWait = 2f;
    public float maxWait = 5f;

    [Header("How fast he turns")]
    public float turnSpeed = 160f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent.stoppingDistance < 0.4f) agent.stoppingDistance = 0.4f;
        StartCoroutine(Loiter());
    }

    IEnumerator Loiter()
    {
        while (true)
        {
            if (loiterPoints != null && loiterPoints.Length > 0)
            {
                Transform p = loiterPoints[Random.Range(0, loiterPoints.Length)];
                agent.SetDestination(p.position);
                yield return WaitUntilArrived();
            }

            // Creepily face the girl
            if (watchTarget != null)
                yield return FaceTarget(watchTarget.position);

            yield return new WaitForSeconds(Random.Range(minWait, maxWait));

            // Occasional shifty glance away and back
            if (Random.value < 0.5f)
            {
                Quaternion original = transform.rotation;
                yield return RotateTo(original * Quaternion.Euler(0f, Random.Range(-80f, 80f), 0f));
                yield return new WaitForSeconds(0.5f);
                yield return RotateTo(original);
            }
        }
    }

    IEnumerator FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;

        agent.updateRotation = false;
        yield return RotateTo(Quaternion.LookRotation(dir));
        agent.updateRotation = true;
    }

    IEnumerator RotateTo(Quaternion targetRot)
    {
        agent.updateRotation = false;
        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    IEnumerator WaitUntilArrived()
    {
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f)
            yield return null;
    }
}
