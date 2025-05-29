using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float followSmoothness = 2f; // Y轴跟随的平滑度
    [SerializeField] private float waitTimeAtEdge = 3f; // 在边界停留的时间
    
    [Header("Attack Settings")]
    [SerializeField] private GameObject leftBulletPrefab; // 左手弹幕预制体
    [SerializeField] private GameObject rightBulletPrefab; // 右手弹幕预制体
    [SerializeField] private Transform leftFirePoint; // 左手开火点
    [SerializeField] private Transform rightFirePoint; // 右手开火点
    [SerializeField] private int bulletsPerAttack = 5;
    [SerializeField] private float leftBulletSpeed = 4f; // 左手弹幕速度
    [SerializeField] private float rightBulletSpeed = 4f; // 右手弹幕速度
    [SerializeField] private float timeBetweenBullets = 0.3f;
    [SerializeField] private float timeBetweenAttacks = 1f; // 移动时攻击间隔
    [SerializeField] private bool alternatingFire = true; // 是否交替开火
    [SerializeField] private bool simultaneousFire = false; // 是否同时开火
    
    [Header("Bullet Direction Settings")]
    [SerializeField] private bool leftBulletVertical = true; // 左手弹幕是否垂直向下
    [SerializeField] private bool rightBulletVertical = false; // 右手弹幕是否垂直向下
    [SerializeField] private float diagonalAngleOffset = 15f; // 斜向弹幕的角度偏移
    
    [Header("Animation Settings")]
    [SerializeField] private float entryAnimationDuration = 2.5f; // 入场动画时长
    [SerializeField] private float attackAnimationDuration = 1.5f; // 攻击动画时长
    
    [Header("Audio")]
    // 攻击音效现在由BossBullet处理，这里不再需要
    
    private Transform player;
    private float yOffset;
    private AudioManager audioManager;
    private bool useLeftHand = true; // 用于交替开火的标记
    private float lifetime = 0f; // Boss生存时间
    private bool hasLifetimeLimit = false;
    private float gameStartTime; // 记录Boss生成时间
    
    // 动画相关
    private Animator animator; // 动画控制器
    private bool isPlayingEntryAnimation = true; // 是否正在播放入场动画
    private float entryAnimationTimer = 0f; // 入场动画计时器
    
    // 活动区域参数（由BossManager传入）
    private bool useActivityArea = false;
    private bool isRelativeToPlayer = false; // 是否相对玩家位置的活动区域
    private float leftBoundary;
    private float rightBoundary;
    private float topBoundary;
    private float bottomBoundary;
    
    // 相对模式下的偏移量
    private float leftOffset;
    private float rightOffset;
    private float topOffset;
    private float bottomOffset;
    
    // 移动控制
    private bool movingRight = true; // 是否向右移动
    private float nextAttackTime = 0f; // 下次攻击时间
    
    // State machine
    private enum BossState
    {
        Entry,              // 入场状态
        MovingAndAttacking, // 移动并攻击
        WaitingAtLeft,      // 在左边界等待
        WaitingAtRight      // 在右边界等待
    }
    
    private BossState currentState = BossState.Entry; // 改为Entry开始
    private float stateTimer = 0f;
    private Coroutine attackCoroutine;
    
    void Start()
    {
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found in Boss");
        }
        
        // 获取Animator组件
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on Boss! Please add an Animator component.");
        }
        
        // 自动设置开火模式
        if (leftFirePoint == null || rightFirePoint == null)
        {
            Debug.LogWarning("Missing fire points! Please assign both left and right fire points.");
        }
        
        // 记录Boss生成时间
        gameStartTime = Time.time;
        
        // 开始入场动画
        StartEntryAnimation();
        
        Debug.Log("Boss spawned - will start attacking after 2.5 seconds");
    }
    
    void StartEntryAnimation()
    {
        isPlayingEntryAnimation = true;
        entryAnimationTimer = 0f;
        currentState = BossState.Entry;
        
        Debug.Log("Boss entry animation started");
        
        // 2.5秒后切换到攻击动画
        StartCoroutine(TransitionToAttackAnimation());
    }
    
    IEnumerator TransitionToAttackAnimation()
    {
        yield return new WaitForSeconds(entryAnimationDuration);
        
        // 触发攻击动画
        if (animator != null)
        {
            animator.SetTrigger("StartAttack");
            Debug.Log("Boss entry animation finished, starting attack animation");
        }
        
        // 结束入场状态，开始正常的攻击和移动
        isPlayingEntryAnimation = false;
        currentState = BossState.MovingAndAttacking;
        stateTimer = 0f;
    }
    
    // 设置Boss生存时间（现在可选，用于特殊情况）
    public void SetLifetime(float duration)
    {
        lifetime = duration;
        hasLifetimeLimit = true;
        Debug.Log($"Boss lifetime set to {duration} seconds (optional for special cases)");
    }
    
    // 手动销毁Boss（用于玩家死亡等特殊情况）
    public void DestroyBoss()
    {
        Debug.Log("Boss manually destroyed");
        DestroySelf();
    }
    
    // 设置Boss为永久存在
    public void SetPersistent()
    {
        hasLifetimeLimit = false;
        Debug.Log("Boss set to persistent mode - will not auto-destroy");
    }
    
    // 不带活动区域的初始化方法（向下兼容）
    public void Initialize(Transform playerTransform, float playerYOffset)
    {
        player = playerTransform;
        yOffset = playerYOffset;
        useActivityArea = false;
        Debug.Log("Boss initialized without activity area");
    }
    
    // 带活动区域的初始化方法 - 相对模式
    public void Initialize(Transform playerTransform, float playerYOffset, 
                          float left, float right, float top, float bottom, bool relativeToPlayer = false)
    {
        player = playerTransform;
        yOffset = playerYOffset;
        useActivityArea = true;
        isRelativeToPlayer = relativeToPlayer;
        
        if (isRelativeToPlayer && player != null)
        {
            // 相对模式：计算偏移量
            leftOffset = left - player.position.x;
            rightOffset = right - player.position.x;
            topOffset = top - player.position.y;
            bottomOffset = bottom - player.position.y;
            
            Debug.Log($"Boss initialized with relative activity area: " +
                     $"偏移 L{leftOffset}, R{rightOffset}, T{topOffset}, B{bottomOffset}");
        }
        else
        {
            // 绝对模式：直接使用边界值
            leftBoundary = left;
            rightBoundary = right;
            topBoundary = top;
            bottomBoundary = bottom;
            
            Debug.Log($"Boss initialized with absolute activity area: " +
                     $"边界 L{leftBoundary}, R{rightBoundary}, T{topBoundary}, B{bottomBoundary}");
        }
        
        ValidateActivityArea();
    }
    
    void ValidateActivityArea()
    {
        float currentLeft, currentRight, currentTop, currentBottom;
        GetCurrentActivityBounds(out currentLeft, out currentRight, out currentTop, out currentBottom);
        
        Debug.Log($"Boss活动区域验证完成: 左{currentLeft}, 右{currentRight}, 上{currentTop}, 下{currentBottom}");
    }
    
    // 获取当前活动区域边界
    private void GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom)
    {
        if (useActivityArea && isRelativeToPlayer && player != null)
        {
            // 相对模式：根据玩家当前位置计算边界
            left = player.position.x + leftOffset;
            right = player.position.x + rightOffset;
            top = player.position.y + topOffset;
            bottom = player.position.y + bottomOffset;
        }
        else if (useActivityArea)
        {
            // 绝对模式：使用固定边界
            left = leftBoundary;
            right = rightBoundary;
            top = topBoundary;
            bottom = bottomBoundary;
        }
        else
        {
            // 无活动区域限制
            left = float.MinValue;
            right = float.MaxValue;
            top = float.MaxValue;
            bottom = float.MinValue;
        }
    }
    
    void Update()
    {
        if (player == null) return;
        
        // Boss现在是永久存在的，移除生存时间检查
        // 如果需要销毁Boss，需要通过其他方式（如玩家死亡时）手动调用
        
        // 入场动画期间只进行Y轴跟随，不进行攻击和X轴移动
        if (isPlayingEntryAnimation)
        {
            entryAnimationTimer += Time.deltaTime;
            
            // 只进行Y轴平滑跟随
            Vector3 currentPos = transform.position;
            float targetY = player.position.y + yOffset;
            
            // 应用活动区域Y轴限制
            if (useActivityArea)
            {
                GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
                targetY = Mathf.Clamp(targetY, bottom, top);
            }
            
            // 使用平滑插值移动到目标Y位置
            float smoothedY = Mathf.Lerp(currentPos.y, targetY, followSmoothness * Time.deltaTime);
            currentPos.y = smoothedY;
            transform.position = currentPos;
            
            return; // 入场动画期间不执行其他逻辑
        }
        
        // 正常状态下的逻辑（原来的Update内容）
        Vector3 currentPosition = transform.position;
        float targetY2 = player.position.y + yOffset;
        
        // 应用活动区域Y轴限制
        if (useActivityArea)
        {
            GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
            targetY2 = Mathf.Clamp(targetY2, bottom, top);
        }
        
        // 使用平滑插值移动到目标Y位置
        float smoothedY2 = Mathf.Lerp(currentPosition.y, targetY2, followSmoothness * Time.deltaTime);
        currentPosition.y = smoothedY2;
        transform.position = currentPosition;
        
        // Handle state machine (入场动画结束后)
        HandleStateMachine();
        
        // 确保Boss始终在活动区域内
        if (useActivityArea)
        {
            ClampToActivityAreaX();
        }
    }
    
    // 只对X轴进行硬性限制，Y轴由平滑跟随处理
    void ClampToActivityAreaX()
    {
        GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
        
        Vector3 currentPos = transform.position;
        
        // 只限制X轴位置，Y轴由平滑跟随处理
        currentPos.x = Mathf.Clamp(currentPos.x, left, right);
        
        transform.position = currentPos;
    }
    
    void HandleStateMachine()
    {
        // Boss生成后2.5秒内不进行任何操作
        if (Time.time - gameStartTime < 2.5f)
            return;
            
        GetCurrentActivityBounds(out float left, out float right, out float top, out float bottom);
        
        switch (currentState)
        {
            case BossState.Entry:
                // Entry状态由动画系统处理，这里不需要额外逻辑
                break;
                
            case BossState.MovingAndAttacking:
                // 移动
                if (movingRight)
                {
                    MoveTowardsDirection(1f); // 向右移动
                    if (transform.position.x >= right - 0.1f)
                    {
                        ChangeState(BossState.WaitingAtRight);
                    }
                }
                else
                {
                    MoveTowardsDirection(-1f); // 向左移动
                    if (transform.position.x <= left + 0.1f)
                    {
                        ChangeState(BossState.WaitingAtLeft);
                    }
                }
                
                // 边移动边攻击
                if (Time.time >= nextAttackTime)
                {
                    FireSingleBullet();
                    nextAttackTime = Time.time + timeBetweenAttacks;
                }
                break;
                
            case BossState.WaitingAtLeft:
                stateTimer += Time.deltaTime;
                if (stateTimer >= waitTimeAtEdge)
                {
                    movingRight = true; // 改为向右移动
                    ChangeState(BossState.MovingAndAttacking);
                }
                
                // 等待时也继续攻击
                if (Time.time >= nextAttackTime)
                {
                    FireSingleBullet();
                    nextAttackTime = Time.time + timeBetweenAttacks;
                }
                break;
                
            case BossState.WaitingAtRight:
                stateTimer += Time.deltaTime;
                if (stateTimer >= waitTimeAtEdge)
                {
                    movingRight = false; // 改为向左移动
                    ChangeState(BossState.MovingAndAttacking);
                }
                
                // 等待时也继续攻击
                if (Time.time >= nextAttackTime)
                {
                    FireSingleBullet();
                    nextAttackTime = Time.time + timeBetweenAttacks;
                }
                break;
        }
    }
    
    void ChangeState(BossState newState)
    {
        // Exit current state
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        
        // Enter new state
        currentState = newState;
        stateTimer = 0f;
        
        Debug.Log($"Boss changed state to: {newState}");
    }
    
    void MoveTowardsDirection(float direction)
    {
        Vector3 movement = Vector3.right * direction * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }
    
    // 发射单发弹幕（移动时使用）
    void FireSingleBullet()
    {
        if (leftBulletPrefab == null || rightBulletPrefab == null || player == null) return;
        
        if (simultaneousFire)
        {
            // 同时从两个手开火
            FireBulletFromBothHands();
        }
        else if (alternatingFire)
        {
            // 交替开火
            FireBulletAlternating();
        }
        else
        {
            // 默认行为：随机选择一只手
            FireBulletRandom();
        }
    }
    
    void FireBulletFromBothHands()
    {
        if (leftBulletPrefab == null || rightBulletPrefab == null || player == null) return;
        
        // 从左手开火
        if (leftFirePoint != null)
        {
            FireBulletFromPoint(leftFirePoint, true); // true表示是左手
        }
        
        // 从右手开火
        if (rightFirePoint != null)
        {
            FireBulletFromPoint(rightFirePoint, false); // false表示是右手
        }
    }
    
    void FireBulletAlternating()
    {
        if (leftBulletPrefab == null || rightBulletPrefab == null || player == null) return;
        
        // 交替使用左右手
        if (useLeftHand && leftFirePoint != null)
        {
            FireBulletFromPoint(leftFirePoint, true); // 左手
        }
        else if (!useLeftHand && rightFirePoint != null)
        {
            FireBulletFromPoint(rightFirePoint, false); // 右手
        }
        
        // 切换到另一只手
        useLeftHand = !useLeftHand;
    }
    
    void FireBulletRandom()
    {
        if (leftBulletPrefab == null || rightBulletPrefab == null || player == null) return;
        
        // 随机选择开火点
        bool useLeft = Random.Range(0, 2) == 0;
        
        if (useLeft && leftFirePoint != null)
        {
            FireBulletFromPoint(leftFirePoint, true); // 左手
        }
        else if (!useLeft && rightFirePoint != null)
        {
            FireBulletFromPoint(rightFirePoint, false); // 右手
        }
    }
    
    void FireBulletFromPoint(Transform firePoint, bool isLeftHand)
    {
        if (firePoint == null || player == null) return;
        
        // 选择对应的弹幕预制体和速度
        GameObject bulletPrefab = isLeftHand ? leftBulletPrefab : rightBulletPrefab;
        float bulletSpeed = isLeftHand ? leftBulletSpeed : rightBulletSpeed;
        bool isVertical = isLeftHand ? leftBulletVertical : rightBulletVertical;
        
        if (bulletPrefab == null) return;
        
        Vector3 direction;
        
        if (isVertical)
        {
            // 垂直向下射击
            direction = Vector3.down;
        }
        else
        {
            // 斜向射击（指向玩家方向，但加入角度偏移）
            Vector3 toPlayer = (player.position - firePoint.position).normalized;
            
            // 添加随机角度偏移，让弹幕更有变化
            float randomAngle = Random.Range(-diagonalAngleOffset, diagonalAngleOffset);
            
            // 计算旋转后的方向
            float radians = randomAngle * Mathf.Deg2Rad;
            direction = new Vector3(
                toPlayer.x * Mathf.Cos(radians) - toPlayer.y * Mathf.Sin(radians),
                toPlayer.x * Mathf.Sin(radians) + toPlayer.y * Mathf.Cos(radians),
                0
            ).normalized;
        }
        
        // 创建子弹
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // 初始化子弹
        BossBullet bulletScript = bullet.GetComponent<BossBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(direction, bulletSpeed);
        }
        else
        {
            // 备用方案：如果没有自定义脚本，使用刚体
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }
        }
        
        // 添加调试信息
        string handName = isLeftHand ? "左手" : "右手";
        string directionName = isVertical ? "垂直向下" : "斜向";
        Debug.Log($"Boss {handName} 发射 {directionName} 弹幕，方向: {direction}");
    }
    
    void DestroySelf()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        
        Debug.Log("Boss destroyed");
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
    }
} 