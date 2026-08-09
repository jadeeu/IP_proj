using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopCustomers : MonoBehaviour
{
    [Header("Navigation Waypoints")]
    [Tooltip("Main doorway position where the customer pauses for the automatic door.")]
    public Transform mainDoorPoint;

    [Tooltip("Shelf or aisle location the customer browses.")]
    public Transform shelfPoint;

    [Tooltip("Cashier or self-checkout location.")]
    public Transform checkoutPoint;

    [Tooltip("Point outside where the customer walks to before despawning.")]
    public Transform finalDespawnPoint;

    [Header("Timing Settings")]
    [Tooltip("Time spent waiting for the automatic door to open.")]
    public float doorWaitTime = 1.0f;

    [Tooltip("Minimum time spent in the shop browsing.")]
    public float minStoreTime = 3.0f;

    [Tooltip("Maximum time spent in the shop browsing.")]
    public float maxStoreTime = 15.0f;

    [Tooltip("Time spent at the checkout counter.")]
    public float checkoutWaitTime = 3.0f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Set realistic physical dimensions and obstacle avoidance properties
        agent.radius = 0.35f;
        agent.height = 1.8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 60); // Prevents deadlocks when NPCs cross paths
    }

    private void Start()
    {
        // Give each customer a slightly different walk speed for natural movement
        agent.speed = Random.Range(1.3f, 1.8f);

        StartCoroutine(CustomerRoutine());
    }

    private IEnumerator CustomerRoutine()
    {
        // 1. Walk to the shop entrance & pause for door
        if (mainDoorPoint != null)
        {
            yield return MoveToDestination(mainDoorPoint.position);
            yield return new WaitForSeconds(doorWaitTime);
        }

        // 2. Walk into the store to browse the shelf
        if (shelfPoint != null)
        {
            yield return MoveToDestination(shelfPoint.position);

            // Turn NPC to face the shelf's forward direction
            yield return FaceDirection(shelfPoint.forward);

            // Browse for a random time between 3 and 15 seconds
            float shopDuration = Random.Range(minStoreTime, maxStoreTime);
            yield return new WaitForSeconds(shopDuration);
        }

        // 3. Approach the cashier or self-checkout
        if (checkoutPoint != null)
        {
            yield return MoveToDestination(checkoutPoint.position);

            // Turn NPC to face the counter/register
            yield return FaceDirection(checkoutPoint.forward);

            // Pay / wait at checkout for 3 seconds
            yield return new WaitForSeconds(checkoutWaitTime);
        }

        // 4. Head back out through the main doorway
        if (mainDoorPoint != null)
        {
            yield return MoveToDestination(mainDoorPoint.position);
            yield return new WaitForSeconds(doorWaitTime);
        }

        // 5. Walk to the outdoor despawn point
        if (finalDespawnPoint != null)
        {
            yield return MoveToDestination(finalDespawnPoint.position);
        }

        // Customer lifecycle complete
        Destroy(gameObject);
    }

    private IEnumerator MoveToDestination(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);

        // Wait until path calculation is finished
        yield return new WaitUntil(() => !agent.pathPending);

        // Walk until destination is reached
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
    }

    private IEnumerator FaceDirection(Vector3 forwardDirection)
    {
        if (forwardDirection == Vector3.zero) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection);
        float time = 0f;

        // Smoothly rotate character over 0.5 seconds
        while (time < 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}