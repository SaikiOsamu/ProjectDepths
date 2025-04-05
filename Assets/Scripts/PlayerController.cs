using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Destroy Range")]
    [SerializeField] private float destroyRange = 1f;

    private Rigidbody2D rb;

    void Start()
    {
        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();

        // If there's no Rigidbody2D attached, add one
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // Disable gravity for top-down movement
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation
        }
    }

    void Update()
    {
        // Handle movement input
        HandleMovement();

        // Handle destroy input
        HandleDestroyInput();
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;

        // Check for A key (left)
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        // Check for D key (right)
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        // Move the player horizontally
        Vector2 movement = new Vector2(horizontalInput * moveSpeed, 0f);
        rb.linearVelocity = movement;
    }

    void HandleDestroyInput()
    {
        // Check for Q key to destroy object on the left
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Try the primary method first
            DestroyObjectInDirection(Vector2.left);
        }
        // Check for D key to move right, E key to destroy right
        else if (Input.GetKeyDown(KeyCode.E))
        {
            // Try the primary method first
            DestroyObjectInDirection(Vector2.right);
        }
        // Check for Space key to destroy object below
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // Try the primary method first
            DestroyObjectInDirection(Vector2.down);
        }
    }

    // This is a completely different approach that doesn't use raycasts
    void FindAndDestroyNearestInDirection(Vector2 direction)
    {
        // Find all colliders in the scene
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();

        // Variables to track closest valid object
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in allColliders)
        {
            // Skip our own collider
            if (collider.gameObject == gameObject)
                continue;

            // Skip objects that don't have the DestroyableObject tag
            if (!collider.CompareTag("DestroyableObject"))
                continue;

            // Calculate direction to the object
            Vector2 toObject = collider.transform.position - transform.position;

            // Dot product tells us if the object is in the general direction we want
            float dot = Vector2.Dot(direction.normalized, toObject.normalized);

            // If dot > 0.7, object is within about 45 degrees of our direction
            if (dot > 0.7f)
            {
                // Check if this is closer than any object we've found so far
                float distance = toObject.magnitude;
                if (distance < closestDistance && distance < destroyRange)
                {
                    closestObject = collider.gameObject;
                    closestDistance = distance;
                }
            }
        }

        // If we found a valid object, destroy it
        if (closestObject != null)
        {
            Debug.Log("Found and destroying nearest object in direction " + direction + ": " + closestObject.name);
            Destroy(closestObject);
        }
    }

    void DestroyObjectInDirection(Vector2 direction)
    {
        // Get the renderer to find the exact size of the player sprite
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();

        // Calculate a safe offset to ensure the ray starts outside the player's collider
        float offsetDistance = 0.55f; // Default value

        if (playerRenderer != null)
        {
            // Use the sprite bounds to get a precise measurement
            offsetDistance = Mathf.Max(playerRenderer.bounds.extents.x, playerRenderer.bounds.extents.y) + 0.1f;
        }
        else
        {
            // Fallback to using the collider if available
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                // Use the bounds to get a more accurate size
                offsetDistance = Mathf.Max(playerCollider.bounds.extents.x, playerCollider.bounds.extents.y) + 0.1f;
            }
        }

        // Starting position for the raycast (OUTSIDE the player's collider)
        Vector2 rayOrigin = (Vector2)transform.position + (direction * offsetDistance);

        // Cast a ray in the specified direction
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, destroyRange);

        // Debug ray for visualization in Scene view
        Debug.DrawRay(rayOrigin, direction * destroyRange, Color.green, 2f);

        // Log the raycast attempt for debugging
        Debug.Log("Casting ray in direction: " + direction + " with range: " + destroyRange + " from position: " + rayOrigin);

        // Check if the ray hit something
        if (hit.collider != null)
        {
            Debug.Log("Ray hit object: " + hit.collider.gameObject.name + " with tag: " + hit.collider.tag);

            // Make sure we didn't hit ourselves
            if (hit.collider.gameObject == gameObject)
            {
                Debug.LogError("Still hitting our own collider! Increase offset distance.");
                return;
            }

            // Check if the hit object has the "DestroyableObject" tag
            if (hit.collider.CompareTag("DestroyableObject"))
            {
                // Destroy the hit object
                Destroy(hit.collider.gameObject);
                Debug.Log("Destroyed object: " + hit.collider.gameObject.name);
                return; // Added return here to ensure no further destroy methods are called
            }
        }
        else
        {
            Debug.Log("Ray didn't hit any object. Try increasing range or adjusting the ray origin.");

            // Let's try one more approach - a sphere cast
            RaycastHit2D sphereHit = Physics2D.CircleCast(
                rayOrigin, // Origin
                0.1f,      // Radius 
                direction, // Direction
                destroyRange // Distance
            );

            if (sphereHit.collider != null && sphereHit.collider.gameObject != gameObject)
            {
                Debug.Log("CircleCast hit: " + sphereHit.collider.gameObject.name);

                if (sphereHit.collider.CompareTag("DestroyableObject"))
                {
                    Destroy(sphereHit.collider.gameObject);
                    Debug.Log("Destroyed object using CircleCast: " + sphereHit.collider.gameObject.name);
                    return; // Added return here to ensure no further destroy methods are called
                }
            }
        }
    }
}