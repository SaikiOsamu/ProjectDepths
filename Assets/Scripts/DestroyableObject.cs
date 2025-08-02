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

    [Header("Spawn Settings")]
    [SerializeField] private GameObject spawnPrefab; // 要生成的预制体
    [SerializeField] private Transform spawnArea; // 生成区域的中心点
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(2f, 2f); // 生成区域的大小
    [SerializeField] private bool enableSpawning = true; // 是否启用生成功能

    private bool isDestroying = false;

    [SerializeField] string bricksHitBySaber = "BricksHitSaber";
    [SerializeField] string bricksHitByFist = "BricksHitFist";
    [SerializeField] string bricksBroken = "BricksBroken";
    [SerializeField] string metalHitByFist = "MetalHitFist";
    [SerializeField] string metalHitBySaber = "MetalHitSaber";

    AudioManager audioManager;

    private void Start()
    {
        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }
    }

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
        
        // 在特定区域生成预制体
        //SpawnPrefabInArea();

        if (!isDestroying)
        {
            isDestroying = true;

            audioManager.PlaySound(bricksBroken);

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ceiling"))
        {
            Destroy(gameObject);
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

    private void SpawnPrefabInArea()
    {
        // 检查是否启用生成功能
        if (!enableSpawning || spawnPrefab == null)
        {
            return;
        }
        
        Vector3 spawnPosition;
        
        // 如果指定了生成区域，在该区域内随机生成
        if (spawnArea != null)
        {
            // 在指定区域内随机选择位置
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomY = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            
            spawnPosition = spawnArea.position + new Vector3(randomX, randomY, 0);
        }
        else
        {
            // 如果没有指定区域，在当前物体位置附近生成
            float randomX = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float randomY = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            
            spawnPosition = transform.position + new Vector3(randomX, randomY, 0);
        }
        
        // 生成预制体
        GameObject spawnedObject = Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
        
        Debug.Log($"Spawned {spawnPrefab.name} at position {spawnPosition}");
    }
}