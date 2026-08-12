using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;   // needed for TextMeshProUGUI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Death UI")]
    public GameObject deathScreen;

    [Header("Score")]
    public int score = 10;
    public TextMeshProUGUI scoreText;   // drag ScoreText here

    private bool isDead = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateScoreDisplay();
    }

    void Update()
    {
        if (isDead && Input.GetKeyDown(KeyCode.R))
        {
            TryAgain();
        }
    }

    public void AddPoints(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
    }

    void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void CarHit()
    {
        if (isDead) return;
        isDead = true;

        if (deathScreen != null)
            deathScreen.SetActive(true);

        SetScoreVisible(false);   // hide score while the popup is up

        Time.timeScale = 0f;
    }

    // Call this from anything that opens/closes a popup
    public void SetScoreVisible(bool visible)
    {
        if (scoreText != null)
            scoreText.gameObject.SetActive(visible);
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}