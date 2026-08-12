using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopCustomer : MonoBehaviour
{
    [Header("Waypoints & Locations")]
    [Tooltip("Assign your shopping area wayfinder meshes/transforms here.")]
    public Transform[] wayfinders;
    
    [Tooltip("The cashier counter operated by the player.")]
    public Transform playerCashier;
    
    [Tooltip("The self-checkout counter.")]
    public Transform selfCheckout;
    
    [Tooltip("Where the customer walks to leave the store before being destroyed.")]
    public Transform exitPoint;

    [Header("UI / Money Visual")]
    [Tooltip("Prefab containing a TextMeshPro component for floating money FX.")]
    public GameObject moneyPopupPrefab;
    
    [Tooltip("Position above the head where money pops up.")]
    public Transform headTransform;

    [Header("Rotation Settings")]
    [Tooltip("How fast the customer rotates to face the target direction.")]
    public float turnSpeed = 5f;

    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        StartCoroutine(ShoppingRoutine());
    }

    private IEnumerator ShoppingRoutine()
    {
        // ---------------------------------------------------------------
        // 1. Shopping Loop (Between 2 to 30 Seconds Total Time)
        // ---------------------------------------------------------------
        float shoppingTimeLimit = Random.Range(2f, 30f);
        float timeSpentShopping = 0f;

        while (timeSpentShopping < shoppingTimeLimit)
        {
            if (wayfinders != null && wayfinders.Length > 0)
            {
                // Select a random wayfinder spot
                int randomIndex = Random.Range(0, wayfinders.Length);
                Transform targetWayfinder = wayfinders[randomIndex];

                // Walk to selected wayfinder spot
                yield return StartCoroutine(MoveToTarget(targetWayfinder));

                // Stop and browse for 2 to 5 seconds
                float browseDuration = Random.Range(2f, 5f);
                yield return new WaitForSeconds(browseDuration);

                timeSpentShopping += browseDuration;
            }
            else
            {
                yield return new WaitForSeconds(1f);
                timeSpentShopping += 1f;
            }
        }

        // ---------------------------------------------------------------
        // 2. Choose Checkout (30% Self-Checkout / 70% Player Cashier)
        // ---------------------------------------------------------------
        int checkoutRoll = Random.Range(1, 101); // 1 to 100
        Transform chosenCheckout = (checkoutRoll <= 70) ? playerCashier : selfCheckout;

        yield return StartCoroutine(MoveToTarget(chosenCheckout));

        // Stay at checkout for 2 to 5 seconds
        yield return new WaitForSeconds(Random.Range(2f, 5f));

        // Spawn Money Popup
        SpawnMoneyPopup();

        // ---------------------------------------------------------------
        // 3. Walk Out to Exit
        // ---------------------------------------------------------------
        yield return StartCoroutine(MoveToTarget(exitPoint));

        // Brief delay so they clearly exit the doorway before despawning
        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

    private IEnumerator MoveToTarget(Transform destination)
    {
        if (destination == null) yield break;

        agent.SetDestination(destination.position);

        // Wait until path calculation is complete
        while (agent.pathPending)
        {
            yield return null;
        }

        // Wait until agent reaches destination spot
        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        // --- NEW: Rotate Customer to match the Target's Facing Direction ---
        Quaternion targetRotation = destination.rotation;
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
            yield return null;
        }
        transform.rotation = targetRotation; // Lock final angle
    }

    private void SpawnMoneyPopup()
    {
        if (moneyPopupPrefab != null && headTransform != null)
        {
            GameObject popup = Instantiate(moneyPopupPrefab, headTransform.position, Quaternion.identity);
            
            TMP_Text tmp = popup.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = "+$" + Random.Range(10, 50);
            }

            Destroy(popup, 1.5f);
        }
    }
}