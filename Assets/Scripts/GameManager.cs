using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Rendering.DebugUI;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    public float moveSpeed;

    private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        moveSpeed = Mathf.MoveTowards(moveSpeed, 3f, 0.01f * Time.deltaTime);
    }
}
