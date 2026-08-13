using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Attach to the thief NPC. Requires a NavMeshAgent + baked NavMesh.
//
// BEHAVIOR:
// - Roams the store like a normal shopper (blends in with the others).
// - When Activate() is called (player touches the plane -> ThiefTrigger),
//   he switches to thief mode: acts suspicious and steals "stealCount" items.
// - He CANNOT leave until "minStayTime" seconds have passed since activation.
//   If he finishes stealing early, he keeps nervously fake-browsing.
// - When the timer is up, he speed-walks to the exit and escapes.
//
// Other scripts can read: thief.HasStolen, thief.ItemsStolen, thief.IsActivated
public class ThiefAI : MonoBehaviour
{
    [Header("Points to browse at (same 14 store points is fine)")]
    public Transform[] browsePoints;

    [Header("Items he can steal (drag shelf item GameObjects here)")]
    public Transform[] stealableItems;
    public int stealCount = 3;

    [Tooltip("ON: picks Steal Count random items from the list.\nOFF: steals the first Steal Count items in the exact order you listed them.")]
    public bool randomizeSelection = true;

    [Header("Must stay in store this long after activation (seconds)")]
    public float minStayTime = 150f;     // 2.5 minutes

    [Header("Doors")]
    public Transform exit;

    [Header("Speeds")]
    public float normalSpeed = 3.5f;     // innocent browsing
    public float sneakSpeed = 1.8f;      // approaching an item
    public float escapeSpeed = 5.5f;     // leaving with the goods

    [Header("Suspicious behavior (only after activation)")]
    [Range(0f, 1f)]
    public float pauseChance = 0.5f;     // chance to stop mid-walk and glance around
    public float lookAngle = 70f;
    public float turnSpeed = 200f;

    [Header("Browse wait times (seconds)")]
    public float minWait = 2f;
    public float maxWait = 5f;

    [Header("After escaping")]
    public bool destroyAfterEscape = true;

    public bool IsActivated { get; private set; }
    public bool HasStolen   { get { return ItemsStolen > 0; } }
    public int  ItemsStolen { get; private set; }

    private NavMeshAgent agent;
    private float activationTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;
        agent.avoidancePriority = Random.Range(30, 70);
        if (agent.stoppingDistance < 0.5f)
            agent.stoppingDistance = 0.5f;

        StartCoroutine(InnocentRoam());
    }

    // Call this when the player confronts him: he panics and runs for the exit.
    public void Flee()
    {
        StopAllCoroutines();
        StartCoroutine(FleeRoutine());
    }

    IEnumerator FleeRoutine()
    {
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.speed = escapeSpeed;

        if (exit != null)
        {
            agent.SetDestination(exit.position);
            yield return WaitUntilArrived();
        }

        if (destroyAfterEscape)
            Destroy(gameObject);
    }

    // Called by ThiefTrigger when the player touches the plane.
    public void Activate()
    {
        if (stealableItems == null || stealableItems.Length == 0)
            Debug.LogWarning("ThiefAI: 'Stealable Items' is EMPTY on " + name +
                             " - drag shelf items into that array or he has nothing to steal!");

        if (IsActivated) return;
        IsActivated = true;
        activationTime = Time.time;

        StopAllCoroutines();             // stop innocent roaming
        StartCoroutine(ThiefRoutine());
    }

    float TimeLeftInStore()
    {
        return Mathf.Max(0f, minStayTime - (Time.time - activationTime));
    }

    // ---------- PHASE 1: blend in like a normal shopper ----------
    IEnumerator InnocentRoam()
    {
        while (true)
        {
            Transform point = browsePoints[Random.Range(0, browsePoints.Length)];
            agent.SetDestination(point.position);
            yield return WaitUntilArrived();

            yield return FaceDirection(point.forward);
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }
    }

    // ---------- PHASE 2: steal, then wait out the clock ----------
    IEnumerator ThiefRoutine()
    {
        // Pick which items to steal
        List<Transform> targets = new List<Transform>(stealableItems);
        if (randomizeSelection)
        {
            // Shuffle so he grabs random ones (no repeats)
            for (int i = 0; i < targets.Count; i++)
            {
                int j = Random.Range(i, targets.Count);
                (targets[i], targets[j]) = (targets[j], targets[i]);
            }
        }
        int count = Mathf.Clamp(stealCount, 1, targets.Count);

        // Steal each item, acting suspicious in between
        for (int i = 0; i < count; i++)
        {
            Transform item = targets[i];

            // Sneak to the item
            agent.speed = sneakSpeed;
            yield return WalkSuspiciously(item.position);

            // Face it, check over both shoulders, grab it
            yield return FaceDirection(item.position - transform.position);
            yield return LookAround();
            yield return LookAround();
            Steal(item);

            // Walk off at normal speed and fake-browse a point (cool-down)
            agent.speed = normalSpeed;
            if (browsePoints.Length > 0)
            {
                Transform point = browsePoints[Random.Range(0, browsePoints.Length)];
                yield return WalkSuspiciously(point.position);
                yield return FaceDirection(point.forward);
                yield return new WaitForSeconds(Random.Range(minWait, maxWait));
            }
        }

        // All items stolen, but he can't leave yet: nervously browse until time is up
        while (TimeLeftInStore() > 0f)
        {
            Transform point = browsePoints[Random.Range(0, browsePoints.Length)];
            yield return WalkSuspiciously(point.position);
            yield return FaceDirection(point.forward);
            yield return LookAround();
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));
        }

        // ---------- PHASE 3: escape ----------
        agent.speed = escapeSpeed;
        if (exit != null)
        {
            agent.SetDestination(exit.position);
            yield return WaitUntilArrived();
        }

        if (destroyAfterEscape)
            Destroy(gameObject);
    }

    void Steal(Transform item)
    {
        ItemsStolen++;

        item.SetParent(transform);
        item.localPosition = Vector3.zero;
        foreach (Renderer r in item.GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (Collider c in item.GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    // Walks toward a position, randomly stopping to glance around.
    IEnumerator WalkSuspiciously(Vector3 destination)
    {
        agent.SetDestination(destination);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            if (Random.value < pauseChance * Time.deltaTime)
            {
                agent.isStopped = true;
                yield return LookAround();
                agent.isStopped = false;
            }
            yield return null;
        }
    }

    // Turns left, then right, then back. Classic "is anyone watching?"
    IEnumerator LookAround()
    {
        agent.updateRotation = false;
        Quaternion original = transform.rotation;
        Quaternion left  = original * Quaternion.Euler(0f, -lookAngle, 0f);
        Quaternion right = original * Quaternion.Euler(0f,  lookAngle, 0f);

        yield return RotateTo(left);
        yield return new WaitForSeconds(0.4f);
        yield return RotateTo(right);
        yield return new WaitForSeconds(0.4f);
        yield return RotateTo(original);

        agent.updateRotation = true;
    }

    IEnumerator RotateTo(Quaternion targetRot)
    {
        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, turnSpeed * Time.deltaTime);
            yield return null;
        }
        transform.rotation = targetRot;
    }

    IEnumerator FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) yield break;

        agent.updateRotation = false;
        yield return RotateTo(Quaternion.LookRotation(direction));
        agent.updateRotation = true;
    }

    IEnumerator WaitUntilArrived()
    {
        float stuckTimer = 0f;

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance + 0.2f)
        {
            if (!agent.pathPending && agent.velocity.sqrMagnitude < 0.01f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 3f)
                {
                    if (agent.remainingDistance < 2f) break;
                    agent.SetDestination(agent.destination);
                    stuckTimer = 0f;
                }
            }
            else stuckTimer = 0f;

            yield return null;
        }
    }
}