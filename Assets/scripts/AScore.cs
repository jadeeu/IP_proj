using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Score & Stage Tracking")]
    public int totalPoints = 0;
    public int stagesCleared = 0;
    public int totalStages = 3;

    [Header("Player Actions (For Summary)")]
    public bool helpedPeer = false;
    public bool reportedDanger = false;
    public bool madeMistake = false;

    private void Awake()
    {
        // Singleton setup so it persists across stage interactions
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    /// <summary>
    /// Call this to add positive points (e.g. ScoreManager.Instance.AddPoints(20))
    /// </summary>
    public void AddPoints(int amount)
    {
        totalPoints += amount;
        UpdateUI();
    }

    /// <summary>
    /// Call this to subtract points for mistakes (e.g. ScoreManager.Instance.DeductPoints(10))
    /// </summary>
    public void DeductPoints(int amount)
    {
        totalPoints -= amount;
        madeMistake = true;

        // Optional: Keep score from dropping below zero if desired
        if (totalPoints < 0)
        {
            totalPoints = 0;
        }

        UpdateUI();
    }

    /// <summary>
    /// Call this when a stage is finished.
    /// </summary>
    public void CompleteStage()
    {
        stagesCleared++;
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"POINTS: {totalPoints}";
        }
    }
}