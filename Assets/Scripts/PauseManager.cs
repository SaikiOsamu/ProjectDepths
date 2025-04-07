using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Input Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set up button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    private void Update()
    {
        // Check for pause key press
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        // Set the time scale to 0 to freeze the game
        Time.timeScale = 0f;
        isPaused = true;

        // Show the pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        Debug.Log("Game paused");
    }

    public void ResumeGame()
    {
        // Set the time scale back to 1 to resume normal time flow
        Time.timeScale = 1f;
        isPaused = false;

        // Hide the pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        Debug.Log("Game resumed");
    }

    public void ReturnToMainMenu()
    {
        // Make sure to reset time scale before changing scenes
        Time.timeScale = 1f;
        isPaused = false;

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
        Debug.Log("Returning to main menu");
    }

    // This ensures time scale is reset if the script is disabled or destroyed
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}