using UnityEngine;

// Put this on a PLANE. Tick "Is Trigger" on the plane's collider.
// When the player steps on it, they gain points via the GameUIManager.
public class PointsPlane : MonoBehaviour
{
    [Header("Drag your GameUIManager here")]
    public GameUIManager ui;

    [Header("How many points to give")]
    public int points = 10;

    [Header("Player tag")]
    public string playerTag = "Player";

    [Tooltip("If ON, this plane only gives points once. If OFF, it gives points every time the player steps on it.")]
    public bool onlyOnce = true;

    private bool used;

    void OnTriggerEnter(Collider other)
    {
        if (ui == null) return;
        if (onlyOnce && used) return;

        if (other.CompareTag(playerTag))
        {
            used = true;
            ui.AddPoints(points);
        }
    }
}
