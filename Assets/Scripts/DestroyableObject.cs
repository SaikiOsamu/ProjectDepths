using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    [Header("Destruction Settings")]
    [SerializeField] private bool showDestructionEffect = true;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private float effectDuration = 1f;

    [Header("Ruin Settings")]
    [SerializeField] private GameObject ruinPrefab; // Reference to your RuinPrefab
    [SerializeField] private float ruinDuration = 3f; // How long the ruins stay visible

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string destroyTriggerName = "Destroy";
    [SerializeField] private float animationTime = 0.3f; // Time to play the animation before spawning ruins

    [Header("Particle System")]
    [SerializeField] private GameObject particleSystemPrefab; // Reference to your DestroyCubeParticleSystem

    private bool isDestroying = false;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void Update()
    {

    }

    // Public method to trigger destruction process
    public void TriggerDestruction()
    {
        GameManager.instance.GainScore();
        if (!isDestroying)
        {
            isDestroying = true;

            // Trigger animation if we have an animator
            if (animator != null)
            {
                animator.SetTrigger(destroyTriggerName);

                // Wait for animation before destroying
                StartCoroutine(DestroyAfterAnimation());
            }
            else
            {
                // No animator, destroy immediately
                DestroyAndSpawnRuins();
            }
        }
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // Wait for animation to play
        yield return new WaitForSeconds(animationTime);

        // Now destroy and spawn ruins
        DestroyAndSpawnRuins();
    }

    private void DestroyAndSpawnRuins()
    {
        // Spawn the ruin prefab
        if (ruinPrefab != null)
        {
            GameObject ruins = Instantiate(ruinPrefab, transform.position, transform.rotation);

            // Initialize the ruins with the duration
            RuinPrefab ruinScript = ruins.GetComponent<RuinPrefab>();
            if (ruinScript != null)
            {
                ruinScript.Initialize(ruinDuration);
            }
            else
            {
                // If no RuinPrefab script, just destroy after duration
                Destroy(ruins, ruinDuration);
            }

            // Spawn particle system
            SpawnParticleSystem();
        }

        // Now destroy this object
        Destroy(gameObject);
    }

    private void SpawnParticleSystem()
    {
        // Only create the particle system if it's available
        if (particleSystemPrefab != null)
        {
            GameObject particleEffect = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity);

            // Get the particle system component
            ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Make sure it plays (if not set to play on awake)
                ps.Play();

                // Calculate proper duration to destroy the game object
                float totalDuration = ps.main.duration;
                if (ps.main.startLifetimeMultiplier > 0)
                {
                    totalDuration += ps.main.startLifetimeMultiplier;
                }

                // Destroy the particle system object after it completes
                Destroy(particleEffect, totalDuration);
            }
            else
            {
                // Fallback if no particle system component found
                Destroy(particleEffect, effectDuration);
            }
        }
    }

    void OnDestroy()
    {
        // Only create the destruction effect if the game is running
        // (not when the scene is being unloaded or the object is destroyed in editor)
        if (Application.isPlaying && showDestructionEffect && !isDestroying)
        {
            // We use !isDestroying check to avoid double effects when we're already in our destruction sequence

            // Instantiate the original destruction effect if available
            if (destructionEffectPrefab != null)
            {
                GameObject effect = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }

            // We don't spawn particle system here since we're not in the normal destroy sequence
        }
    }
}