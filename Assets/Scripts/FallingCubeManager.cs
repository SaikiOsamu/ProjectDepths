using UnityEngine;

public class FallingCube : MonoBehaviour
{

    public GameObject FallingObject;

    float initial_X = -0.6870681f;
    float initial_Y = 9.5f;
    public float spawnRate;
    float spawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > 1 / spawnRate)
        {
            GameObject falling = GameObject.Instantiate(FallingObject, transform);
            int i = Random.Range(0, 8);
            falling.transform.localPosition = new Vector2(initial_X + i, initial_Y);
            spawnTimer = 0;
        }
    }
}
