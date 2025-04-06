using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{

    float moveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveSpeed = GameManager.instance.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);

        if (transform.position.y > 11)
        {
            transform.position = new Vector3(0, -11, 0);
        }
    }
}
