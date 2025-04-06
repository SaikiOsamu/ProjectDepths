using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    // This method can be directly called from the button's OnClick event
    public void BackToMainMenu()
    {
        Debug.Log("Returning to main menu from credits");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}