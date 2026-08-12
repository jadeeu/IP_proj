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
        // =========================
        // STARTING SCORE
        // =========================

        currentScore = startingScore;
        UpdateScore();


        // =========================
        // HIDE UI ELEMENTS
        // =========================

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


        // =========================
        // STARTING MONOLOGUE
        // =========================

        ShowMonologue(
            "I should get going. My shift starts soon.",
            "Follow the arrows towards the lift."
        );
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

        pointCoroutine = StartCoroutine(
            PointChangeRoutine(message)
        );
    }


    private IEnumerator PointChangeRoutine(string message)
    {
        if (pointChangeText == null)
            yield break;

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

            float alpha = Mathf.Lerp(
                1f,
                0f,
                time / fadeDuration
            );

            colour.a = alpha;
            pointChangeText.color = colour;

            yield return null;
        }


        pointChangeText.gameObject.SetActive(false);
    }


    // =========================
    // MONOLOGUE
    // =========================

    public void ShowMonologue(
        string message,
        string nextObjective
    )
    {
        if (monologueCoroutine != null)
            StopCoroutine(monologueCoroutine);

        monologueCoroutine = StartCoroutine(
            MonologueRoutine(
                message,
                nextObjective
            )
        );
    }


    private IEnumerator MonologueRoutine(
        string message,
        string nextObjective
    )
    {
        // Hide objective while monologue is playing
        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);


        // Make sure monologue exists
        if (monologueText == null)
            yield break;


        // Show monologue
        monologueText.gameObject.SetActive(true);


        // Make sure text starts fully visible
        Color colour = monologueText.color;
        colour.a = 1f;
        monologueText.color = colour;


        // Clear previous text
        monologueText.text = "";


        // =========================
        // TYPEWRITER EFFECT
        // =========================

        foreach (char letter in message)
        {
            monologueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }


        // =========================
        // READING TIME
        // =========================

        yield return new WaitForSeconds(
            readingTime
        );


        // =========================
        // FADE OUT
        // =========================

        float fadeTimer = 0f;

        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;

            float alpha = Mathf.Lerp(
                1f,
                0f,
                fadeTimer / fadeDuration
            );

            colour = monologueText.color;
            colour.a = alpha;
            monologueText.color = colour;

            yield return null;
        }


        // Make absolutely sure it is invisible
        colour = monologueText.color;
        colour.a = 0f;
        monologueText.color = colour;


        // Hide monologue
        monologueText.gameObject.SetActive(false);


        // =========================
        // SHOW OBJECTIVE
        // =========================

        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(true);

            objectiveText.text =
                "OBJECTIVE\n" + nextObjective;


            // Make sure objective is fully visible
            colour = objectiveText.color;
            colour.a = 1f;
            objectiveText.color = colour;
        }
    }


    // =========================
    // OBJECTIVE
    // =========================

    public void ShowObjective(string message)
    {
        if (objectiveText == null)
            return;

        objectiveText.gameObject.SetActive(true);

        objectiveText.text =
            "OBJECTIVE\n" + message;


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
        StartCoroutine(
            NarrationRoutine(message)
        );
    }


    private IEnumerator NarrationRoutine(string message)
    {
        if (narrationText == null)
            yield break;

        narrationText.gameObject.SetActive(true);

        narrationText.text = message;


        yield return new WaitForSeconds(4f);


        yield return StartCoroutine(
            FadeText(
                narrationText,
                1f,
                0f
            )
        );


        narrationText.gameObject.SetActive(false);
    }


    // =========================
    // WARNING
    // =========================

    public void ShowWarning(string message)
    {
        StartCoroutine(
            WarningRoutine(message)
        );
    }


    private IEnumerator WarningRoutine(string message)
    {
        if (warningText == null)
            yield break;

        warningText.gameObject.SetActive(true);

        warningText.text = message;


        yield return new WaitForSeconds(2f);


        yield return StartCoroutine(
            FadeText(
                warningText,
                1f,
                0f
            )
        );


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
        if (text == null)
            yield break;

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