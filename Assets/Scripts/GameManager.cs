using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public float timer = 0f;

    public static GameManager instance;
    [SerializeField] GameObject Ceiling;
    [SerializeField] private TextMeshProUGUI TimeValue;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int score; 

    public int current_score;

    [SerializeField] public float moveSpeed;
    [SerializeField] public float maximumCeilingSpeed;
    [SerializeField] public float extraSpeed;

    private void Awake()
    {
        current_score = 0;
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreText.text = "00000";
    }

    // Update is called once per frame
    void Update()
    {
        Ceiling.transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        timer += Time.deltaTime;

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        TimeValue.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        moveSpeed = Mathf.MoveTowards(moveSpeed, maximumCeilingSpeed, 0.01f * Time.deltaTime);


        scoreText.text = current_score.ToString("D5");
    }

    public void Punish()
    {
        float position_diff = Ceiling.transform.position.y - CubeManager.Instance.GetPlayerPosition();
        if (position_diff < 4)
        {
            FallingCubeManager.Instance.punish();
        }
        else
        {
            Ceiling.transform.Translate(Vector3.down * (position_diff - 4));
        }
    }

    public void GainScore()
    {
        current_score += score;
    }

    public void GainScore(int i)
    {
        current_score += i;
    }

    public int GetScore()
    {
        int tmp = current_score;
        current_score = 0;
        return tmp;
    }
}
