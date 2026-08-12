using UnityEngine;

public class PopupZone : MonoBehaviour
{
    [Header("Which popup this plane shows")]
    public GameObject popupPanel;      // drag the panel for THIS plane

    [Header("Behaviour")]
    public bool pauseGame = false;     // freeze while popup is up?
    public bool hideScore = true;      // hide the score while popup is up?
    public bool showOnce = false;      // only trigger the first time?

    private bool alreadyShown = false;
    private bool isOpen = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (showOnce && alreadyShown) return;

        OpenPopup();
    }

    void Update()
    {
        // Close on G while this popup is open
        if (isOpen && Input.GetKeyDown(KeyCode.G))
        {
            ClosePopup();
        }
    }

    void OpenPopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        if (hideScore && GameManager.Instance != null)
            GameManager.Instance.SetScoreVisible(false);

        if (pauseGame)
            Time.timeScale = 0f;

        isOpen = true;
        alreadyShown = true;
    }

    void ClosePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        if (hideScore && GameManager.Instance != null)
            GameManager.Instance.SetScoreVisible(true);

        if (pauseGame)
            Time.timeScale = 1f;

        isOpen = false;
    }
}