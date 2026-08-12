using UnityEngine;

public class RoadZone : MonoBehaviour
{
    public int penaltyPerSecond = -5;
    private bool playerOnRoad = false;
    private float timer = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerOnRoad = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) { playerOnRoad = false; timer = 0f; }
    }

    void Update()
    {
        if (!playerOnRoad || GameManager.Instance == null) return;

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            GameManager.Instance.AddPoints(penaltyPerSecond);
            timer -= 1f;
        }
    }
}