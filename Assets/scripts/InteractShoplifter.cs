using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class NPCSellerTalk : MonoBehaviour
{
    [Header("UI & Dialogue")]
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public GameObject pressEPrompt;

    [Header("Settings")]
    public Transform playerTransform;
    public Transform walkAwayTarget; // Where she walks after saying bye

    private NavMeshAgent agent;
    private Animator animator;
    private int dialogueStep = 0;
    private bool isPlayerInRange = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (pressEPrompt) pressEPrompt.SetActive(false);
    }

    void Update()
    {
        // When player is near and presses E
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            AdvanceDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (pressEPrompt) pressEPrompt.SetActive(true);
            
            // Stop NPC movement and face player
            if (agent) agent.isStopped = true;
            if (animator) animator.SetFloat("Speed", 0f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (pressEPrompt) pressEPrompt.SetActive(false);
            if (dialoguePanel) dialoguePanel.SetActive(false);
        }
    }

    void AdvanceDialogue()
    {
        if (pressEPrompt) pressEPrompt.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(true);

        dialogueStep++;

        if (dialogueStep == 1)
        {
            dialogueText.text = "There has been a rise of shop thefts recently... I actually caught a teen shoplifting earlier today!";
        }
        else if (dialogueStep == 2)
        {
            dialogueText.text = "Alright then, stay sharp. Bye!";
        }
        else if (dialogueStep == 3)
        {
            // End dialogue and walk away
            if (dialoguePanel) dialoguePanel.SetActive(false);
            StartCoroutine(WalkAway());
        }
    }

    IEnumerator WalkAway()
    {
        if (agent && walkAwayTarget)
        {
            agent.isStopped = false;
            agent.SetDestination(walkAwayTarget.position);
            if (animator) animator.SetFloat("Speed", 1f);
        }
        yield return null;
    }
}