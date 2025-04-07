using UnityEngine;

public class FallingCubeManager : MonoBehaviour
{

    public GameObject FallingObject;

    float initial_X = -3.987467f;
    [SerializeField] GameObject Ceiling;
    float generation_Y;
    public float spawnRate;
    float spawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generation_Y = Ceiling.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        generation_Y = Ceiling.transform.position.y;
        spawnTimer += Time.deltaTime;
        if (spawnTimer > 1 / spawnRate)
        {
            GameObject falling = GameObject.Instantiate(FallingObject, transform);
            int i = Random.Range(0, 8);
            falling.transform.position = new Vector3(initial_X + 0.5f + i, generation_Y + 1.5f, 0);
            spawnTimer = 0;
        }
    }
}
