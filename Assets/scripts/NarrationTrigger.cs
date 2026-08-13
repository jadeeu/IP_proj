using UnityEngine;

public class NarrationTrigger : MonoBehaviour
{
    [Header("UI Manager")]
    public GameUIManager uiManager;

    [Header("Narration")]
    [TextArea(2, 4)]
    public string monologue;

    [TextArea(2, 4)]
    public string objective;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Make sure only the player activates the trigger
        if (!other.CompareTag("Player"))
            return;

        // Prevent the narration from playing repeatedly
        if (hasTriggered)
            return;

        hasTriggered = true;

        // If there is a monologue, show it first.
        if (!string.IsNullOrEmpty(monologue))
        {
            uiManager.ShowMonologue(
                monologue,
                objective
            );
        }
        else
        {
            // If there is no monologue,
            // show the objective immediately.
            uiManager.ShowObjective(objective);
        }
    }
}