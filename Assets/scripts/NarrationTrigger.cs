using UnityEngine;

public class NarrationTrigger : MonoBehaviour
{
    public GameUIManager uiManager;

    [TextArea(2, 4)]
    public string monologue;

    [TextArea(2, 4)]
    public string objective;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;

        // If there is a monologue, show it first.
        if (!string.IsNullOrEmpty(monologue))
        {
            uiManager.ShowMonologue(monologue, objective);
        }
        else
        {
            // If there is no monologue, show the objective immediately.
            uiManager.ShowObjective(objective);
        }
    }
}