using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    [Header("Destruction Settings")]
    [SerializeField] private bool showDestructionEffect = true;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private float effectDuration = 1f;

    [Header("Particle System")]
    [SerializeField] private GameObject particleSystemPrefab; // Reference to your DestroyCubeParticleSystem

    void OnDestroy()
    {
        // Only create the destruction effect if the game is running
        // (not when the scene is being unloaded or the object is destroyed in editor)
        if (Application.isPlaying && showDestructionEffect)
        {
            // Instantiate the original destruction effect if available
            if (destructionEffectPrefab != null)
            {
                GameObject effect = Instantiate(destructionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }

            // Instantiate the particle system
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
    }
}