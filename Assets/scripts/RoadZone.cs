using UnityEngine;

public class RoadZone : MonoBehaviour
{
    public int penalty = -5;
    private bool penaltyApplied = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !penaltyApplied)
        {
            GameManager.Instance.AddPoints(penalty);
            penaltyApplied = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            penaltyApplied = false;
        }
    }
}