using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Sound Effects")]
    [SerializeField] string hoverOverSound = "ButtonHover";
    [SerializeField] string clickButtonSound = "ButtonClick";
    [SerializeField] string gameOverSound = "BGM_PauseMenu";

    [Header("Audio Settings")]
    [SerializeField] private string playSceneBGM = "BGM_PlayScene";
    [SerializeField] private string pauseMenuBGM = "BGM_PauseMenu";
    [SerializeField] private float audioFadeDuration = 0.15f;

    AudioManager audioManager;

    private bool isGameOver = false;

    private void Start()
    {
        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }

        // Ensure the game over panel is hidden initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    // Call this method when the player dies to display the game over screen
    public void ShowGameOver()
    {
        if (isGameOver) return; // Prevent multiple activations
        isGameOver = true;

        // Use smooth audio transition instead of abrupt stop/play
        SetAudioState(true);

        // Get score from GameManager
        int finalScore = GameManager.instance.current_score;

        // Get time from the GameManager
        float gameTime = GameManager.instance.timer;

        // Calculate minutes and seconds from game time
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);

        // Update UI elements
        if (finalScoreText != null)
        {
            finalScoreText.text = finalScore.ToString("D5");
        }

        if (timeText != null)
        {
            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // Show the game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Freeze the game
        Time.timeScale = 0f;

        Debug.Log("Game Over. Final Score: " + finalScore + ", Time Survived: " + string.Format("{0:00}:{1:00}", minutes, seconds));
    }

    private void SetAudioState(bool isGameOver)
    {
        if (audioManager == null) return;

        // We'll use a coroutine for smooth transitions
        StartCoroutine(TransitionAudio(isGameOver));
    }

    private IEnumerator TransitionAudio(bool isGameOver)
    {
        // Find audio sources by searching for all sources under the AudioManager
        for (int i = 0; i < audioManager.transform.childCount; i++)
        {
            Transform child = audioManager.transform.GetChild(i);
            string name = child.name;

            if (name.Contains(playSceneBGM))
            {
                AudioSource source = child.GetComponent<AudioSource>();
                if (source != null)
                {
                    if (isGameOver)
                    {
                        // Fade out play scene BGM
                        float startVolume = source.volume;
                        float elapsed = 0f;

                        while (elapsed < audioFadeDuration)
                        {
                            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / audioFadeDuration);
                            elapsed += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        source.volume = 0f;
                    }
                }
            }

            if (name.Contains(pauseMenuBGM))
            {
                AudioSource source = child.GetComponent<AudioSource>();
                if (source != null)
                {
                    if (isGameOver)
                    {
                        // Fade in pause menu BGM
                        float startVolume = source.volume;
                        float elapsed = 0f;

                        while (elapsed < audioFadeDuration)
                        {
                            source.volume = Mathf.Lerp(startVolume, 1f, elapsed / audioFadeDuration);
                            elapsed += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        source.volume = 1f; // Match inspector value
                    }
                }
            }
        }
    }

    // Restart the current game scene
    public void RestartGame()
    {
        // Play click sound
        audioManager.PlaySound(clickButtonSound);

        // Reset time scale
        Time.timeScale = 1f;
        isGameOver = false;

        // Use a coroutine to add a slight delay for the sound to play
        StartCoroutine(LoadSceneWithDelay(gameSceneName));

        Debug.Log("Restarting game");
    }

    // Return to the main menu
    public void ReturnToMainMenu()
    {
        // Play click sound
        audioManager.PlaySound(clickButtonSound);

        // Reset time scale
        Time.timeScale = 1f;
        isGameOver = false;

        // Use a coroutine to add a slight delay for the sound to play
        StartCoroutine(LoadSceneWithDelay(mainMenuSceneName));

        Debug.Log("Returning to main menu");
    }

    // Coroutine to add a small delay between sound and scene change
    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Wait a small amount of time for the click sound to play
        yield return new WaitForSecondsRealtime(0.2f);
        audioManager.PlaySound("BGM_MainMenu");

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    // Play sound when mouse hovering UI
    public void OnMouseOver()
    {
        audioManager.PlaySound(hoverOverSound);
    }

    // This ensures time scale is reset if the script is disabled or destroyed
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}