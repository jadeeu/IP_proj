using System;
using System.Collections;
using UnityEngine;

public class IntersectionLightController : MonoBehaviour
{
    public enum LightState { A, B }

    [SerializeField] private float cycleLength = 120f;   // total cycle: 2 minutes
    [SerializeField] private float pauseDuration = 10f;  // time spent in B

    public LightState CurrentState { get; private set; } = LightState.A;
    public event Action<LightState> OnLightChanged;

    private void Start() => StartCoroutine(RunCycle());

    private IEnumerator RunCycle()
    {
        while (true)
        {
            SetState(LightState.A);
            yield return new WaitForSeconds(cycleLength - pauseDuration); // 110s

            SetState(LightState.B);
            yield return new WaitForSeconds(pauseDuration); // 10s
        }
    }

    private void SetState(LightState state)
    {
        CurrentState = state;
        OnLightChanged?.Invoke(state);
    }
}