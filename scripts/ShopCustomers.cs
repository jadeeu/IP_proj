using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopCustomers : MonoBehaviour
{
    [Header("Navigation Waypoints")]
    [Tooltip("Main doorway position the customer walks through.")]
    public Transform mainDoorPoint;

    [Tooltip("Shelf or item display model the customer browses.")]
    public Transform shelfPoint;

    [Tooltip("Cashier or self-checkout location.")]
    public Transform checkoutPoint;

    [Tooltip("Point outside where the customer walks to before despawning.")]
    public Transform finalDespawnPoint;

    [Header("Browsing Pause Range")]
    [Tooltip("Minimum wait time when approaching a display model.")]
    public float minBrowseTime = 2.0f;

    [Tooltip("Maximum wait time when approaching a display model.")]
    public float maxBrowseTime = 15.0f;

    [Tooltip("Time spent at the checkout counter.")]
    public float checkoutWaitTime = 3.0f;

    private NavMeshAgent agent;
    private Animator animator;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.radius = 0.35f;
        agent.height = 1.8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(30, 60);
    }

    private void Start()
    {
        agent.speed = Random.Range(1.3f, 1.8f);
        StartCoroutine(CustomerRoutine());
    }

    private void Update()
    {
        // Keeps walking/idle animation in sync with speed
        if (animator != null && agent != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    private IEnumerator CustomerRoutine()
    {
        // 1. Walk through main entrance directly
        if (mainDoorPoint != null)
        {
            yield return MoveToDestination(mainDoorPoint.position);
        }

        // 2. Walk to display model / shelf & pause to browse
        if (shelfPoint != null)
        {
            yield return MoveToDestination(shelfPoint.position);
            yield return FaceDirection(shelfPoint.forward);

            float browseTime = Random.Range(minBrowseTime, maxBrowseTime);
            yield return PauseMovement(browseTime);
        }

        // 3. Move to checkout
        if (checkoutPoint != null)
        {
            yield return MoveToDestination(checkoutPoint.position);
            yield return FaceDirection(checkoutPoint.forward);
            yield return PauseMovement(checkoutWaitTime);
        }

        // 4. Exit through main doorway directly
        if (mainDoorPoint != null)
        {
            yield return MoveToDestination(mainDoorPoint.position);
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
        agent.isStopped = false;
        agent.SetDestination(targetPosition);

        yield return new WaitUntil(() => !agent.pathPending);

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }
    }

    private IEnumerator PauseMovement(float seconds)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero; // Stops walking on the spot when paused at shelves/checkout

        yield return new WaitForSeconds(seconds);

        agent.isStopped = false;
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