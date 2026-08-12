using System.Collections;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("UI & Camera Elements")]
    public GameObject dialogueBox;          // UI panel containing dialogue text
    public UnityEngine.UI.Text dialogueText; // Text component to display lines
    public GameObject signboardUI;          // The signboard panel to show at the end
    public Camera mainPlayerCamera;         // Your main walking/player camera
    public Camera cashierCamera;            // The fixed camera positioned at the cashier

    [Header("Player Movement Control")]
    public MonoBehaviour playerMovementScript; // Script that controls player movement (e.g., FirstPersonController)

    private int dialogueStep = 0;
    private bool isPlayerNearby = false;

    void Update()
    {
        // Press 'E' to interact or advance dialogue when nearby
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    void AdvanceDialogue()
    {
        dialogueBox.SetActive(true);

        if (dialogueStep == 0)
        {
            dialogueText.text = "There has been a rise of shop thefts recently... I actually caught a teen shoplifting earlier today!";
            dialogueStep++;
        }
        else if (dialogueStep == 1)
        {
            dialogueText.text = "Anyway, I have to go now. Bye!";
            dialogueStep++;
        }
        else if (dialogueStep == 2)
        {
            // Hide dialogue box and trigger NPC leave sequence
            dialogueBox.SetActive(false);
            StartCoroutine(WalkAwayAndSwitchToCashier());
        }
    }

    IEnumerator WalkAwayAndSwitchToCashier()
    {
        // 1. Make NPC walk forward/away (simple local translation over time)
        float walkTime = 3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < walkTime)
        {
            transform.Translate(Vector3.forward * 2f * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Disable NPC after walking away
        gameObject.SetActive(false);

        // 2. Lock player movement completely
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // 3. Switch camera perspective to Cashier Remote View
        if (mainPlayerCamera != null) mainPlayerCamera.gameObject.SetActive(false);
        if (cashierCamera != null) cashierCamera.gameObject.SetActive(true);

        // 4. Show Signboard UI
        if (signboardUI != null)
        {
            signboardUI.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            dialogueBox.SetActive(false);
        }
    }
}