using UnityEngine;

public class TrafficLightVisual : MonoBehaviour
{
    public IntersectionLightController controller;

    [Header("Light versions")]
    public GameObject redLightObject;
    public GameObject greenLightObject;

    void OnEnable()
    {
        if (controller != null)
        {
            controller.OnLightChanged += HandleLightChanged;
            HandleLightChanged(controller.CurrentState);
        }
    }

    void OnDisable()
    {
        if (controller != null)
            controller.OnLightChanged -= HandleLightChanged;
    }

    void HandleLightChanged(IntersectionLightController.LightState state)
    {
        // State A = pedestrian green (walk), State B = pedestrian red (stop)
        bool pedestriansCanWalk = state == IntersectionLightController.LightState.A;

        greenLightObject.SetActive(pedestriansCanWalk);
        redLightObject.SetActive(!pedestriansCanWalk);
    }
}