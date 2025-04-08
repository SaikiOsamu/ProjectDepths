using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class CubeManager : MonoBehaviour
{
    [SerializeField] GameObject Player;
    [SerializeField] private GameObject BottomLine;
    public static CubeManager Instance;
    public GameObject BreakableObject;
    public GameObject UnbreakableObject;
    float spawnRate;
    float spawnTimer;

    public int columns = 8;
    public float startX = -7.359347f;
    public float cellWidth = 1f;
    public float initialRowY = 3.5f;
    public float rowHeight = 1f;

    float current_Last_Position;
    float current_Player_Position;
    float current_BottomLine_Position;

    int currentGapColumn;
    int direction = 0;
    int newGapColumn;

    [SerializeField] GameObject AirWallPrefab;  // Your new air wall prefab

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGapColumn = Random.Range(0, columns);
        current_Last_Position = SpawnSixRows(initialRowY, rowHeight);
    }

    float SpawnSixRows(float initialRowY, float height)
    {
        float rowY = initialRowY;
        for (int j = 0; j < 6; j++)
        {
            if (j > 0)
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
            }

            rowY = initialRowY - j * rowHeight;
            SpawnNextRow(rowY, currentGapColumn, direction);
        }
        return rowY;
    }

    void SpawnRow(float height)
    {
        // Replace wall left with air wall
        GameObject wallLeft = Instantiate(AirWallPrefab, transform);
        wallLeft.transform.localPosition = new Vector3(startX - 1 * cellWidth, height, 0);

        for (int i = 0; i < columns; i++)
        {
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

        // Replace wall right with air wall
        GameObject wallRight = Instantiate(AirWallPrefab, transform);
        wallRight.transform.localPosition = new Vector3(startX + columns * cellWidth, height, 0);
    }

    void SpawnNextRow(float height, int currentGapColumn, int direction)
    {
        // Left edge - Air Wall
        GameObject wallLeft = Instantiate(AirWallPrefab, transform);
        wallLeft.transform.localPosition = new Vector3(startX - 1 * cellWidth, height, 0);

        // Middle columns - game objects
        for (int i = 0; i < columns; i++)
        {
            GameObject cube = null;
            if (i == currentGapColumn || i == currentGapColumn + direction)
            {
                cube = Instantiate(BreakableObject, transform);
            }
            else
            {
                if (Random.value < 0.2f)
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

        // Right edge - Air Wall
        GameObject wallRight = Instantiate(AirWallPrefab, transform);
        wallRight.transform.localPosition = new Vector3(startX + columns * cellWidth, height, 0);
    }

    int GetNextGap(int currentGap)
    {
        int left = Mathf.Clamp(currentGap - 1, 0, columns - 1);
        int right = Mathf.Clamp(currentGap + 1, 0, columns - 1);
        int[] candidates = new int[] { left,right };
        return candidates[Random.Range(0, candidates.Length)];
    }

    public float GetPlayerPosition()
    {
        return current_Player_Position;
    }

    // Update is called once per frame
    void Update()
    {
        current_Player_Position = Player.transform.position.y;
        current_BottomLine_Position = BottomLine.transform.position.y;
        if(current_Player_Position - current_Last_Position < 6)
        {
            current_Last_Position = SpawnSixRows(current_Last_Position - 1, rowHeight);
        }
        if(current_Last_Position - current_BottomLine_Position < 6)
        {
            BottomLine.transform.position += Vector3.down * 6;
        }
        /*
        spawnRate = GameManager.instance.moveSpeed;
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
        */
    }
}
