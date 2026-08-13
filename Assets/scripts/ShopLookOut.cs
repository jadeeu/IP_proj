using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Standard for modern Unity text UI

public class ShopLookOut : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText; // TextMeshPro text slot
    public GameObject signboardUI;

    [Header("Movement & Targets")]
    public Transform exitWaypoint;
    public float walkSpeed = 2f;

    [Header("Dialogue Content")]
    [TextArea(3, 5)]
    public string coworkerDialogue = "Hey! Thanks for taking over. Watch out today—there's been a rise in shop thefts recently. I actually caught a teen shoplifting earlier today. Anyway, see ya!";

    private bool isPlayerInZone = false;
    private bool hasInteracted = false;

    void Update()
    {
        // Press E to interact when standing near the coworker
        if (isPlayerInZone && !hasInteracted && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(StartTakeoverSequence());
        }
    }

    private IEnumerator StartTakeoverSequence()
    {
        hasInteracted = true;

        // 1. Find player by Tag and disable movement components
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            // Disables CharacterController if present
            CharacterController charController = player.GetComponent<CharacterController>();
            if (charController != null) charController.enabled = false;

            // Freezes Rigidbody physics if present
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            // Disables custom movement scripts on the player object
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                script.enabled = false;
            }
        }

        // 2. Show Dialogue Box and write message
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
            if (dialogueText != null)
            {
                dialogueText.text = coworkerDialogue;
            }
        }

        // Wait 4 seconds for the player to read
        yield return new WaitForSeconds(4f); 

        // Hide Dialogue Box
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // 3. Coworker walks off-screen
        yield return StartCoroutine(WalkToExit());

        // 4. Show Signboard UI after she leaves
        if (signboardUI != null)
        {
            signboardUI.SetActive(true);
        }

        // Hide coworker object after reaching target destination
        gameObject.SetActive(false);
    }

    private IEnumerator WalkToExit()
    {
        if (exitWaypoint == null) yield break;

        while (Vector3.Distance(transform.position, exitWaypoint.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                exitWaypoint.position, 
                walkSpeed * Time.deltaTime
            );
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