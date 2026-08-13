using UnityEngine;

// Put this on the PLANE. Tick "Is Trigger" on the plane's collider.
// When the player steps ON the plane: the stage popup shows immediately AND
// the confront ray turns on and stays on for the rest of the game.
public class RayZoneTrigger : MonoBehaviour
{
    [Header("Drag your ConfrontManager (on the player) here")]
    public ConfrontManager confront;

    [Header("Player tag")]
    public string playerTag = "Player";

    private bool used;   // only fire once

    void OnTriggerEnter(Collider other)
    {
        if (used || confront == null) return;

        if (other.CompareTag(playerTag))
        {
            used = true;
            confront.ActivateRay();     // ray on, permanently
            confront.ShowStagePopup();  // popup immediately on stepping on
        }
    }
}