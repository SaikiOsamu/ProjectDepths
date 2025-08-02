using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float lifetime = 10f; // Auto-destroy after this time
    [SerializeField] private LayerMask playerLayer = 3; // Player layer
    
    [Header("Audio")]
    [SerializeField] private string attackSound = "BossAttack"; // 弹幕发射音效
    [SerializeField] private string hitPlayerSound = "BulletHitPlayer";
    
    private Vector3 direction;
    private float timer = 0f;
    private bool hasHit = false;
    private AudioManager audioManager;
    
    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found in BossBullet");
        }
        else
        {
            // 播放弹幕发射音效
            Debug.Log($"BossBullet attempting to play attack sound: {attackSound}");
            audioManager.PlaySound(attackSound);
        }
    }
    
    public void Initialize(Vector3 moveDirection, float bulletSpeed)
    {
        direction = moveDirection.normalized;
        speed = bulletSpeed;
    }
    
    void Update()
    {
        if (hasHit) return;
        
        // Move bullet
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // Update timer
        timer += Time.deltaTime;
        
        // Auto-destroy after lifetime
        if (timer >= lifetime)
        {
            DestroySelf();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;
        
        // Check if hit player
        if (other.CompareTag("Player"))
        {
            hasHit = true;
            
            // Play hit sound
            if (audioManager != null)
            {
                audioManager.PlaySound(hitPlayerSound);
            }
            
            // Trigger player death immediately
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TriggerDeath();
            }
            
            // Destroy bullet immediately (no hit effect)
            DestroySelf();
        }
        // Check if hit BottomLine (bullet should be destroyed when hitting the bottom boundary)
        else if (other.GetComponent<BottomLine>() != null)
        {
            Debug.Log("Boss bullet hit BottomLine, destroying bullet");
            DestroySelf();
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;
        
        // Handle collision-based detection as backup
        if (collision.gameObject.CompareTag("Player"))
        {
            hasHit = true;
            
            // Play hit sound
            if (audioManager != null)
            {
                audioManager.PlaySound(hitPlayerSound);
            }
            
            // Trigger player death immediately
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TriggerDeath();
            }
            
            // Destroy bullet immediately (no hit effect)
            DestroySelf();
        }
        // Check if hit BottomLine using collision detection
        else if (collision.gameObject.GetComponent<BottomLine>() != null)
        {
            Debug.Log("Boss bullet hit BottomLine (collision), destroying bullet");
            DestroySelf();
        }
        else
        {
            // Hit something else, just destroy
            DestroySelf();
        }
    }
    
    void DestroySelf()
    {
        Destroy(gameObject);
    }
    
    // Called when bullet goes off-screen or lifetime expires
    void OnBecameInvisible()
    {
        // Small delay to avoid immediate destruction on spawn
        if (timer > 0.5f)
        {
            DestroySelf();
        }
    }
} 