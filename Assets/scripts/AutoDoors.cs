using UnityEngine;

public class AutoDoors : MonoBehaviour
{
    [Header("Door Setup")]
    public Transform doorMesh;
    public Vector3 openOffset = new Vector3(2f, 0, 0); // Direction to slide
    public float speed = 3.0f;
    public string targetTag = "NPC";

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private int npcCount = 0;

    private void Start()
    {
        if (doorMesh != null)
        {
            closedPosition = doorMesh.localPosition;
            openPosition = closedPosition + openOffset;
        }
    }

    private void Update()
    {
        if (doorMesh == null) return;

        Vector3 targetPos = (npcCount > 0) ? openPosition : closedPosition;
        doorMesh.localPosition = Vector3.Lerp(doorMesh.localPosition, targetPos, Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) || other.CompareTag("Player"))
        {
            npcCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag) || other.CompareTag("Player"))
        {
            npcCount = Mathf.Max(0, npcCount - 1);
        }
    }
}