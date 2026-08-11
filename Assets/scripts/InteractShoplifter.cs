using UnityEngine;

public class CashierGameManager : MonoBehaviour
{
    // Singleton instance so other scripts can access it easily
    public static CashierGameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int currentScore = 10;

    private void Awake()
    {
        // Setup Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPoints(int points)
    {
        currentScore += points;
        Debug.Log($"Points added: {points}. Total Score: {currentScore}");
    }

    public void DeductPoints(int points)
    {
        currentScore -= points;
        Debug.Log($"Points deducted: {points}. Total Score: {currentScore}");
    }

    public void OnShoplifterCaught()
    {
        AddPoints(20);
        Debug.Log("Shoplifter caught successfully!");
    }

    public void OnThiefEscaped()
    {
        DeductPoints(7);
        Debug.Log("Thief escaped!");
    }
}