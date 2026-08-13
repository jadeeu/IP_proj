using UnityEngine;

// Put this on the SHOP PLANE. Tick "Is Trigger" on the plane's collider.
// When the player steps on it, the confront ray turns back on.
public class ShopPlaneTrigger : MonoBehaviour
{
    [Header("Drag the player's BusStopConfront here")]
    public BusStopConfront confront;

    [Header("Also show the instructions popup again?")]
    public bool showPopup = false;

    [Header("Player tag")]
    public string playerTag = "Player";

    private bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used || confront == null) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;
        confront.ActivateRay();                 // ray back on
        if (showPopup) confront.ShowStagePopup();
    }
}