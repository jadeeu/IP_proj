using UnityEngine;

public class AutoDoors : MonoBehaviour
{
    [Header("Door Setup")]
    [Tooltip("Drag the moving glass panel object here.")]
    public Transform doorMesh;

    [Tooltip("Movement offset when open (e.g., Y = 3 slides UP, X = 2 slides RIGHT).")]
    public Vector3 openOffset = new Vector3(0, 3f, 0);

    public float speed = 3.0f;
    public string targetTag = "NPC";

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private int customerCount = 0;

    private void Start()
    {
        if (doorMesh == null) doorMesh = transform;

        // Remember initial closed position
        closedPosition = doorMesh.localPosition;
        openPosition = closedPosition + openOffset;
    }

    private void Update()
    {
        // Smoothly move door toward open or closed position
        Vector3 targetPos = (customerCount > 0) ? openPosition : closedPosition;
        doorMesh.localPosition = Vector3.Lerp(doorMesh.localPosition, targetPos, Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) || other.CompareTag("Player"))
        {
            customerCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag) || other.CompareTag("Player"))
        {
            customerCount = Mathf.Max(0, customerCount - 1);
        }
    }
}