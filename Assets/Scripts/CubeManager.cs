using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class CubeManager : MonoBehaviour
{
    public GameObject BreakableObject;
    public GameObject UnbreakableObject;
    public float spawnRate;
    float spawnTimer;

    public int columns = 8;
    public float startX = -7.359347f;
    public float cellWidth = 1f;
    public float initialRowY = -0.5f;
    public float rowHeight = 1f;

    int currentGapColumn;
    int direction = 0;
    int newGapColumn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGapColumn = Random.Range(0, columns);

        for (int j = 0; j < 6; j++)
        {
            if (j > 0)
            {
                newGapColumn = GetNextGap(currentGapColumn);
                if(newGapColumn > currentGapColumn)
                {
                    direction = -1;
                }
                else if(newGapColumn < currentGapColumn)
                {
                    direction = 1;
                }
                else
                {
                    direction = 0;
                }
                currentGapColumn = newGapColumn;
            }

            float rowY = initialRowY - j * rowHeight;
            SpawnRow(rowY);
            //SpawnNextRow(rowY, currentGapColumn, direction);
        }
    }

    void SpawnRow(float height)
    {
        for(int i  = 0; i < columns; i++) {
            GameObject cube = null;
            if (Random.value < 0.175f)
                {
                    cube = Instantiate(UnbreakableObject, transform);
                }
            else
                {
                    cube = Instantiate(BreakableObject, transform);
                }
            cube.transform.localPosition = new Vector3(startX + i * cellWidth, height, 0);
        }
    }

    void SpawnNextRow(float height, int currentGapColumn, int direction)
    {
        for (int i = 0; i < columns; i++)
        {
            GameObject cube = null;
            if (i == currentGapColumn || i == currentGapColumn + direction)
            {
                cube = Instantiate(BreakableObject, transform);
            }
            else
            {
                if (Random.value < 0.5f)
                {
                    cube = Instantiate(UnbreakableObject, transform);
                }
                else
                {
                    cube = Instantiate(BreakableObject, transform);
                }
            }
            cube.transform.localPosition = new Vector3(startX + i * cellWidth, height, 0);
        }
    }

    int GetNextGap(int currentGap)
    {
        int left = Mathf.Clamp(currentGap - 1, 0, columns - 1);
        int right = Mathf.Clamp(currentGap + 1, 0, columns - 1);
        int[] candidates = new int[] { left,right };
        return candidates[Random.Range(0, candidates.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > 1 / spawnRate)
        {
            newGapColumn = GetNextGap(currentGapColumn);
            if (newGapColumn > currentGapColumn)
            {
                direction = -1;
            }
            else if (newGapColumn < currentGapColumn)
            {
                direction = 1;
            }
            else
            {
                direction = 0;
            }
            currentGapColumn = newGapColumn;
            SpawnRow(-5.5f);
            //SpawnNextRow(-5.5f, currentGapColumn, direction);
            spawnTimer = 0;
        }


    }
}
