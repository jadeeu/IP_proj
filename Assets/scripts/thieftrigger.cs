using UnityEngine;

// Put this on the PLANE. The plane's collider must have "Is Trigger" checked.
// When the Player steps on it, the thief starts walking into the store.
public class ThiefTrigger : MonoBehaviour
{
    [Header("Drag the thief NPC here")]
    public ThiefAI thief;

    private bool used;

    void OnTriggerEnter(Collider other)
    {
        if (used) return;

        if (other.CompareTag("Player"))
        {
            used = true;
            thief.Activate();
        }
    }
}