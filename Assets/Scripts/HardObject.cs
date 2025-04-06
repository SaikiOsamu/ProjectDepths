using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardObject : MonoBehaviour
{

    float moveSpeed;

    public void Start()
    {
        moveSpeed = GameManager.instance.moveSpeed;
    }

    public void Update()
    {
        moveSpeed = GameManager.instance.moveSpeed;
        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
        if (transform.position.y > 11f)
            Destroy(gameObject);
    }
}