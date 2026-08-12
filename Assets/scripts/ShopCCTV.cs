using UnityEngine;
using TMPro;

public class ShopCCTV : MonoBehaviour
{
    [Header("Camera Setup")]
    [Tooltip("Drag all 4 CCTV Cameras here so they stream live to Render Textures.")]
    public Camera[] cctvCameras;

    [Header("UI Panels")]
    [Tooltip("Drag 'CCTVGridPanel' (the parent of your 4 RawImages) here.")]
    public GameObject cctvGridPanel;

    [Tooltip("Drag 'PressEcctv' (the Press E prompt panel) here.")]
    public GameObject interactPromptUI;

    [Tooltip("Drag the TextMeshPro text inside PressEcctv here.")]
    public TMP_Text promptText;

    [Header("Player Control (Optional)")]
    [Tooltip("Drag your player movement script here to freeze movement while viewing.")]
    public MonoBehaviour playerMovementScript;

    private bool isPlayerInRadius = false;
    private bool isViewingCCTV = false;

    private void Start()
    {
        // Hide the UI panels when game starts
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (cctvGridPanel != null) cctvGridPanel.SetActive(false);

        // Keep CCTV cameras active so they feed live images to the textures
        EnableAllCCTVs();
    }

    private void Update()
    {
        if (isPlayerInRadius)
        {
            // Toggle CCTV view when pressing E
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleCCTVMode();
            }
        }
        else if (isViewingCCTV && Input.GetKeyDown(KeyCode.E))
        {
            // Exit if E is pressed while looking
            ToggleCCTVMode();
        }
    }

    private void ToggleCCTVMode()
    {
        isViewingCCTV = !isViewingCCTV;

        // Show/Hide the 4-screen CCTV grid
        if (cctvGridPanel != null)
        {
            cctvGridPanel.SetActive(isViewingCCTV);
        }

        // Freeze/Unfreeze player movement while looking at screens
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = !isViewingCCTV;
        }

        // Update prompt text
        if (promptText != null)
        {
            promptText.text = isViewingCCTV ? "Press [E] to Exit CCTV" : "Press [E] to Access CCTV";
        }
    }

    private void EnableAllCCTVs()
    {
        if (cctvCameras == null) return;
        foreach (Camera cam in cctvCameras)
        {
            if (cam != null) cam.gameObject.SetActive(true);
        }
    }

    // ---------------------------------------------------------------
    // Trigger Radius Logic
    // ---------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            isPlayerInRadius = true;

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(true);
            }

            if (promptText != null)
            {
                promptText.text = "Press [E] to Access CCTV";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
        {
            isPlayerInRadius = false;

            // Hide everything if player walks away
            if (isViewingCCTV)
            {
                ToggleCCTVMode();
            }

            if (interactPromptUI != null)
            {
                interactPromptUI.SetActive(false);
            }
        }
    }
}