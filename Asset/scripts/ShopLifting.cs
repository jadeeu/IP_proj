using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopLifting : MonoBehaviour
{
    [Header("Sequence Positions")]
    [Tooltip("Place an empty GameObject outside the shop where he should loiter first.")]
    public Transform outsideLoiterSpot;

    [Tooltip("Place an empty GameObject at the entrance threshold.")]
    public Transform shopEntrance;

    [Tooltip("Assign 3 meshes/models inside the shop for him to inspect.")]
    public List<GameObject> targetModels = new List<GameObject>();

    [Header("Timer Settings")]
    public float timeInStore = 0f;
    public float targetDuration = 60f; // 1 minute mark
    private bool hasStayedTooLong = false;
    private bool hasEnteredStore = false;

    [Header("Look Behavior Settings")]
    public Transform headOrBody; // Drag head or neck joint here
    public float lookAngle = 45f;
    public float lookSpeed = 2f;

    [Header("Timing Settings")]
    public float outsideLoiterTime = 5f; // Time spent outside before entering
    public float minInspectTime = 3f;     // Min time at each mesh
    public float maxInspectTime = 6f;     // Max time at each mesh

    private NavMeshAgent agent;
    private Quaternion originalRotation;
    private bool isWalking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (headOrBody == null)
        {
            headOrBody = transform;
        }

        originalRotation = headOrBody.localRotation;

        // Start the movement sequence and head-turning routines
        StartCoroutine(CustomerSequenceRoutine());
        StartCoroutine(LookLeftAndRightRoutine());
    }

    void Update()
    {
        // Only count store time once he passes the entrance
        if (hasEnteredStore && !hasStayedTooLong)
        {
            timeInStore += Time.deltaTime;

            if (timeInStore >= targetDuration)
            {
                hasStayedTooLong = true;
                OnStayedTooLong();
            }
        }
    }

    private IEnumerator CustomerSequenceRoutine()
    {
        // --- STEP 1: Walk to Outside Loiter Spot ---
        if (outsideLoiterSpot != null)
        {
            yield return MoveToDestination(outsideLoiterSpot.position);
            
            // Loiter outside and look around
            yield return new WaitForSeconds(outsideLoiterTime);
        }

        // --- STEP 2: Walk into the Shop ---
        if (shopEntrance != null)
        {
            yield return MoveToDestination(shopEntrance.position);
            hasEnteredStore = true; // Start timer
        }

        // --- STEP 3: Visit 3 Target Models Inside ---
        int visitCount = Mathf.Min(3, targetModels.Count);

        for (int i = 0; i < visitCount; i++)
        {
            GameObject target = targetModels[i];

            if (target != null)
            {
                yield return MoveToDestination(target.transform.position);

                // Stop at the mesh and inspect it while looking around
                float pauseDuration = Random.Range(minInspectTime, maxInspectTime);
                yield return new WaitForSeconds(pauseDuration);
            }
        }

        Debug.Log("Customer has completed inspecting all 3 models.");
    }

    // Helper method to handle NavMesh navigation smooth movement
    private IEnumerator MoveToDestination(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
        isWalking = true;

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        isWalking = false;
    }

    // Rotates head/body left and right while standing still
    private IEnumerator LookLeftAndRightRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.5f);

            if (!isWalking)
            {
                // Look Left
                Quaternion leftRotation = originalRotation * Quaternion.Euler(0, -lookAngle, 0);
                yield return RotateTo(leftRotation);

                yield return new WaitForSeconds(0.6f);

                // Look Right
                Quaternion rightRotation = originalRotation * Quaternion.Euler(0, lookAngle, 0);
                yield return RotateTo(rightRotation);

                yield return new WaitForSeconds(0.6f);

                // Return to Center
                yield return RotateTo(originalRotation);
            }
        }
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (Quaternion.Angle(headOrBody.localRotation, targetRotation) > 0.5f)
        {
            headOrBody.localRotation = Quaternion.Slerp(headOrBody.localRotation, targetRotation, Time.deltaTime * lookSpeed);
            yield return null;
        }
        headOrBody.localRotation = targetRotation;
    }

    private void OnStayedTooLong()
    {
        Debug.Log("Customer has been inside the store for over 1 minute!");
    }
}