using UnityEngine;

public class BottomLine : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ceiling"))
        {
            Destroy(collision.gameObject);
        }
        else if(collision.CompareTag("Player"))
        {
            Debug.Log("Player Dead, game over.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
