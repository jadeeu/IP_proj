using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        Entering,
        GrabbingBasket,
        Shopping,
        HeadingToCheckout,
        Paying,
        Exiting
    }

    [Header("Current State")]
    public CustomerState currentState;

    [Header("Store Locations")]
    [Tooltip("Waypoints in the aisles where customers stop to look at items.")]
    public List<Transform> shoppingWaypoints = new List<Transform>();
    
    [Tooltip("Point near entrance where baskets are stacked on the ground.")]
    public Transform basketStackPoint;
    
    [Tooltip("Point outside the doors where customers exit.")]
    public Transform exitPoint;

    [Header("Checkout System")]
    [Tooltip("Assign the CashierQueue component on your Player Cashier Counter.")]
    public CashierQueue playerCashierQueue;
    
    [Tooltip("Self-checkout counter targets if player cashier queue is full (> 2).")]
    public List<Transform> selfCheckoutCounters = new List<Transform>();

    [Header("Basket Setup")]
    [Tooltip("Drag the Basket GameObject that is parented INSIDE hand.R bone here.")]
    public GameObject handBasketProp;

    [Header("Money Visual FX")]
    [Tooltip("Prefab of the cash note or particle effect to spawn.")]
    public GameObject paymentNotesPrefab;
    
    [Tooltip("Empty GameObject placed near NPC's head for money spawn location.")]
    public Transform headPosition;

    [Header("Timers & Settings")]
    [Tooltip("Random shop time range in seconds.")]
    public float minShopTime = 2f;
    public float maxShopTime = 30f;
    
    [Tooltip("How long they pause at each shelf.")]
    public float browsePauseDuration = 3f;
    
    [Tooltip("Payment duration at checkout.")]
    public float paymentDuration = 3f;
    
    public float turnSpeed = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform currentTarget;
    private Vector3 currentQueueDestination;
    private bool isBrowsingPause = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.updateRotation = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Hide hand basket at start
        if (handBasketProp != null)
        {
            handBasketProp.SetActive(false);
        }

        // 1. Head to Basket Stack
        if (basketStackPoint != null)
        {
            currentState = CustomerState.GrabbingBasket;
            currentTarget = basketStackPoint;
            SetDestinationSafe(basketStackPoint.position);
        }
        else
        {
            if (handBasketProp != null) handBasketProp.SetActive(true);
            StartShopping();
        }
    }

    void Update()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        // Sync Animator Speed parameter
        if (animator != null)
        {
            float currentSpeed = (agent.isStopped || agent.velocity.sqrMagnitude < 0.01f) ? 0f : agent.velocity.magnitude;
            animator.SetFloat("Speed", currentSpeed);
        }

        // Check if destination reached
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
        {
            OnReachDestination();
        }
    }

    void OnReachDestination()
    {
        switch (currentState)
        {
            case CustomerState.GrabbingBasket:
                // Arrived at basket stack: Enable basket!
                if (handBasketProp != null)
                {
                    handBasketProp.SetActive(true);
                }
                StartShopping();
                break;

            case CustomerState.Shopping:
                if (!isBrowsingPause)
                {
                    StartCoroutine(PauseAndBrowseRoutine());
                }
                break;

            case CustomerState.HeadingToCheckout:
                // If they reached the front spot (Index 0 in line), start paying!
                if (playerCashierQueue != null && playerCashierQueue.waitingCustomers.IndexOf(this) == 0)
                {
                    currentState = CustomerState.Paying;
                    StartCoroutine(PaymentRoutine());
                }
                break;

            case CustomerState.Exiting:
                Destroy(gameObject);
                break;
        }
    }

    void StartShopping()
    {
        currentState = CustomerState.Shopping;
        StartCoroutine(ShoppingTimerRoutine());
        MoveToNextShopSpot();
    }

    private IEnumerator ShoppingTimerRoutine()
    {
        float totalShopDuration = Random.Range(minShopTime, maxShopTime);
        yield return new WaitForSeconds(totalShopDuration);
        
        GoToCheckout();
    }

    private IEnumerator PauseAndBrowseRoutine()
    {
        isBrowsingPause = true;

        // Freeze movement completely
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (animator != null) animator.SetFloat("Speed", 0f);

        float timer = 0f;
        while (timer < browsePauseDuration)
        {
            if (currentTarget != null)
            {
                SmoothFaceTarget(currentTarget.position);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        isBrowsingPause = false;

        if (currentState == CustomerState.Shopping)
        {
            MoveToNextShopSpot();
        }
    }

    void MoveToNextShopSpot()
    {
        if (shoppingWaypoints == null || shoppingWaypoints.Count == 0) return;

        currentTarget = shoppingWaypoints[Random.Range(0, shoppingWaypoints.Count)];
        SetDestinationSafe(currentTarget.position);
    }

    void GoToCheckout()
    {
        currentState = CustomerState.HeadingToCheckout;

        // If line is too long (> 2) and self checkout exists -> Divert
        if (playerCashierQueue != null && playerCashierQueue.GetQueueCount() > 2 && selfCheckoutCounters.Count > 0)
        {
            currentTarget = selfCheckoutCounters[Random.Range(0, selfCheckoutCounters.Count)];
            SetDestinationSafe(currentTarget.position);
        }
        else if (playerCashierQueue != null)
        {
            // Join the Player Cashier line
            playerCashierQueue.JoinQueue(this);
            Vector3 myLineSpot = playerCashierQueue.GetWaitingPosition(this);
            SetDestinationSafe(myLineSpot);
        }
        else
        {
            LeaveStore();
        }
    }

    // Called by CashierQueue whenever someone ahead leaves
    public void UpdateQueueTarget(Vector3 newQueuePosition)
    {
        currentQueueDestination = newQueuePosition;
        if (currentState == CustomerState.HeadingToCheckout)
        {
            SetDestinationSafe(newQueuePosition);
        }
    }

    private IEnumerator PaymentRoutine()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        if (animator != null) animator.SetFloat("Speed", 0f);

        float timer = 0f;

        // Face register while paying
        while (timer < paymentDuration)
        {
            if (playerCashierQueue != null && playerCashierQueue.lineStartSpot != null)
            {
                SmoothFaceTarget(playerCashierQueue.lineStartSpot.position + playerCashierQueue.lineStartSpot.forward);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Spawn Money Notes FX
        if (paymentNotesPrefab != null)
        {
            Vector3 spawnPos = headPosition != null ? headPosition.position : transform.position + Vector3.up * 2f;
            GameObject notes = Instantiate(paymentNotesPrefab, spawnPos, Quaternion.identity);
            Destroy(notes, 2.5f);
        }

        // Return Basket (Hide)
        if (handBasketProp != null)
        {
            handBasketProp.SetActive(false);
        }

        // Step out of queue so people behind shift forward
        if (playerCashierQueue != null)
        {
            playerCashierQueue.LeaveQueue(this);
        }

        yield return new WaitForSeconds(0.5f);

        agent.isStopped = false;
        LeaveStore();
    }

    void LeaveStore()
    {
        currentState = CustomerState.Exiting;
        currentTarget = exitPoint;

        if (exitPoint != null)
        {
            SetDestinationSafe(exitPoint.position);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetDestinationSafe(Vector3 target)
    {
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(target);
        }
    }

    void SmoothFaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }
}