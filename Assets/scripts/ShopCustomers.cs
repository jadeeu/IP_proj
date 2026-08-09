using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopCustomers : MonoBehaviour
{
    [Header("Navigation Waypoints")]
    [Tooltip("Main doorway position where the customer pauses for the automatic door.")]
    public Transform mainDoorPoint;

    [Tooltip("Shelf or item display model the customer browses.")]
    public Transform shelfPoint;

    [Tooltip("Cashier or self-checkout location.")]
    public Transform checkoutPoint;

    [Tooltip("Point outside where the customer walks to before despawning.")]
    public Transform finalDespawnPoint;

    [Header("Timing Settings")]
    [Tooltip("Time spent waiting for the automatic door to open.")]
    public float doorWaitTime = 1.0f;

    [Header("Browsing Pause Range")]
    [Tooltip("Minimum wait time when approaching a display model.")]
    public float minBrowseTime = 2.0f;

    [Tooltip("Maximum wait time when approaching a display model.")]
    public float maxBrowseTime = 15.0f;

    [Tooltip("Time spent at the checkout counter.")]
    public float checkoutWaitTime = 3.0f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        // Realistic agent physical dimensions & avoidance settings
        agent.radius = 0.35f;
        agent.height = 1.8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 60);
    }

    private void Start()
    {
        // Random walking speed for varied customer movement
        agent.speed = Random.Range(1.3f, 1.8f);

        StartCoroutine(CustomerRoutine());
    }

    private IEnumerator CustomerRoutine()
    {
        // 1. Walk to entrance & wait for auto door
        if (mainDoorPoint != null)
        {
            yield return MoveToDestination(mainDoorPoint.position);
            yield return new WaitForSeconds(doorWaitTime);
        }

        // 2. Walk to display model / shelf & pause realistically
        if (shelfPoint != null)
        {
            yield return MoveToDestination(shelfPoint.position);

            // Rotate character to face the display model
            yield return FaceDirection(shelfPoint.forward);

            // Pause for a random time between 2 and 15 seconds
            float browseTime = Random.Range(minBrowseTime, maxBrowseTime);
            yield return new WaitForSeconds(browseTime);
        }

        // 3. Move to checkout
        if (checkoutPoint != null)
        {
            yield return MoveToDestination(checkoutPoint.position);
            yield return FaceDirection(checkoutPoint.forward);
            yield return new WaitForSeconds(checkoutWaitTime);
        }

        // 4. Exit through main doorway
        if (mainDoorPoint != null)
        {
            yield return MoveToDestination(mainDoorPoint.position);
            yield return new WaitForSeconds(doorWaitTime);
        }

        // 5. Walk outside and despawn
        if (finalDespawnPoint != null)
        {
            yield return MoveToDestination(finalDespawnPoint.position);
        }

        Destroy(gameObject);
    }

    private IEnumerator MoveToDestination(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        yield return new WaitUntil(() => !agent.pathPending);

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

        while (time < 0.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
    }
}