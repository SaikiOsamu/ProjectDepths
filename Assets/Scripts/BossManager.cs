using System.Collections;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float bossYOffset = 8f; // Boss spawns this far above player
    
    [Header("Spawn Position")]
    [SerializeField] private float spawnX = 0f; // Center of screen
    
    [Header("Activity Area (Relative to Player)")]
    [SerializeField] private bool useActivityArea = true; // 是否启用活动区域限制
    [SerializeField] private float leftOffset = -8f; // 相对玩家位置的左边界偏移
    [SerializeField] private float rightOffset = 8f; // 相对玩家位置的右边界偏移
    [SerializeField] private float topOffset = 15f; // 相对玩家位置的上边界偏移
    [SerializeField] private float bottomOffset = 5f; // 相对玩家位置的下边界偏移
    [SerializeField] private bool showBoundaries = true; // 是否在Scene视图中显示边界
    
    [Header("Audio")]
    [SerializeField] private string bossAppearSound = "BossAppear";
    // 攻击音效现在由BossBullet处理，BossManager不再需要配置
    
    private bool gameActive = true;
    private AudioManager audioManager;
    private GameObject currentBoss; // 当前Boss实例
    
    public static BossManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found in BossManager");
        }
        else
        {
            Debug.Log("AudioManager found successfully in BossManager");
            
            // 检查AudioManager的状态
            Debug.Log($"AudioManager child count: {audioManager.transform.childCount}");
            
            // 列出所有子对象（音效）
            for (int i = 0; i < audioManager.transform.childCount; i++)
            {
                Transform child = audioManager.transform.GetChild(i);
                Debug.Log($"Audio child {i}: {child.name}");
            }
        }
        
        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure player has 'Player' tag.");
            }
        }
        
        // 验证活动区域设置
        if (useActivityArea)
        {
            ValidateActivityArea();
        }
        
        // 游戏开始时立即生成Boss
        SpawnBoss();
    }
    
    void Update()
    {
        // 检查Boss是否仍然存在，如果不存在则重新生成
        if (gameActive && currentBoss == null)
        {
            Debug.LogWarning("Boss was destroyed unexpectedly, respawning...");
            SpawnBoss();
        }
    }
    
    void ValidateActivityArea()
    {
        if (leftOffset >= rightOffset)
        {
            Debug.LogWarning("Boss活动区域设置错误：左偏移应该小于右偏移！");
        }
        
        if (bottomOffset >= topOffset)
        {
            Debug.LogWarning("Boss活动区域设置错误：下偏移应该小于上偏移！");
        }
        
        Debug.Log($"Boss活动区域偏移设置: 左{leftOffset}, 右{rightOffset}, 上{topOffset}, 下{bottomOffset} (相对玩家位置)");
    }
    
    // 计算当前相对于玩家的活动区域边界
    private void GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom)
    {
        if (player != null)
        {
            left = player.position.x + leftOffset;
            right = player.position.x + rightOffset;
            top = player.position.y + topOffset;
            bottom = player.position.y + bottomOffset;
        }
        else
        {
            // 如果没有玩家，使用默认值
            left = leftOffset;
            right = rightOffset;
            top = topOffset;
            bottom = bottomOffset;
        }
    }
    
    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Boss prefab is not assigned!");
            return;
        }
        
        // Calculate spawn position - above player at specified offset
        Vector3 spawnPosition = new Vector3(player.position.x + spawnX, player.position.y + bossYOffset, 0f);
        
        // 如果启用活动区域，确保生成位置在边界内
        if (useActivityArea)
        {
            GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
            spawnPosition.x = Mathf.Clamp(spawnPosition.x, left, right);
            spawnPosition.y = Mathf.Clamp(spawnPosition.y, bottom, top);
        }
        
        // Instantiate boss
        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        
        // Get boss component and initialize it with activity area
        Boss bossScript = currentBoss.GetComponent<Boss>();
        if (bossScript != null)
        {
            if (useActivityArea)
            {
                GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
                bossScript.Initialize(player, bossYOffset, left, right, top, bottom, true); // 传递相对模式标记
            }
            else
            {
                bossScript.Initialize(player, bossYOffset);
            }
            
            // 设置Boss为永久存在
            bossScript.SetPersistent();
        }
        
        // Play boss appear sound
        if (audioManager != null)
        {
            Debug.Log($"BossManager attempting to play appear sound: {bossAppearSound}");
            audioManager.PlaySound(bossAppearSound);
        }
        else
        {
            Debug.LogError("AudioManager is null in BossManager!");
        }
        
        Debug.Log($"Persistent Boss spawned at position: {spawnPosition} (player at: {player.position})");
    }
    
    public void StopBossSpawning()
    {
        gameActive = false;
    }
    
    public void ResumeBossSpawning()
    {
        gameActive = true;
    }
    
    // Called when player dies to stop boss spawning
    public void OnPlayerDeath()
    {
        StopBossSpawning();
        // 可选：销毁当前Boss
        DestroyCurrentBoss();
    }
    
    // 手动销毁当前Boss
    public void DestroyCurrentBoss()
    {
        if (currentBoss != null)
        {
            Boss bossScript = currentBoss.GetComponent<Boss>();
            if (bossScript != null)
            {
                bossScript.DestroyBoss();
            }
            currentBoss = null;
            Debug.Log("Current Boss destroyed by BossManager");
        }
    }
    
    // 重新启动Boss系统（重新生成Boss）
    public void RestartBossSystem()
    {
        gameActive = true;
        if (currentBoss == null)
        {
            SpawnBoss();
        }
    }
    
    // 在Scene视图中绘制活动区域边界（相对于玩家位置）
    void OnDrawGizmos()
    {
        if (!useActivityArea || !showBoundaries || player == null) return;
        
        // 计算当前活动区域边界
        GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
        
        // 设置Gizmo颜色
        Gizmos.color = Color.yellow;
        
        // 绘制活动区域矩形边界
        Vector3 topLeft = new Vector3(left, top, 0);
        Vector3 topRight = new Vector3(right, top, 0);
        Vector3 bottomLeft = new Vector3(left, bottom, 0);
        Vector3 bottomRight = new Vector3(right, bottom, 0);
        
        // 绘制矩形边框
        Gizmos.DrawLine(topLeft, topRight);      // 上边
        Gizmos.DrawLine(topRight, bottomRight);  // 右边
        Gizmos.DrawLine(bottomRight, bottomLeft); // 下边
        Gizmos.DrawLine(bottomLeft, topLeft);    // 左边
        
        // 绘制半透明填充区域
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f); // 半透明黄色
        Vector3 center = new Vector3((left + right) / 2, (top + bottom) / 2, 0);
        Vector3 size = new Vector3(right - left, top - bottom, 0.1f);
        Gizmos.DrawCube(center, size);
        
        // 绘制玩家位置标记
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(player.position, 0.5f);
        
        // 绘制Boss生成位置标记
        Gizmos.color = Color.green;
        Vector3 spawnPos = new Vector3(player.position.x + spawnX, player.position.y + bossYOffset, 0);
        Gizmos.DrawWireSphere(spawnPos, 0.8f);
        
        // 添加文字标签（仅在编辑器中）
        #if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(new Vector3(left - 1, top + 0.5f, 0), "Boss活动区域(跟随玩家)");
        UnityEditor.Handles.Label(spawnPos + Vector3.up * 1.5f, "Boss生成点");
        UnityEditor.Handles.Label(player.position + Vector3.down * 1.5f, "玩家位置");
        #endif
    }
} 