using UnityEngine;

// Put this on the BUS-STOP PLANE. Tick "Is Trigger" on the plane's collider.
// When the player steps ON: the instructions popup shows and the ray turns on.
public class BusStopZoneTrigger : MonoBehaviour
{
    [Header("Drag your BusStopConfront (on the player) here")]
    public BusStopConfront confront;

    [Header("Player tag")]
    public string playerTag = "Player";

    private bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used || confront == null) return;

        if (other.CompareTag(playerTag))
        {
            used = true;
            confront.ActivateRay();     // ray on
            confront.ShowStagePopup();  // instructions popup immediately
        }
    }
}