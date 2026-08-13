using UnityEngine;

public class WarningTrigger : MonoBehaviour
{
    public GameUIManager uiManager;

    [TextArea(2, 4)]
    public string warningMessage = "Be careful and do not jaywalk!";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasTriggered)
            return;

        hasTriggered = true;

        uiManager.ShowWarning(warningMessage);
    }
}