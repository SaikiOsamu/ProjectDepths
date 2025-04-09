using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChangeSceneOnTimer : MonoBehaviour
{
    public float changeTime;
    public string sceneToChangeTo;
    AudioManager audioManager;

    // Update is called once per frame
    private void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
            return; // Add this line to avoid null reference exception
        }

        Debug.Log("Attempting to play BGM_MainMenu");
        audioManager.PlaySound("BGM_MainMenu");
        Debug.Log("After attempting to play BGM_MainMenu");

        // Add these lines to check if the sound exists and is playing
        bool isPlaying = audioManager.IsSoundPlaying("BGM_MainMenu");
        Debug.Log("Is BGM_MainMenu playing? " + isPlaying);
    }
    void Update()
    {
        // Check if any key is pressed
        if (Input.anyKeyDown)
        {
            // Load the specified scene immediately when any key is pressed
            SceneManager.LoadScene(sceneToChangeTo);
        }

        // Original timer-based functionality
        changeTime -= Time.deltaTime;
        if (changeTime <= 0)
        {
            // Load the specified scene
            SceneManager.LoadScene(sceneToChangeTo);
        }
    }
}