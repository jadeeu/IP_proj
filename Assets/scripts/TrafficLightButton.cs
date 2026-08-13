using UnityEngine;
using TMPro;

public class TrafficLightButton : MonoBehaviour
{
    public IntersectionLightController controller;
    public float interactRange = 2.5f;
    public Transform player;
    public GameObject pressEHint;   // drag a "Press E" text/UI here (child of button)

    void Update()
    {
        if (player == null || controller == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactRange;

        if (pressEHint != null) pressEHint.SetActive(inRange);

        if (inRange && Input.GetKeyDown(KeyCode.E))
        {
            controller.RequestCrossing();
        }
    }
}