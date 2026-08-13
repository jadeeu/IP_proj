using System;
using System.Collections;
using UnityEngine;

public class IntersectionLightController : MonoBehaviour
{
    public enum LightState { A, B }

    [SerializeField] private float delayBeforeGreen = 3f;   // wait after button press
    [SerializeField] private float greenDuration = 10f;     // how long cars are stopped
    [SerializeField] private float cooldownAfterGreen = 5f; // ignore presses briefly after cycle

    // Start in State A = green for cars, red for pedestrians
    public LightState CurrentState { get; private set; } = LightState.A;
    public event Action<LightState> OnLightChanged;

    private bool isCycleRunning = false;

    public void RequestCrossing()
    {
        if (isCycleRunning) return; // ignore spam presses
        StartCoroutine(RunRequestedCycle());
    }

    private IEnumerator RunRequestedCycle()
    {
        isCycleRunning = true;

        // Wait 3 seconds before turning red for cars
        yield return new WaitForSecondsRealtime(delayBeforeGreen);

        // Cars stop (State B)
        SetState(LightState.B);
        yield return new WaitForSecondsRealtime(greenDuration);

        // Cars go again (State A)
        SetState(LightState.A);
        yield return new WaitForSecondsRealtime(cooldownAfterGreen);

        isCycleRunning = false;
    }

    private void SetState(LightState state)
    {
        CurrentState = state;
        OnLightChanged?.Invoke(state);
    }
}