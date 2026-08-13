using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConfrontationUIManager : MonoBehaviour
{
    [Header("Popup UI Image Panels")]
    [Tooltip("Drag your 'Thief Caught' Image GameObject here")]
    public GameObject thiefCaughtPopupImage;

    [Tooltip("Drag your 'Wrong Shopper' Image GameObject here")]
    public GameObject wrongShopperPopupImage;

    private bool isGameOver = false;

    void Start()
    {
        // Hide both popup images at the start of the game
        if (thiefCaughtPopupImage != null) thiefCaughtPopupImage.SetActive(false);
        if (wrongShopperPopupImage != null) wrongShopperPopupImage.SetActive(false);

        // Make sure time runs normally when scene loads
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Listen for 'R' keypress after a popup appears
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void ShowThiefCaughtPopup()
    {
        isGameOver = true;
        if (thiefCaughtPopupImage != null) 
            thiefCaughtPopupImage.SetActive(true); // Shows the image

        PauseGame();
    }

    public void ShowWrongShopperPopup()
    {
        isGameOver = true;
        if (wrongShopperPopupImage != null) 
            wrongShopperPopupImage.SetActive(true); // Shows the image

        PauseGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Pauses AI, physics, and movement
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Restore game speed before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}