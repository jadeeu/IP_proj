using UnityEngine;

public class PlayerConfrontation : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactDistance = 4f;
    public LayerMask npcLayer; // Assign the NPC layer in the Inspector

    [Header("UI Reference")]
    public ConfrontationUIManager uiManager;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // Only trigger if C is pressed while the game is running (timeScale > 0)
        if (Input.GetKeyDown(KeyCode.C) && Time.timeScale > 0f)
        {
            CheckConfrontation();
        }
    }

    private void CheckConfrontation()
    {
        // Raycast from the center of the screen
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, npcLayer))
        {
            // Check if object hit is the Thief
            ThiefAI thief = hit.collider.GetComponentInParent<ThiefAI>();
            if (thief != null)
            {
                uiManager.ShowThiefCaughtPopup();
                return;
            }

            // Check if object hit is an Innocent Shopper
            ShopperAI shopper = hit.collider.GetComponentInParent<ShopperAI>();
            if (shopper != null)
            {
                uiManager.ShowWrongShopperPopup();
                return;
            }
        }
    }
}