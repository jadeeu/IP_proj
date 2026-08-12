using UnityEngine;

public class DestinationArrow : MonoBehaviour
{
    [Header("Current Destination")]
    public Transform target;

    [Header("Arrow Settings")]
    public float heightAboveTarget = 2.5f;
    public float moveSpeed = 5f;

    private void Update()
    {
        if (target == null)
            return;

        // Position the arrow above the destination
        Vector3 targetPosition = target.position;
        targetPosition.y += heightAboveTarget;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // Point the arrow downward
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            0f
        );
    }

    public void SetDestination(Transform newTarget)
    {
        target = newTarget;
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