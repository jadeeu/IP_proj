using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Respawn")]
    public Transform homePoint;
    public GameObject player;

    [Header("Score")]
    public int score = 10;

    void Awake()
    {
        Instance = this;
    }

    public void CarHit()
    {
        Debug.Log("Player was hit by a car!");
        RespawnAtHome();
    }

    public void AddPoints(int amount)
    {
        score += amount;
        Debug.Log($"Score: {score} ({(amount >= 0 ? "+" : "")}{amount})");
    }

    void RespawnAtHome()
    {
        if (homePoint == null || player == null) return;

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.transform.SetPositionAndRotation(homePoint.position, homePoint.rotation);

        if (cc != null) cc.enabled = true;
    }
}