using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DoorwayRouter : MonoBehaviour
{
    public Transform doorOutside;  
    public Transform doorInside;    // empty just inside the door

    // Walks agent through the door, then to finalTarget
    public IEnumerator EnterShopTo(NavMeshAgent agent, Vector3 finalTarget)
    {
        yield return MoveTo(agent, doorOutside.position);
        yield return MoveTo(agent, doorInside.position);
        yield return MoveTo(agent, finalTarget);
    }

    // Reverse for leaving
    public IEnumerator ExitShopTo(NavMeshAgent agent, Vector3 finalTarget)
    {
        yield return MoveTo(agent, doorInside.position);
        yield return MoveTo(agent, doorOutside.position);
        yield return MoveTo(agent, finalTarget);
    }

    private IEnumerator MoveTo(NavMeshAgent agent, Vector3 pos)
    {
        if (!agent.isOnNavMesh) yield break;
        agent.SetDestination(pos);

        float timeout = 15f;
        float t = 0f;
        while (t < timeout)
        {
            Vector3 a = agent.transform.position; a.y = 0;
            Vector3 b = pos; b.y = 0;
            if (Vector3.Distance(a, b) <= Mathf.Max(agent.stoppingDistance, 0.6f))
                break;
            t += Time.deltaTime;
            yield return null;
        }
    }
}