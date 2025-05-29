using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Destroy Range")]
    [SerializeField] private float destroyRange;

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
    private bool isAttacking = false; // New flag to track any attack state

    // Audio Manager
    [SerializeField] string fistAttackSound = "FistAttack";
    [SerializeField] string swordAttackSound = "SwordAttack";
    [SerializeField] string playerMoveSound = "WalkSound";
    [SerializeField] string playerDeadSound = "DeadSound";

    // We still need these & error window click close
    [SerializeField] string brickHitByFist = "BricksHitFist";
    [SerializeField] string brickHitBySaber = "BricksHitSaber";
    [SerializeField] string metalHitByFist = "MetalHitFist";
    [SerializeField] string metalHitBySaber = "MetalHitSaber";

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

        // Only process movement if not attacking
        if (!isAttacking)
        {
            // Handle movement input
            HandleMovement();
        }
        else
        {
            // Stop movement when attacking
            StopMovement();
        }

        // Only handle attack input if not currently in an attack state
        // This prevents starting a new attack while one is already in progress
        if (!isAttacking)
        {
            // Handle attack input
            HandleAttackInput();
        }

        // We'll handle destruction in the attack methods directly
        // No need to call HandleDestroyInput() anymore since the attack
        // methods themselves will call DestroyObjectInDirection()
    }

    // New method to stop movement
    void StopMovement()
    {
        // Stop horizontal movement
        Vector2 newVelocity = rb.linearVelocity;
        newVelocity.x = 0;
        rb.linearVelocity = newVelocity;

        // Update animation parameters to stop walking animation
        if (animator != null)
        {
            animator.SetFloat(HORIZONTAL_MOVEMENT, 0);
            animator.SetBool(IS_MOVING, false);
        }
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
        // Don't allow starting new attacks if already attacking
        if (isAttacking) return;

        // For example, using E key for right attack and Q key for left attack
        if (Input.GetKeyDown(KeyCode.E))
        {
            isAttackingRight = true;
            isAttacking = true; // Set master attack flag

            // Call destroy method first to ensure it happens
            DestroyObjectInDirection(Vector2.right);

            // Play sound
            audioManager.PlaySound(swordAttackSound);

            animator.SetBool("AttackRight", true);
            StartCoroutine(ResetAttackParameters()); // Start coroutine here
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            isAttackingLeft = true;
            isAttacking = true; // Set master attack flag

            // Call destroy method first to ensure it happens
            DestroyObjectInDirection(Vector2.left);

            // Play sound
            audioManager.PlaySound(swordAttackSound);

            animator.SetBool("AttackLeft", true);
            StartCoroutine(ResetAttackParameters()); // Start coroutine here
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            isSlaming = true;
            isAttacking = true; // Set master attack flag

            // Call destroy method first to ensure it happens
            DestroyObjectInDirection(Vector2.down);

            // Play sound
            audioManager.PlaySound(fistAttackSound);

            animator.SetBool("Slam", true);
            StartCoroutine(ResetAttackParameters()); // Start coroutine here for slam too
        }
    }

    IEnumerator ResetAttackParameters()
    {
        // Wait briefly to allow the animation trigger to be detected
        yield return new WaitForSeconds(0.33f);

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

        // Reset master attack flag after all specific attack flags are reset
        isAttacking = false;
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
        // Process destroy inputs when attack keys are initially pressed
        // This is integrated with the attack system, so we want these to work together

        // Check for Q key to destroy object on the left
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Try the primary method first
            DestroyObjectInDirection(Vector2.left);
        }
        // Check for E key to destroy right
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
        Vector2Int gridDir = Vector2Int.RoundToInt(direction);
        Vector2 targetPosition = (Vector2)transform.position + (Vector2)gridDir;

        float detectionRadius = 0.1f;
        Collider2D hit = Physics2D.OverlapCircle(targetPosition, detectionRadius);

        if (hit != null)
        {
            Debug.Log("OverlapPoint hit: " + hit.name + " at " + targetPosition);

            if (hit.CompareTag("DestroyableObject"))
            {
                DestroyableObject destroyable = hit.GetComponent<DestroyableObject>();
                if (destroyable != null)
                {
                    destroyable.TriggerDestruction();
                    if (isAttackingLeft || isAttackingRight)
                        audioManager.PlaySound(brickHitBySaber);
                }
                else
                {
                    Destroy(hit.gameObject);
                }
            }
            else if (hit.CompareTag("HardObject"))
            {
                if (isAttackingLeft || isAttackingRight)
                    audioManager.PlaySound(metalHitBySaber);
                else if (isSlaming)
                    audioManager.PlaySound(metalHitByFist);
            }
        }
        else
        {
            Debug.Log("No object found at grid position: " + targetPosition);
        }
    }

    /*
    void DestroyObjectInDirection(Vector2 direction)
    {
        // Get the renderer to find the exact size of the player sprite
        SpriteRenderer playerRenderer = GetComponent<SpriteRenderer>();

        // Calculate offset as before
        float offsetDistance = 0.55f;
        if (playerRenderer != null)
        {
            offsetDistance = Mathf.Max(playerRenderer.bounds.extents.x, playerRenderer.bounds.extents.y) + 0.1f;
        }
        else
        {
            Collider2D colliderComponent = GetComponent<Collider2D>();
            if (colliderComponent != null)
            {
                offsetDistance = Mathf.Max(colliderComponent.bounds.extents.x, colliderComponent.bounds.extents.y) + 0.1f;
            }
        }

        // Starting position for the raycast
        Vector2 rayOrigin = (Vector2)transform.position + (direction * offsetDistance);

        // Create a layer mask that ignores both the player's layer AND the Ruins layer
        int ruinsLayer = LayerMask.NameToLayer("Ruins"); // Make sure this matches your layer name
        int playerLayer = gameObject.layer;

        // Create a mask that includes everything EXCEPT the player and ruins layers
        int layerMask = ~((1 << playerLayer) | (1 << ruinsLayer));

        // Cast the ray with the updated layer mask
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, direction, destroyRange, layerMask);

        // Rest of your method remains the same...
        Debug.DrawRay(rayOrigin, direction * destroyRange, Color.green, 2f);

        // Check if the ray hit something
        if (hits.Length > 0)
        {
            RaycastHit2D closestHit = hits[0];
            float closestDistance = Vector2.Distance(rayOrigin, closestHit.point);

            foreach (RaycastHit2D hit in hits)
            {
                float distance = Vector2.Distance(rayOrigin, hit.point);
                if (distance < closestDistance)
                {
                    closestHit = hit;
                    closestDistance = distance;
                }
            }

            GameObject hitObject = closestHit.collider.gameObject;
            Debug.Log("Closest object hit: " + hitObject.name + " at distance: " + closestDistance);

            // We don't need to check for Ruin tag anymore since the layer mask excludes them

            // Check if the hit object has the "DestroyableObject" tag
            if (hitObject.CompareTag("DestroyableObject"))
            {
                // Get the DestroyableObject component and trigger destruction
                DestroyableObject destroyable = hitObject.GetComponent<DestroyableObject>();
                if (destroyable != null)
                {
                    //audioManager.PlaySound();
                    destroyable.TriggerDestruction();
                    if (isAttackingLeft || isAttackingRight)
                    {
                        audioManager.PlaySound(brickHitBySaber);
                    }
                }
                else
                {
                    // Fallback to direct destruction
                    Destroy(hitObject.gameObject);
                }
                Debug.Log("Destroyed object: " + hitObject.gameObject.name);
                return;
            }
            else if (hitObject.CompareTag("HardObject"))
            {
                if (isAttackingLeft || isAttackingRight)
                {
                    audioManager.PlaySound(metalHitBySaber);
                }
                else if (isSlaming)
                {
                    audioManager.PlaySound(metalHitByFist);
                }
            }
        }
        else
        {
            Debug.Log("Ray didn't hit any object. Try increasing range or adjusting the ray origin.");
        }

        // Update the layer mask in the alternative methods too
        TryAlternativeRaycast(rayOrigin, direction, layerMask);
    } */

    // Updated to accept the layer mask parameter
    void TryAlternativeRaycast(Vector2 rayOrigin, Vector2 direction, int layerMask)
    {
        // Use the passed in layer mask that already excludes ruins and player
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            rayOrigin,     // Origin
            0.1f,          // Radius 
            direction,     // Direction
            destroyRange,  // Distance
            layerMask      // Layer mask that excludes Ruins
        );

        // Process all hits
        foreach (RaycastHit2D hit in hits)
        {
            // We don't need to check for Ruin tag anymore
            if (hit.collider.CompareTag("DestroyableObject"))
            {
                DestroyableObject destroyable = hit.collider.GetComponent<DestroyableObject>();
                if (destroyable != null)
                {
                    destroyable.TriggerDestruction();
                    Debug.Log("Destroyed object using CircleCast: " + hit.collider.gameObject.name);
                    return;
                }
                else
                {
                    Destroy(hit.collider.gameObject);
                    Debug.Log("Destroyed object using CircleCast: " + hit.collider.gameObject.name);
                    return;
                }
            }
        }

        // If we still haven't found anything, use the final backup method
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

            // Skip Ruin objects
            if (collider.CompareTag("Ruin"))
                continue;

            // Calculate direction to the object
            Vector2 toObject = collider.transform.position - transform.position;

            // Dot product tells us if the object is in the general direction we want
            float dot = Vector2.Dot(direction.normalized, toObject.normalized);

            // If dot > 0.7, object is within about 45 degrees of our direction
            if (dot > 1f)
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