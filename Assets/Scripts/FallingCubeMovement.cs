using UnityEngine;

public class FallingCubeMovement : MonoBehaviour
{

    float moveSpeed;
    float extraSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveSpeed = GameManager.instance.moveSpeed;
        extraSpeed = GameManager.instance.extraSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(Vector3.down * (GameManager.instance.moveSpeed + extraSpeed) * Time.deltaTime);
    }
}
