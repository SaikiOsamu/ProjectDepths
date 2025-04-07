using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardObject : MonoBehaviour
{


    public void Start()
    {
    }

    public void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ceiling"))
        {
            Destroy(gameObject);
        }
    }
}