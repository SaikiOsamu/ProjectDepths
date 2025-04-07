using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;
using TMPro;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int score; 

    public int current_score;

    public float moveSpeed;

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
        moveSpeed = Mathf.MoveTowards(moveSpeed, 3f, 0.01f * Time.deltaTime);
        scoreText.text = current_score.ToString("D5");
    }

    public void GainScore()
    {
        current_score += score;
    }

    public int GetScore()
    {
        int tmp = current_score;
        current_score = 0;
        return tmp;
    }
}
