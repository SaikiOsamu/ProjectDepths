using UnityEngine;
using UnityEngine.UI;

public class ButtonMuteToggle : MonoBehaviour
{
    [SerializeField] private Button muteButton;
    [SerializeField] private GameObject targetObject;
    [SerializeField] private AudioManager audioManager;

    SpriteRenderer sr;

    private bool isMuted = false;
   

    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }

        sr = targetObject.GetComponent<SpriteRenderer>();
        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMute);
        }
    }

    void ToggleMute()
    {
        Debug.Log("Entered mute function");
        isMuted = !isMuted;

        if (isMuted)
        {
            audioManager.MuteAll();
        }
        else
        {
            audioManager.UnmuteAll();
        }

        Color color = sr.color;
        color.a = isMuted ? 145f / 255f : 0f;
        sr.color = color;
    }


}
