using UnityEngine;

// Put this on the PLAYER (the object with the trigger collider).
// Touch the SUSPECT (Suspect marker) -> right popup (+points).
// Touch anyone else who enters the trigger -> wrong popup (-points).
public class ContactConfront : MonoBehaviour
{
    [Header("Popups")]
    public GameObject rightPopup;
    public GameObject wrongPopup;

    [Header("Scoring (optional - drag GameUIManager)")]
    public GameUIManager ui;
    public int winPoints = 15;
    public int penaltyPoints = 5;

    [Tooltip("Name fallback if the Suspect marker isn't on him. Leave blank to use the marker only.")]
    public string suspectName = "upskirter";

    [Header("Only react once")]
    public bool onlyOnce = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool done;

    void Start()
    {
        if (rightPopup != null) rightPopup.SetActive(false);
        if (wrongPopup != null) wrongPopup.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (onlyOnce && done) return;

        // Ignore things that aren't people (walls, floor) - only react to NPCs.
        // An NPC here = has a Suspect marker, an Innocent marker, OR matches the name.
        bool isSuspect = other.GetComponentInParent<Suspect>() != null
                      || other.GetComponentInChildren<Suspect>() != null
                      || (!string.IsNullOrEmpty(suspectName) &&
                          other.name.ToLower().Contains(suspectName.ToLower()));

        bool isPerson = isSuspect
                     || other.GetComponentInParent<Innocent>() != null
                     || other.GetComponentInChildren<Innocent>() != null
                     || other.CompareTag("NPC");

        if (debugLogs)
            Debug.Log($"[Contact] '{other.name}' entered. isPerson={isPerson}, isSuspect={isSuspect}.");

        if (!isPerson) return;   // not a civilian or suspect, ignore

        done = true;

        if (isSuspect)
        {
            if (ui != null) ui.AddPoints(winPoints);
            if (rightPopup != null) rightPopup.SetActive(true);
            else Debug.LogWarning("[Contact] Right Popup slot is EMPTY!");
        }
        else
        {
            if (ui != null) ui.RemovePoints(penaltyPoints);
            if (wrongPopup != null) wrongPopup.SetActive(true);
            else Debug.LogWarning("[Contact] Wrong Popup slot is EMPTY!");
        }
    }
}
