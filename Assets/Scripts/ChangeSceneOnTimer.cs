using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ChangeSceneOnTimer : MonoBehaviour
{
    public float changeTime;
    public string sceneToChangeTo;

    // Update is called once per frame
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