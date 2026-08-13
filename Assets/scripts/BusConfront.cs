using UnityEngine;
using UnityEngine.InputSystem; // Required for the New Input System

public class ConfrontationSystem : MonoBehaviour
{
    [Header("Score Settings")]
    public int currentScore = 0;
    public int winPoints = 15;
    public int penaltyPoints = 5;

    // Track the NPC currently nearby
    private NPC targetNPC;

    void Update()
    {
        // Check if the Keyboard is connected and if the C key was pressed this frame
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (targetNPC != null)
            {
                Confront(targetNPC);
            }
        }
    }

    private void Confront(NPC npc)
    {
        // Prevent confronting the same NPC multiple times
        if (npc.hasBeenConfronted)
        {
            Debug.Log("You have already confronted this person.");
            return;
        }

        npc.hasBeenConfronted = true;

        if (npc.isSuspicious)
        {
            currentScore += winPoints;
            Debug.Log($"Confronted the right person! +{winPoints} points. Total Score: {currentScore}");
            ProceedToNextLevel();
        }
        else
        {
            currentScore -= penaltyPoints;
            Debug.Log($"They were innocent! They got angry. -{penaltyPoints} points. Total Score: {currentScore}");
        }
    }

    private void ProceedToNextLevel()
    {
        Debug.Log("Proceeding to the next stage...");
        // Add your scene transition or game progression logic here
    }

    // Detect when player gets close to an NPC
    private void OnTriggerEnter(Collider other)
    {
        NPC npc = other.GetComponent<NPC>();
        if (npc != null)
        {
            targetNPC = npc;
            Debug.Log("Press 'C' to confront " + other.name);
        }
    }

    // Detect when player walks away from an NPC
    private void OnTriggerExit(Collider other)
    {
        NPC npc = other.GetComponent<NPC>();
        if (npc != null && npc == targetNPC)
        {
            targetNPC = null;
        }
    }
}