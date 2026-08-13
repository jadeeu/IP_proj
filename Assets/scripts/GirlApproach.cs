using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Attach to the girl NPC. Requires a NavMeshAgent + baked NavMesh.
// BeginApproach() is called after the player closes the stage instructions.
// She walks to the player, faces them, shows her warning popup, and then the
// confront ray is switched on so you can go find the stalker.
public class GirlApproach : MonoBehaviour
{
    [Header("Her warning popup ('that guy is following me...')")]
    public GameObject warningPopup;

    [Header("Turns the ray on after she speaks")]
    public BusStopConfront confront;

    [Header("How close she stops to the player (meters)")]
    public float stopDistance = 1.8f;

    [Header("Optional: auto-hide the warning after N seconds (0 = stays)")]
    public float autoHideSeconds = 0f;

    private NavMeshAgent agent;
    private bool started;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (warningPopup != null) warningPopup.SetActive(false);
    }

    public void BeginApproach(Transform player)
    {
        if (started) return;
        started = true;
        StartCoroutine(ApproachRoutine(player));
    }

    IEnumerator ApproachRoutine(Transform player)
    {
        agent.stoppingDistance = stopDistance;

        while (agent.pathPending || agent.remainingDistance > stopDistance + 0.2f)
        {
            agent.SetDestination(player.position);
            yield return null;
        }

        // Face the player
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        // She warns you
        if (warningPopup != null) warningPopup.SetActive(true);

        // NOW the ray turns on
        if (confront != null) confront.ActivateRay();

        // Optionally auto-hide her warning
        if (warningPopup != null && autoHideSeconds > 0f)
        {
            yield return new WaitForSeconds(autoHideSeconds);
            warningPopup.SetActive(false);
        }
    }
}