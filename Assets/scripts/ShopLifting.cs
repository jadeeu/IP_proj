using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopLifting : MonoBehaviour
{
    [Header("Sequence Positions")]
    public Transform outsideLoiterSpot;
    public Transform shopEntrance;
    public Transform shopExit;            // can be same as entrance
    public List<GameObject> targetModels = new List<GameObject>();

    [Header("Look Behavior Settings")]
    public Transform headOrBody;
    public float lookAngle = 45f;
    public float lookSpeed = 2f;

    [Header("Timing Settings")]
    public float outsideLoiterTime = 5f;
    public float minInspectTime = 3f;
    public float maxInspectTime = 6f;
    public float stealTellDuration = 4f;

    [Header("Anti-Stuck")]
    public float stuckCheckInterval = 1.5f;  // how often we check progress
    public float stuckDistanceThreshold = 0.1f; // moved less than this = stuck
    public float maxMoveTime = 20f;          // absolute cap per destination

    [Header("Theft Result")]
    public GameObject escapedPopup;
    public int escapePenalty = -7;

    public bool StealInProgress { get; private set; } = false;
    public bool HasStolen => hasStolen;

    private NavMeshAgent agent;
    private bool isWalking = false;
    private bool hasStolen = false;
    private bool wasCaught = false;
    private int stealAtIndex;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (headOrBody == null) headOrBody = transform;

        stealAtIndex = Random.Range(0, Mathf.Min(3, targetModels.Count));

        if (escapedPopup != null) escapedPopup.SetActive(false);

        StartCoroutine(CustomerSequenceRoutine());
        StartCoroutine(LookLeftAndRightRoutine());
    }

    private IEnumerator CustomerSequenceRoutine()
    {
        if (outsideLoiterSpot != null)
        {
            yield return MoveToDestination(outsideLoiterSpot.position);
            yield return new WaitForSeconds(outsideLoiterTime);
        }

        if (shopEntrance != null)
            yield return MoveToDestination(shopEntrance.position);

        int visitCount = Mathf.Min(3, targetModels.Count);

        for (int i = 0; i < visitCount; i++)
        {
            if (wasCaught) yield break;

            GameObject target = targetModels[i];
            if (target == null) continue;

            yield return MoveToDestination(target.transform.position);

            if (i == stealAtIndex)
            {
                StealInProgress = true;
                yield return new WaitForSeconds(stealTellDuration);
                if (wasCaught) yield break;

                target.SetActive(false);
                hasStolen = true;
            }

            float pauseDuration = Random.Range(minInspectTime, maxInspectTime);
            yield return new WaitForSeconds(pauseDuration);
        }

        Transform exit = shopExit != null ? shopExit : shopEntrance;
        if (exit != null)
            yield return MoveToDestination(exit.position);

        if (hasStolen && !wasCaught)
            Escaped();
    }

    private IEnumerator MoveToDestination(Vector3 targetPosition)
    {
        // Snap target onto walkable mesh (shelf items are usually off-mesh)
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            targetPosition = hit.position;

        // If the agent itself fell off the mesh, put it back
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit selfHit, 4f, NavMesh.AllAreas))
                agent.Warp(selfHit.position);
            else
            {
                Debug.LogWarning(name + ": agent lost off-mesh, warping straight to target");
                agent.Warp(targetPosition);
                yield break;
            }
        }

        agent.SetDestination(targetPosition);
        isWalking = true;

        float totalTime = 0f;
        float sinceCheck = 0f;
        Vector3 lastCheckedPos = transform.position;

        while (HorizontalDistance(transform.position, targetPosition) > Mathf.Max(agent.stoppingDistance, 0.6f))
        {
            totalTime += Time.deltaTime;
            sinceCheck += Time.deltaTime;

            // Progress check — the anti-stuck core
            if (sinceCheck >= stuckCheckInterval)
            {
                float progressed = HorizontalDistance(transform.position, lastCheckedPos);
                if (progressed < stuckDistanceThreshold)
                {
                    Debug.LogWarning(name + " stuck near " + transform.position + " — warping to destination");
                    agent.Warp(targetPosition);
                    break;
                }
                lastCheckedPos = transform.position;
                sinceCheck = 0f;
            }

            // Hard cap regardless
            if (totalTime > maxMoveTime)
            {
                Debug.LogWarning(name + " move timed out — warping to destination");
                agent.Warp(targetPosition);
                break;
            }

            yield return null;
        }

        isWalking = false;
    }

    private float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0; b.y = 0;
        return Vector3.Distance(a, b);
    }

    private void Escaped()
    {
        Debug.Log("Thief escaped!");
        if (GameManager.Instance != null)
            GameManager.Instance.AddPoints(escapePenalty);
        if (escapedPopup != null)
            escapedPopup.SetActive(true);
        gameObject.SetActive(false);
    }

    public void CaughtByPlayer()
    {
        if (wasCaught) return;
        wasCaught = true;
        StopAllCoroutines();
        Debug.Log("Thief caught!");
        gameObject.SetActive(false);
    }

    private IEnumerator LookLeftAndRightRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1.5f);
            if (isWalking) continue;

            Quaternion center = headOrBody.localRotation;

            yield return RotateTo(center * Quaternion.Euler(0, -lookAngle, 0));
            if (isWalking) continue;
            yield return new WaitForSeconds(0.6f);

            yield return RotateTo(center * Quaternion.Euler(0, lookAngle, 0));
            if (isWalking) continue;
            yield return new WaitForSeconds(0.6f);

            yield return RotateTo(center);
        }
    }

    private IEnumerator RotateTo(Quaternion targetRotation)
    {
        while (!isWalking && Quaternion.Angle(headOrBody.localRotation, targetRotation) > 0.5f)
        {
            headOrBody.localRotation = Quaternion.Slerp(headOrBody.localRotation, targetRotation, Time.deltaTime * lookSpeed);
            yield return null;
        }
    }
}