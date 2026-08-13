using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Death UI")]
    public GameObject deathScreen;

    [Header("Score")]
    public int score = 10;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateScoreDisplay();
        if (deathScreen != null) deathScreen.SetActive(false);
    }

    public void CarHit()
    {
        Debug.Log("Player was hit by a car!");
        ShowDeathScreen();
    }

    public void AddPoints(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
        Debug.Log($"Score: {score} ({(amount >= 0 ? "+" : "")}{amount})");
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void ShowDeathScreen()
    {
        if (deathScreen != null) deathScreen.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Called by the Try Again button
    public void TryAgain()
    {
        if (deathScreen != null) deathScreen.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reload the scene to reset everything
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void SetScoreVisible(bool visible)
    {
        if (scoreText != null)
            scoreText.gameObject.SetActive(visible);
    }

    void Update()
{
    if (deathScreen != null && deathScreen.activeSelf && Input.GetKeyDown(KeyCode.R))
    {
        Debug.Log("R pressed — calling TryAgain");
        TryAgain();
    }
}
}