using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Destroy Range")]
    [SerializeField] private float destroyRange = 1f;

    [Header("Game Over References")]
    [SerializeField] private GameOverManager gameOverManager;
    [SerializeField] private float deathAnimationDuration = 2.2f; // 65 frames at 30fps ≈ 2.2 seconds

    [Header("Animation")]
    private Animator animator;

    // Animation parameter names
    private const string HORIZONTAL_MOVEMENT = "HorizontalMovement";
    private const string IS_MOVING = "IsMoving";
    private const string DIE_TRIGGER = "Die";

    private Rigidbody2D rb;

    // Variables to track key press timing
    private float lastAKeyPressTime = 0f;
    private float lastDKeyPressTime = 0f;

    private bool isAttackingRight = false;
    private bool isAttackingLeft = false;
    private bool isDead = false;
    private bool isSlaming = false;

    // Audio Manager
    [SerializeField] string fistAttackSound = "FistAttack";
    [SerializeField] string swordAttackSound = "SwordAttack";
    [SerializeField] string playerMoveSound = "WalkSound";
    [SerializeField] string playerDeadSound = "DeadSound";

    AudioManager audioManager;

    void Start()
    {
        // Audio Manager
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }


        // Get the Rigidbody2D component attached to this GameObject
        rb = GetComponent<Rigidbody2D>();

        // Always ensure rotation is frozen
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Get animator component
        animator = GetComponent<Animator>();

        // Find game over manager if not assigned
        if (gameOverManager == null)
        {
            gameOverManager = FindObjectOfType<GameOverManager>();
            if (gameOverManager == null)
            {
                Debug.LogError("No GameOverManager found in the scene!");
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // Handle movement input
        HandleMovement();

        // Handle attack input
        HandleAttackInput();

        // Handle destroy input
        HandleDestroyInput();
    }

    // Modify your OnTriggerEnter2D method
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Ceiling") || collision.CompareTag("Falling")) && !isDead)
        {
            TriggerDeath();
        }
    }

    public void TriggerDeath()
    {
        if (!isDead)
        {
            isDead = true;

            audioManager.PlaySound(playerDeadSound);

            // Disable player movement
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;

            // Freeze everything except the player's animation
            // Set timescale to 0 to freeze all other objects
            Time.timeScale = 0f;

            // ADDED: Freeze the cursor by hiding and locking it
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            // Play death animation
            if (animator != null)
            {
                // Make sure player animation still runs despite frozen time
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                animator.SetTrigger(DIE_TRIGGER);

                // Wait for animation to complete before showing game over panel
                StartCoroutine(ShowGameOverAfterAnimation());
            }
            else
            {
                // If no animator, show game over immediately
                ShowGameOver();
            }
        }
    }

    private IEnumerator ShowGameOverAfterAnimation()
    {
        // Wait for the death animation to complete using unscaled time
        // since Time.timeScale is set to 0
        yield return new WaitForSecondsRealtime(deathAnimationDuration);

        // ADDED: Unfreeze the cursor after animation completes
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Show the game over panel
        ShowGameOver();
    }

    private void ShowGameOver()
    {
        // ADDED: Additional safety check to ensure cursor is unfrozen
        // when the game over screen is shown
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Show game over panel
        if (gameOverManager != null)
        {
            gameOverManager.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager reference is missing!");
        }
    }

    void HandleAttackInput()
    {
        // For example, using E key for right attack and Q key for left attack
        if (Input.GetKeyDown(KeyCode.E))
        {
            isAttackingRight = true;

            // Play sound
            audioManager.PlaySound(swordAttackSound);

            animator.SetBool("AttackRight", true);
            StartCoroutine(ResetAttackParameters()); // Start coroutine here
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            isAttackingLeft = true;

            // Play sound
            audioManager.PlaySound(swordAttackSound);

            animator.SetBool("AttackLeft", true);
            StartCoroutine(ResetAttackParameters()); // Start coroutine here
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            isSlaming = true;

            // Play sound
            audioManager.PlaySound(fistAttackSound);

            animator.SetBool("Slam", true);
            StartCoroutine(ResetAttackParameters()); // Start coroutine here for slam too
        }
    }

    IEnumerator ResetAttackParameters()
    {
        // Wait briefly to allow the animation trigger to be detected
        yield return new WaitForSeconds(0.1f);

        if (isAttackingRight)
        {
            animator.SetBool("AttackRight", false);
            isAttackingRight = false;
        }

        if (isAttackingLeft)
        {
            animator.SetBool("AttackLeft", false);
            isAttackingLeft = false;
        }
        if (isSlaming)
        {
            animator.SetBool("Slam", false);
            isSlaming = false;
        }
    }

    void HandleMovement()
    {
        float horizontalInput = 0f;

        // Check key states and update press times
        if (Input.GetKeyDown(KeyCode.A))
        {
            // Play sound
            audioManager.PlaySound(playerMoveSound);

            lastAKeyPressTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            // Play sound
            audioManager.PlaySound(playerMoveSound);

            lastDKeyPressTime = Time.time;
        }

        // Determine direction based on most recently pressed key that is still held
        bool isAHeld = Input.GetKey(KeyCode.A);
        bool isDHeld = Input.GetKey(KeyCode.D);

        if (isAHeld && isDHeld)
        {
            // Both keys are pressed, use the one pressed more recently
            if (lastDKeyPressTime > lastAKeyPressTime)
            {
                horizontalInput = 1f; // Move right
            }
            else
            {
                horizontalInput = -1f; // Move left
            }
        }
        else if (isAHeld)
        {
            horizontalInput = -1f; // Move left
        }
        else if (isDHeld)
        {
            horizontalInput = 1f; // Move right
        }

        // Only change the x component of velocity, preserve vertical velocity
        Vector2 newVelocity = rb.linearVelocity;
        newVelocity.x = horizontalInput * moveSpeed;
        rb.linearVelocity = newVelocity;

        // Update animation parameters
        if (animator != null)
        {
            animator.SetFloat(HORIZONTAL_MOVEMENT, horizontalInput);
            animator.SetBool(IS_MOVING, horizontalInput != 0);
        }
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
                // Get the DestroyableObject component and trigger destruction
                DestroyableObject destroyable = hit.collider.GetComponent<DestroyableObject>();
                if (destroyable != null)
                {
                    destroyable.TriggerDestruction();
                }
                else
                {
                    // Fallback to direct destruction if the object doesn't have our component
                    Destroy(hit.collider.gameObject);
                }
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
                    // Get the DestroyableObject component and trigger destruction
                    DestroyableObject destroyable = sphereHit.collider.GetComponent<DestroyableObject>();
                    if (destroyable != null)
                    {
                        destroyable.TriggerDestruction();
                    }
                    else
                    {
                        // Fallback to direct destruction if the object doesn't have our component
                        Destroy(sphereHit.collider.gameObject);
                    }
                    Debug.Log("Destroyed object using CircleCast: " + sphereHit.collider.gameObject.name);
                    return; // Added return here to ensure no further destroy methods are called
                }
            }
        }

        // If we got here, try the backup method
        FindAndDestroyNearestInDirection(direction);
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

            // Get the DestroyableObject component and trigger destruction
            DestroyableObject destroyable = closestObject.GetComponent<DestroyableObject>();
            if (destroyable != null)
            {
                destroyable.TriggerDestruction();
            }
            else
            {
                // Fallback to direct destruction if the object doesn't have our component
                Destroy(closestObject);
            }
        }
    }
}