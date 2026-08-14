using UnityEngine;
using TMPro;

public class GameSummary : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject summaryPanel; // The UI Panel containing your summary UI
    [SerializeField] private TextMeshProUGUI summaryText;

    [Header("Game Progress Data")]
    public int stagesCleared = 0;
    public int totalStages = 3;

    [Header("Actions Completed")]
    public bool helpedGirlScared = false;
    public bool checkedOnHer = false;
    public bool reportedUnsafeGuy = false;

    private void Start()
    {
        // Hide the summary panel when the game starts
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Call this when a stage is cleared. If it's the final stage, it triggers the pop-up.
    /// </summary>
    public void CompleteStage()
    {
        stagesCleared++;

        // Check if all stages are finished
        if (stagesCleared >= totalStages)
        {
            EndGameAndShowSummary();
        }
    }

    /// <summary>
    /// Displays the text and pops up the summary panel.
    /// </summary>
    public void EndGameAndShowSummary()
    {
        BuildSummaryText();

        // Show the UI panel
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(true);
        }
    }

    private void BuildSummaryText()
    {
        // Pull score directly from ScoreManager if it exists
        int finalScore = 0;
        if (ScoreManager.Instance != null)
        {
            finalScore = ScoreManager.Instance.totalPoints;
        }

        string summary = "<b>--- MISSION SUMMARY ---</b>\n\n";
        summary += $"<b>Total Points Earned:</b> {finalScore} PTS\n";
        summary += $"<b>Stages Completed:</b> {stagesCleared} / {totalStages}\n\n";
        summary += "<b>What You Did Well:</b>\n";

        int goodActionsCount = 0;

        if (helpedGirlScared)
        {
            summary += "• Acted quickly when noticing someone feeling unsafe (+20 PTS)\n";
            goodActionsCount++;
        }
        if (checkedOnHer)
        {
            summary += "• Checked in directly on a peer in need (+20 PTS)\n";
            goodActionsCount++;
        }
        if (reportedUnsafeGuy)
        {
            summary += "• Alerted authorities/bystanders to safe navigation (+10 PTS)\n";
            goodActionsCount++;
        }

        if (goodActionsCount == 0)
        {
            summary += "• Keep practicing! Try helping peers and staying alert next run.\n";
        }

        if (summaryText != null)
        {
            summaryText.text = summary;
        }
    }
}