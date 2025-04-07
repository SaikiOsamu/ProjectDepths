using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
   
    // Sound
    [SerializeField] string hoverOverSound = "ButtonHover";
    [SerializeField] string clickButtonSound = "ButtonClick";

    AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }
    }

    // Play sound when mouse hovering UI 
    public void OnMouseOver()
    {
        audioManager.PlaySound(hoverOverSound);
    }
    // This method can be directly called from the button's OnClick event
    public void BackToMainMenu()
    {
        Debug.Log("Returning to main menu from credits");
        
        // Play Click sound
        audioManager.PlaySound(clickButtonSound);

        StartCoroutine(LoadSceneWithDelay(mainMenuSceneName));
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Wait a small amount of time for the click sound to play
        yield return new WaitForSecondsRealtime(0.8f);

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

}