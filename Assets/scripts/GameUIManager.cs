using System.Collections;
using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text objectiveText;
    public TMP_Text scoreText;
    public TMP_Text pointChangeText;
    public TMP_Text monologueText;
    public TMP_Text narrationText;
    public TMP_Text warningText;
    public TMP_Text timerText;

    [Header("Score Settings")]
    public int startingScore = 10;

    [Header("Monologue Settings")]
    public float typingSpeed = 0.04f;
    public float readingTime = 4f;
    public float fadeDuration = 0.5f;

    private int currentScore;

    private Coroutine monologueCoroutine;
    private Coroutine pointCoroutine;

    private void Start()
    {
        // Set starting score
        currentScore = startingScore;
        UpdateScore();

        // Hide UI elements that shouldn't appear immediately
        if (pointChangeText != null)
            pointChangeText.gameObject.SetActive(false);

        if (monologueText != null)
            monologueText.gameObject.SetActive(false);

        if (narrationText != null)
            narrationText.gameObject.SetActive(false);

        if (warningText != null)
            warningText.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);
    }

    // =========================
    // SCORE
    // =========================

    public void AddPoints(int amount)
    {
        currentScore += amount;
        UpdateScore();

        ShowPointChange("+" + amount);
    }

    public void RemovePoints(int amount)
    {
        currentScore -= amount;

        // Prevent score from going below zero
        if (currentScore < 0)
            currentScore = 0;

        UpdateScore();

        ShowPointChange("-" + amount);
    }

    private void UpdateScore()
    {
        if (scoreText != null)
        {
            scoreText.text = "POINTS: " + currentScore;
        }
    }

    private void ShowPointChange(string message)
    {
        if (pointCoroutine != null)
            StopCoroutine(pointCoroutine);

        pointCoroutine = StartCoroutine(PointChangeRoutine(message));
    }

    private IEnumerator PointChangeRoutine(string message)
    {
        pointChangeText.gameObject.SetActive(true);

        pointChangeText.text = message;

        // Start fully visible
        Color colour = pointChangeText.color;
        colour.a = 1f;
        pointChangeText.color = colour;

        // Wait briefly
        yield return new WaitForSeconds(1f);

        // Fade out
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, time / fadeDuration);

            colour.a = alpha;
            pointChangeText.color = colour;

            yield return null;
        }

        pointChangeText.gameObject.SetActive(false);
    }

    // =========================
    // MONOLOGUE
    // =========================

    public void ShowMonologue(string message, string nextObjective)
    {
        if (monologueCoroutine != null)
            StopCoroutine(monologueCoroutine);

        monologueCoroutine = StartCoroutine(
            MonologueRoutine(message, nextObjective)
        );
    }

    private IEnumerator MonologueRoutine(
        string message,
        string nextObjective
    )
    {
        // Hide objective while character is thinking
        objectiveText.gameObject.SetActive(false);

        // Show monologue
        monologueText.gameObject.SetActive(true);

        // Start empty
        monologueText.text = "";

        // Typewriter effect
        foreach (char letter in message)
        {
            monologueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }

        // Allow player to read
        float timer = 0f;

        while (timer < readingTime)
        {
            timer += Time.deltaTime;

            // Left mouse click skips the waiting period
            if (Input.GetMouseButtonDown(0))
            {
                break;
            }

            yield return null;
        }

        // Fade out
        yield return StartCoroutine(FadeText(
            monologueText,
            1f,
            0f
        ));

        monologueText.gameObject.SetActive(false);

        // Show next objective
        ShowObjective(nextObjective);
    }

    // =========================
    // OBJECTIVE
    // =========================

    public void ShowObjective(string message)
    {
        objectiveText.gameObject.SetActive(true);

        objectiveText.text = "OBJECTIVE\n" + message;

        // Make sure it is visible
        Color colour = objectiveText.color;
        colour.a = 1f;
        objectiveText.color = colour;
    }

    // =========================
    // NARRATION
    // =========================

    public void ShowNarration(string message)
    {
        StartCoroutine(NarrationRoutine(message));
    }

    private IEnumerator NarrationRoutine(string message)
    {
        narrationText.gameObject.SetActive(true);

        narrationText.text = message;

        yield return new WaitForSeconds(4f);

        yield return StartCoroutine(FadeText(
            narrationText,
            1f,
            0f
        ));

        narrationText.gameObject.SetActive(false);
    }

    // =========================
    // WARNING
    // =========================

    public void ShowWarning(string message)
    {
        StartCoroutine(WarningRoutine(message));
    }

    private IEnumerator WarningRoutine(string message)
    {
        warningText.gameObject.SetActive(true);

        warningText.text = message;

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(FadeText(
            warningText,
            1f,
            0f
        ));

        warningText.gameObject.SetActive(false);
    }

    // =========================
    // FADE
    // =========================

    private IEnumerator FadeText(
        TMP_Text text,
        float startAlpha,
        float endAlpha
    )
    {
        float time = 0f;

        Color colour = text.color;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                time / fadeDuration
            );

            colour.a = alpha;
            text.color = colour;

            yield return null;
        }

        colour.a = endAlpha;
        text.color = colour;
    }
}