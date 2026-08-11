using UnityEngine;

public class ShopCCTV : MonoBehaviour
{
    [Header("Distance Settings")]
    public Transform playerTransform;
    public float interactDistance = 2.5f;

    [Header("UI Display")]
    public GameObject interactPromptUI;
    public GameObject cctvScreenPanels;

    [Header("Cameras")]
    public GameObject mainPlayerCamera; // Will auto-find if left empty!
    public GameObject tvZoomCamera;

    private bool isZoomedIn = false;

    void Start()
    {
        // Auto-find PlayerFollowCamera by name if not dragged in
        if (mainPlayerCamera == null)
        {
            mainPlayerCamera = GameObject.Find("PlayerFollowCamera");
        }

        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (cctvScreenPanels != null) cctvScreenPanels.SetActive(false);
        if (tvZoomCamera != null) tvZoomCamera.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool isCloseEnough = distance <= interactDistance;

        if (isCloseEnough && !isZoomedIn)
        {
            if (interactPromptUI != null && !interactPromptUI.activeSelf)
                interactPromptUI.SetActive(true);
        }
        else if (!isCloseEnough && !isZoomedIn)
        {
            if (interactPromptUI != null && interactPromptUI.activeSelf)
                interactPromptUI.SetActive(false);
        }

        if (!isCloseEnough && isZoomedIn)
        {
            ToggleZoom();
        }

        if (isCloseEnough && Input.GetKeyDown(KeyCode.E))
        {
            ToggleZoom();
        }
    }

    void ToggleZoom()
    {
        isZoomedIn = !isZoomedIn;

        if (tvZoomCamera != null) tvZoomCamera.SetActive(isZoomedIn);
        if (mainPlayerCamera != null) mainPlayerCamera.SetActive(!isZoomedIn);

        if (cctvScreenPanels != null) cctvScreenPanels.SetActive(isZoomedIn);
        if (interactPromptUI != null) interactPromptUI.SetActive(!isZoomedIn);
    }
}