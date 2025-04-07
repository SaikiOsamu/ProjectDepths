using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        SceneManager.LoadScene(mainMenuSceneName);
    }

}