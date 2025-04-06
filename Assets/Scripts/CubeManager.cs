using UnityEngine;

public class CubeManager : MonoBehaviour
{

    public float spawnRate;
    float spawnTimer;

    public GameObject[] cubePool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int j = 0; j < 6; j++)
        {
            int r;
            for (int i = 0; i < 8; i++)
            {
                r = Random.Range(0, cubePool.Length);
                GameObject cube = Instantiate(cubePool[r], transform);
                cube.transform.localPosition = new Vector3(-7.359347f + i, -0.5f - j, 0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if(spawnTimer > 1 / spawnRate)
        {
            SpawnRow(-5.5f);
            spawnTimer = 0;
        }


    }

    void SpawnRow(float height)
    {
        int r;
        for(int i = 0; i < 8; i++)
        {
            r = Random.Range(0, cubePool.Length);
            GameObject cube = Instantiate(cubePool[r], transform);
            cube.transform.localPosition = new Vector3(-7.359347f + i, height, 0);
        }
    }
}
