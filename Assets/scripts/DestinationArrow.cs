using UnityEngine;

public class DestinationArrow : MonoBehaviour
{
    [Header("Destinations")]
    public Transform[] destinations;

    [Header("Player")]
    public Transform player;

    [Header("Arrow Settings")]
    public float heightAboveTarget = 2.5f;
    public float moveSpeed = 5f;

    [Header("Arrival Settings")]
    public float arrivalDistance = 2f;

    private int currentDestinationIndex = 0;

    private void Start()
    {
        if (destinations.Length == 0)
        {
            Debug.LogWarning("No destinations assigned to the arrow.");
            return;
        }

        SetCurrentDestination();
    }

    private void Update()
    {
        if (destinations.Length == 0)
            return;

        Transform currentTarget =
            destinations[currentDestinationIndex];

        if (currentTarget == null)
            return;

        // =========================
        // MOVE ARROW ABOVE TARGET
        // =========================

        Vector3 targetPosition = currentTarget.position;

        targetPosition.y += heightAboveTarget;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );


        // =========================
        // KEEP ARROW POINTING DOWN
        // =========================

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            0f
        );


        // =========================
        // CHECK PLAYER DISTANCE
        // =========================

        if (player != null)
        {
            float distance = Vector3.Distance(
                player.position,
                currentTarget.position
            );

            if (distance <= arrivalDistance)
            {
                GoToNextDestination();
            }
        }
    }


    private void GoToNextDestination()
    {
        currentDestinationIndex++;

        // =========================
        // ALL DESTINATIONS COMPLETE
        // =========================

        if (currentDestinationIndex >= destinations.Length)
        {
            HideArrow();
            return;
        }


        // =========================
        // MOVE TO NEXT DESTINATION
        // =========================

        SetCurrentDestination();
    }


    private void SetCurrentDestination()
    {
        Transform target =
            destinations[currentDestinationIndex];

        if (target == null)
            return;

        ShowArrow();
    }


    public void HideArrow()
    {
        gameObject.SetActive(false);
    }


    public void ShowArrow()
    {
        gameObject.SetActive(true);
    }
}