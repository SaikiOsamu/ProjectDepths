using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Playground"; // Name of your gameplay scene
    [SerializeField] private string creditsSceneName = "Credits"; // Name of your credits scene (if you have one)

    [Header("Animation Settings (Optional)")]
    [SerializeField] private float fadeTime = 1.0f;
    [SerializeField] private bool useTransitionEffect = false;

    [SerializeField]
    string hoverOverSound = "ButtonHover";

    AudioManager audioManager;
    // Called when the Start Game button is clicked


    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }
    }

    public void OnMouseOver()
    {
        audioManager.PlaySound(hoverOverSound);
    }


    public void StartGame()
    {
        Debug.Log("Starting game...");

        if (useTransitionEffect)
        {
            // You could add a transition effect here if desired
            StartCoroutine(LoadSceneWithDelay(gameSceneName, fadeTime));
        }
        else
        {
            // Load the game scene directly
            SceneManager.LoadScene(gameSceneName);
        }
    }

    // Called when the Credits button is clicked
    public void ShowCredits()
    {
        Debug.Log("Showing credits...");

        // Check if you have a separate credits scene
        if (!string.IsNullOrEmpty(creditsSceneName))
        {
            SceneManager.LoadScene(creditsSceneName);
        }
        else
        {
            // If no separate scene, you could show a credits panel in the main menu
            // Implement this if you have a UI panel for credits
            Debug.Log("No credits scene specified. Consider showing a credits panel instead.");
        }
    }

    // Called when the Quit Game button is clicked
    public void QuitGame()
    {
        Debug.Log("Quitting game...");

        // In the editor, this will stop play mode
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // In a build, this will quit the application
            Application.Quit();
#endif
    }

    // Coroutine for scene transition with delay
    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        // Wait for the specified delay time
        yield return new WaitForSeconds(delay);

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }
}