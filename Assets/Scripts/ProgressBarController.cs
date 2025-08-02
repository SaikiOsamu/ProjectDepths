using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    public Scrollbar progressBar;
    public float fillDuration = 30f;

    [Header("Reward Settings")]
    [SerializeField] private int scoreReward = 100;
    
    [Header("Prefab Spawning")]
    [SerializeField] private GameObject rewardPrefab; // 要生成的prefab
    [SerializeField] private Transform spawnerTransform; // +100Spawner的Transform，如果为空会自动查找
    [SerializeField] private Canvas targetCanvas; // UI prefab需要的Canvas父对象
    [SerializeField] private bool isUIPrefab = true; // 是否为UI prefab
    
    [Header("Debug/Test")]
    [SerializeField] private bool enableTestKey = true; // 是否启用测试按键
    [SerializeField] private KeyCode testKey = KeyCode.F; // 测试按键
    
    private float timer = 0f;

    void Start()
    {
        UpdateProgressBar(0f);
        
        // 如果是UI prefab但没有指定Canvas，自动查找合适的Canvas
        if (isUIPrefab && targetCanvas == null)
        {
            FindSuitableCanvas();
        }
    }

    void Update()
    {
        // 测试按键功能
        if (enableTestKey && Input.GetKeyDown(testKey))
        {
            Debug.Log("Test key pressed! Spawning reward prefab...");
            SpawnRewardPrefab();
        }
        
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / fillDuration);
        UpdateProgressBar(progress);

        if (progress >= 1f)
        {
            timer = 0f;
            UpdateProgressBar(0f);
            
            // 加分
            GameManager.instance.GainScore(scoreReward);
            
            // 生成prefab
            SpawnRewardPrefab();
        }
    }

    void UpdateProgressBar(float progress)
    {
        progressBar.size = Mathf.Lerp(0f, 0.92f, progress);
    }
    
    void SpawnRewardPrefab()
    {
        if (rewardPrefab == null)
        {
            Debug.LogWarning("No reward prefab assigned!");
            return;
        }

        GameObject spawnedReward = null;

        if (isUIPrefab)
        {
            // UI prefab处理
            if (targetCanvas != null)
            {
                // 在Canvas下生成UI prefab
                spawnedReward = Instantiate(rewardPrefab, targetCanvas.transform);
                
                // 获取RectTransform来设置UI位置
                RectTransform rewardRect = spawnedReward.GetComponent<RectTransform>();
                if (rewardRect != null && spawnerTransform != null)
                {
                    // 将世界坐标转换为Canvas内的UI坐标
                    Vector2 uiPosition = WorldToCanvasPosition(spawnerTransform.position);
                    rewardRect.anchoredPosition = uiPosition;
                    
                    Debug.Log($"Spawned UI prefab at canvas position: {uiPosition}");
                }
                else if (rewardRect != null)
                {
                    // 如果没有spawner位置，放在Canvas中心
                    rewardRect.anchoredPosition = Vector2.zero;
                    Debug.Log("Spawned UI prefab at canvas center (no spawner position)");
                }
            }
            else
            {
                Debug.LogWarning("No target canvas found for UI prefab!");
                return;
            }
        }
        else
        {
            // 普通3D prefab处理
            if (spawnerTransform != null)
            {
                spawnedReward = Instantiate(rewardPrefab, spawnerTransform.position, spawnerTransform.rotation);
                Debug.Log("Spawned 3D prefab at: " + spawnerTransform.position);
            }
            else
            {
                Debug.LogWarning("No spawner transform found for 3D prefab!");
                return;
            }
        }

        if (spawnedReward != null)
        {
            Debug.Log("Successfully spawned reward prefab: " + spawnedReward.name);
        }
    }
    
    Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        if (targetCanvas == null) return Vector2.zero;
        
        // 获取Canvas的RectTransform
        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
        
        // 将世界坐标转换为屏幕坐标
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        
        // 将屏幕坐标转换为Canvas内的UI坐标
        Vector2 canvasPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, targetCanvas.worldCamera, out canvasPosition);
        
        return canvasPosition;
    }
    
    void FindSuitableCanvas()
    {
        // 尝试查找合适的Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        foreach (Canvas canvas in canvases)
        {
            // 优先选择Screen Space - Overlay模式的Canvas
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                targetCanvas = canvas;
                Debug.Log("Auto-selected Canvas: " + canvas.name);
                return;
            }
        }
        
        // 如果没有找到Overlay Canvas，选择第一个Canvas
        if (canvases.Length > 0)
        {
            targetCanvas = canvases[0];
            Debug.Log("Auto-selected first available Canvas: " + targetCanvas.name);
        }
        else
        {
            Debug.LogWarning("No Canvas found in scene for UI prefab!");
        }
    }
}
