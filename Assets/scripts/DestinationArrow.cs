using UnityEngine;

public class DestinationArrow : MonoBehaviour
{
    [Header("Current Destination")]
    public Transform target;

    [Header("Player")]
    public Transform player;

    [Header("Arrow Settings")]
    public float heightAboveTarget = 2.5f;
    public float moveSpeed = 5f;

    [Header("Arrival Settings")]
    public float arrivalDistance = 2f;

    private void Update()
    {
        if (target == null)
            return;

        // Position arrow above the destination
        Vector3 targetPosition = target.position;
        targetPosition.y += heightAboveTarget;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Keep arrow pointing downward
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        // Check if player reached destination
        if (player != null)
        {
            float distance = Vector3.Distance(
                player.position,
                target.position
            );

            if (distance <= arrivalDistance)
            {
                HideArrow();
            }
        }
    }

    public void SetDestination(Transform newTarget)
    {
        target = newTarget;

        // Make sure arrow is visible when a new destination is assigned
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