using System.Collections.Generic;
using UnityEngine;

public class FallingCubeManager : MonoBehaviour
{

    public GameObject FallingObject;
    public static FallingCubeManager Instance;
    float initial_X = -3.987467f;
    [SerializeField] GameObject Ceiling;
    float generation_Y;
    public float spawnRate;
    float spawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        generation_Y = Ceiling.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        generation_Y = Ceiling.transform.position.y;
        /*spawnTimer += Time.deltaTime;
        if (spawnTimer > 1 / spawnRate)
        {
            GameObject falling = GameObject.Instantiate(FallingObject, transform);
            int i = Random.Range(0, 8);
            falling.transform.position = new Vector3(initial_X + 0.5f + i, generation_Y + 1.5f, 0);
            spawnTimer = 0;
        }*/
    }

    public void punish()
    {
        List<int> sourceNumbers = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7 };
        List<int> selectedNumbers = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, sourceNumbers.Count); 
            selectedNumbers.Add(sourceNumbers[index]);
            sourceNumbers.RemoveAt(index);
        }

        foreach (int i in selectedNumbers)
        {
            GameObject falling = GameObject.Instantiate(FallingObject, transform);
            falling.transform.position = new Vector3(initial_X + 0.5f + i, generation_Y + 1.5f, 0);
        }
    }
}
