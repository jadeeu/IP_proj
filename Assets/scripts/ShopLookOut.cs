using System.Collections;
using UnityEngine;
using TMPro;

public class ShopLookOut : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public GameObject signboardUI;

    [Header("Movement & Targets")]
    public Transform exitWaypoint;
    public Transform shopLookArea; // Ensure this is NOT a child of the Coworker object in Hierarchy!
    public float walkSpeed = 2f;
    public float turnSpeed = 5f;

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string coworkerDialogue = "Hey! Thanks for taking over. Watch out today—there's been a rise in shop thefts recently. I actually caught a teen shoplifting earlier today. Anyway, see ya!";

    private bool isPlayerInZone = false;
    private bool hasInteracted = false;
    private Transform playerTransform;

    void Start()
    {
        // Hide Dialogue Box & Signboard immediately when scene starts
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (signboardUI != null)
        {
            signboardUI.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Idle state: Face toward shop look area until interacted with
        if (!hasInteracted && shopLookArea != null)
        {
            LookAtTarget(shopLookArea.position);
        }

        // 2. Press E to interact when near
        if (isPlayerInZone && !hasInteracted && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(StartTakeoverSequence());
        }
    }

    private IEnumerator StartTakeoverSequence()
    {
        hasInteracted = true;

        // Find player by Tag
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;

            // Lock player movement components
            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null) charController.enabled = false;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
        }

        // Turn to face the player face-to-face
        if (playerTransform != null)
        {
            yield return StartCoroutine(TurnToFace(playerTransform.position));
        }

        // Show Dialogue Box
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            if (dialogueText != null)
            {
                dialogueText.text = coworkerDialogue;
            }
        }

        // Wait 4 seconds for reading
        yield return new WaitForSeconds(4f);

        // Hide Dialogue Box
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // Coworker walks away
        yield return StartCoroutine(WalkToExit());

        // Show Signboard UI
        if (signboardUI != null)
        {
            signboardUI.SetActive(true);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator WalkToExit()
    {
        if (exitWaypoint == null) yield break;

        while (Vector3.Distance(transform.position, exitWaypoint.position) > 0.1f)
        {
            LookAtTarget(exitWaypoint.position);

            transform.position = Vector3.MoveTowards(
                transform.position,
                exitWaypoint.position,
                walkSpeed * Time.deltaTime
            );
            yield return null;
        }
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep horizontal plane rotation only

        if (direction.sqrMagnitude > 0.001f) // Safeguard against spinning when too close
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private IEnumerator TurnToFace(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Rotate until facing roughly towards player
        while (Quaternion.Angle(transform.rotation, targetRotation) > 5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }
}