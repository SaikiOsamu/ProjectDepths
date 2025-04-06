using UnityEngine;

public class BlockMover : MonoBehaviour
{
    public float stepTime = 1f;
    public float stepSize = 1f;

    void Start()
    {
        InvokeRepeating("MoveDown", stepTime, stepTime);
    }

    void MoveDown()
    {
        transform.position += Vector3.down * stepSize;
    }
}

