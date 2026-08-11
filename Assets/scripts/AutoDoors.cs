using UnityEngine;

public class AutoDoors : MonoBehaviour
{
    [Header("Door Leaves")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Settings")]
    public float slideDistance = 1.5f; // How far doors move apart
    public float openSpeed = 3.0f;     // Speed of opening/closing

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = false;

    void Start()
    {
        // Save initial closed positions (local position)
        if (leftDoor) leftClosedPos = leftDoor.localPosition;
        if (rightDoor) rightClosedPos = rightDoor.localPosition;

        // Calculate open positions along local X axis
        if (leftDoor) leftOpenPos = leftClosedPos + Vector3.left * slideDistance;
        if (rightDoor) rightOpenPos = rightClosedPos + Vector3.right * slideDistance;
    }

    void Update()
    {
        Vector3 targetLeft = isOpen ? leftOpenPos : leftClosedPos;
        Vector3 targetRight = isOpen ? rightOpenPos : rightClosedPos;

        if (leftDoor)
            leftDoor.localPosition = Vector3.Lerp(leftDoor.localPosition, targetLeft, Time.deltaTime * openSpeed);

        if (rightDoor)
            rightDoor.localPosition = Vector3.Lerp(rightDoor.localPosition, targetRight, Time.deltaTime * openSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect player or NPCs
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC") || other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }
}