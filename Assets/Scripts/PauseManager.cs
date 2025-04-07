using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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

    // Sound
    [SerializeField] string hoverOverSound = "ButtonHover";
    [SerializeField] string clickButtonSound = "ButtonClick";
    [SerializeField] string pauseMenuBGM = "BGM_PauseMenu";

     AudioManager audioManager;

    private void Start()
    {
        
        // audio instance
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }

        // Ensure the pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set up button listeners
        /*if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }*/
    }

    // Play sound when mouse hovering UI 
    public void OnMouseOver()
    {
        audioManager.PlaySound(hoverOverSound);
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


            // Audio manager
            audioManager.PlaySound(pauseMenuBGM);
            audioManager.StopSound("BGM_PlayScene");
        }

        Debug.Log("Game paused");
    }

    public void ResumeGame()
    {

        // Play Click sound
        audioManager.PlaySound(clickButtonSound);
        audioManager.StopSound(pauseMenuBGM);

        // Set the time scale back to 1 to resume normal time flow
        Time.timeScale = 1f;
        isPaused = false;

        // Hide the pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);

            // Audio manager
            audioManager.PlaySound("BGM_PlayScene");
        }

        Debug.Log("Game resumed");
    }

    public void ReturnToMainMenu()
    {

        // Play Click sound
        audioManager.PlaySound(clickButtonSound);

        // Make sure to reset time scale before changing scenes
        Time.timeScale = 1f;
        isPaused = false;

        // Load the main menu scene
        StartCoroutine(LoadSceneWithDelay(mainMenuSceneName));
        Debug.Log("Returning to main menu");
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Wait a small amount of time for the click sound to play
        yield return new WaitForSecondsRealtime(0.8f);

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    // This ensures time scale is reset if the script is disabled or destroyed
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}