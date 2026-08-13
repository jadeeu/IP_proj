using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Attach this to each shopper NPC.
// Requires: a baked NavMesh + a NavMeshAgent component on the NPC.
//
// Shoppers roam RANDOMLY between the store points. No two shoppers
// go to the same point at the same time (points are "claimed").
// They enter, visit "pointsPerVisit" random points, then leave and respawn.
public class ShopperAI : MonoBehaviour
{
    [Header("All store points (drag all 14 here, same on every shopper)")]
    public Transform[] allPoints;

    [Header("How many random points to visit before leaving")]
    public int pointsPerVisit = 5;

    [Header("How long they browse at each point (seconds)")]
    public float minWait = 2f;
    public float maxWait = 6f;

    [Header("How fast they turn to face the shelf (degrees/sec)")]
    public float turnSpeed = 240f;

    [Header("Delay before this shopper starts walking (set 0 / 2 / 4 on your 3 NPCs)")]
    public float startDelay = 0f;

    [Header("Entrance / leaving (both optional)")]
    public Transform entrance;           // point just INSIDE the door
    public Transform exit;               // defaults to entrance if empty
    public bool respawnAsNewCustomer = true;
    public float respawnDelay = 5f;
    public Transform spawnPoint;         // outside spot to reappear at (default: start position)

    // Shared between all shoppers: which points are currently taken
    private static HashSet<Transform> claimedPoints = new HashSet<Transform>();

    private NavMeshAgent agent;
    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Give each shopper a different avoidance priority so they slide
        // around each other instead of deadlocking face-to-face.
        agent.avoidancePriority = Random.Range(30, 70);

        // A little breathing room when "arriving" so they don't fight
        // for the exact same spot on the floor.
        if (agent.stoppingDistance < 0.5f)
            agent.stoppingDistance = 0.5f;

        startPos = spawnPoint != null ? spawnPoint.position : transform.position;
        startRot = transform.rotation;

        StartCoroutine(LifeCycle());
    }

    IEnumerator LifeCycle()
    {
        // Stagger the start so all 3 don't rush the door at once
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // --- Enter the store ---
            if (entrance != null)
            {
                agent.SetDestination(entrance.position);
                yield return WaitUntilArrived();
            }

            // --- Visit N random, un-claimed points ---
            for (int visited = 0; visited < pointsPerVisit; visited++)
            {
                Transform point = ClaimRandomFreePoint();
                if (point == null) break;   // shouldn't happen with 14 points / 3 npcs

                agent.SetDestination(point.position);
                yield return WaitUntilArrived();

                yield return FaceDirection(point.forward);
                yield return new WaitForSeconds(Random.Range(minWait, maxWait));

                claimedPoints.Remove(point);   // free it up for the others
            }

            // --- Leave ---
            Transform door = exit != null ? exit : entrance;
            if (door != null)
            {
                agent.SetDestination(door.position);
                yield return WaitUntilArrived();
            }

            if (!respawnAsNewCustomer)
            {
                Destroy(gameObject);
                yield break;
            }

            // --- Respawn as a fresh customer ---
            SetVisible(false);
            agent.Warp(startPos);
            transform.rotation = startRot;
            yield return new WaitForSeconds(respawnDelay);
            SetVisible(true);
        }
    }

    Transform ClaimRandomFreePoint()
    {
        // Collect points nobody is using right now
        List<Transform> free = new List<Transform>();
        foreach (Transform p in allPoints)
            if (!claimedPoints.Contains(p))
                free.Add(p);

        if (free.Count == 0) return null;

        Transform pick = free[Random.Range(0, free.Count)];
        claimedPoints.Add(pick);
        return pick;
    }

    IEnumerator WaitUntilArrived()
    {
        float stuckTimer = 0f;

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            // Anti-stuck: if we're barely moving for 3 seconds (e.g. blocked
            // by another shopper in the doorway), accept "close enough" or repath.
            if (!agent.pathPending && agent.velocity.sqrMagnitude < 0.01f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 3f)
                {
                    if (agent.remainingDistance < 2f)
                        break;                              // close enough, move on
                    agent.SetDestination(agent.destination); // force a fresh path
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            yield return null;
        }
    }

    IEnumerator FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) yield break;

        agent.updateRotation = false;
        Quaternion targetRot = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRot;
        agent.updateRotation = true;
    }

    void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }
}