using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    [Header("Destruction Settings")]
    [SerializeField] private bool showDestructionEffect = true;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private float effectDuration = 1f;

    float moveSpeed;

    public void Start()
    {
       // moveSpeed = GameManager.instance.objMoveSpeed;
    }

    public void Update()
    {
        transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
        if (transform.position.y > 12f)
            Destroy(gameObject);
    }


    void OnDestroy()
    {
        // Only create the destruction effect if the game is running
        // (not when the scene is being unloaded or the object is destroyed in editor)
        if (Application.isPlaying && showDestructionEffect && destructionEffectPrefab != null)
        {
            // Instantiate the destruction effect at the position of this object
            GameObject effect = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);

            // Destroy the effect after the specified duration
            Destroy(effect, effectDuration);
        }
    }
}