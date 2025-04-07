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
        changeTime -= Time.deltaTime;
        if (changeTime <= 0)
        {
            // Load the specified scene
            SceneManager.LoadScene(sceneToChangeTo);
        }
    }
}
