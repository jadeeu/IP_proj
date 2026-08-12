using UnityEngine;

public class TVMonitorInteraction : MonoBehaviour
{
    [Header("UI Display")]
    public GameObject interactPromptUI;  // "Press E to interact" text
    public GameObject cctvScreenPanels;  // Parent object holding the 4 RawImages

    [Header("Cameras")]
    public Camera mainPlayerCamera;      // Your regular player Camera component
    public Camera tvZoomCamera;          // Your Zoom Camera pointing at the TV screen

    private bool isPlayerInTrigger = false;
    private bool isZoomedIn = false;

    void Start()
    {
        // Set correct initial states
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (cctvScreenPanels != null) cctvScreenPanels.SetActive(false);
        
        // Ensure TV zoom camera starts disabled
        if (tvZoomCamera != null) tvZoomCamera.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        isZoomedIn = !isZoomedIn;

        // 1. Switch Cameras by adjusting priority or turning GameObjects on/off
        if (tvZoomCamera != null) 
            tvZoomCamera.gameObject.SetActive(isZoomedIn);
            
        if (mainPlayerCamera != null) 
            mainPlayerCamera.gameObject.SetActive(!isZoomedIn);

        // 2. Toggle CCTV Canvas
        if (cctvScreenPanels != null) 
            cctvScreenPanels.SetActive(isZoomedIn);

        // 3. Hide prompt while viewing monitor
        if (interactPromptUI != null) 
            interactPromptUI.SetActive(!isZoomedIn);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            if (!isZoomedIn && interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);

            // Exit monitor view if player walks away
            if (isZoomedIn)
            {
                ToggleZoom();
            }
        }
    }
}