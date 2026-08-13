using UnityEngine;

public class AutoDoors : MonoBehaviour
{
    [Header("Door Leaves")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Settings")]
    public float slideDistance = 1.5f;
    public float openSpeed = 3.0f;
    public float stayOpenTime = 5f;   // how long doors stay open after last person leaves

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = false;
    private int occupants = 0;
    private float closeTimer = 0f;

    void Start()
    {
        if (leftDoor) leftClosedPos = leftDoor.localPosition;
        if (rightDoor) rightClosedPos = rightDoor.localPosition;

        if (leftDoor) leftOpenPos = leftClosedPos + Vector3.left * slideDistance;
        if (rightDoor) rightOpenPos = rightClosedPos + Vector3.right * slideDistance;
    }

    void Update()
    {
        // Count down only when nobody is inside the trigger
        if (isOpen && occupants == 0)
        {
            closeTimer -= Time.deltaTime;
            if (closeTimer <= 0f)
                isOpen = false;
        }

        Vector3 targetLeft = isOpen ? leftOpenPos : leftClosedPos;
        Vector3 targetRight = isOpen ? rightOpenPos : rightClosedPos;

        if (leftDoor)
            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, targetLeft, Time.deltaTime * openSpeed);

        if (rightDoor)
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, targetRight, Time.deltaTime * openSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            occupants++;
            isOpen = true;
            closeTimer = stayOpenTime;   // reset timer every time someone arrives
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            occupants = Mathf.Max(0, occupants - 1);
            if (occupants == 0)
                closeTimer = stayOpenTime;   // start the 5s countdown when last person leaves
        }
    }
}