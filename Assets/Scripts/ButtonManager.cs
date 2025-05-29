using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField] Button[] buttons;
    [SerializeField] TextMeshProUGUI LoadText;
    [SerializeField] TextMeshProUGUI FirstDigit;
    [SerializeField] TextMeshProUGUI SecondDigit;
    [SerializeField] TextMeshProUGUI ThirdDigit;

    [SerializeField] private float codeTimeLimit = 5f; // Time limit in seconds to input the code

    [Header("Prefab Spawning")]
    [SerializeField] private GameObject rewardPrefab; // 正确时生成的prefab
    [SerializeField] private GameObject punishPrefab; // 错误/过期时生成的prefab
    [SerializeField] private Transform spawnerTransform; // spawner位置标记
    [SerializeField] private Canvas targetCanvas; // UI prefab需要的Canvas父对象
    [SerializeField] private bool isUIPrefab = true; // 是否为UI prefab

    [Header("Debug/Test")]
    [SerializeField] private bool enableTestKey = true; // 是否启用测试按键
    [SerializeField] private KeyCode testRewardKey = KeyCode.R; // 测试奖励按键
    [SerializeField] private KeyCode testPunishKey = KeyCode.T; // 测试惩罚按键

    private string targetCode = "";
    private string playerInput = "";
    private int currentDigitPosition = 0; // Track which digit we're currently inputting
    private Coroutine timeoutCoroutine; // Reference to track the timeout coroutine
    private Coroutine randomTriggerCoroutine; // Reference to track the random trigger coroutine
    private bool isCodeActive = false; // Flag to indicate if a code is currently active
    private bool isProcessingResult = false; // Flag to prevent input during result animation

    // Colors for feedback
    private Color correctColor = Color.green;
    private Color incorrectColor = Color.red;
    private Color defaultColor = Color.white;

    // Sound Variables
    [SerializeField] string passwordEnterSound = "PasswordEnter";
    [SerializeField] string passwordCorrectSound = "PasswordCorrect";
    [SerializeField] string passwordWrongSound = "PasswordWrong";

    AudioManager audioManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }

        // 如果是UI prefab但没有指定Canvas，自动查找合适的Canvas
        if (isUIPrefab && targetCanvas == null)
        {
            FindSuitableCanvas();
        }

        LoadText.text = "LOADING...";
        LoadText.color = defaultColor;
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";

        for (int i = 0; i < buttons.Length; i++)
        {
            int digit = i;
            buttons[i].onClick.AddListener(() => OnDigitPressed(digit));
        }

        // Start the random trigger coroutine
        randomTriggerCoroutine = StartCoroutine(RandomTrigger());
    }

    void Update()
    {
        // 测试按键功能
        if (enableTestKey)
        {
            if (Input.GetKeyDown(testRewardKey))
            {
                Debug.Log("Test reward key pressed! Spawning reward prefab...");
                SpawnPrefab(rewardPrefab);
            }
            
            if (Input.GetKeyDown(testPunishKey))
            {
                Debug.Log("Test punish key pressed! Spawning punish prefab...");
                SpawnPrefab(punishPrefab);
            }
        }
    }

    IEnumerator RandomTrigger()
    {
        while (true)
        {
            if (!isCodeActive && !isProcessingResult) // Only generate a new code if no code is active and not processing
            {
                float waitTime = Random.Range(5f, 10f);
                yield return new WaitForSeconds(waitTime);

                // Double check before triggering a new code
                if (!isCodeActive && !isProcessingResult)
                {
                    StartCoroutine(TriggerPasswordEvent());
                }
            }
            else
            {
                // If a code is active or result is processing, just check again shortly
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    IEnumerator TriggerPasswordEvent()
    {
        // Set the flag to indicate a code is active
        isCodeActive = true;
        isProcessingResult = false;

        playerInput = "";
        targetCode = "";
        currentDigitPosition = 0; // Reset digit position
        FirstDigit.color = SecondDigit.color = ThirdDigit.color = defaultColor;

        for (int i = 0; i < 3; i++)
        {
            int digit = Random.Range(0, 10);
            targetCode += digit.ToString();

            if (i == 0) FirstDigit.text = digit.ToString();
            else if (i == 1) SecondDigit.text = digit.ToString();
            else ThirdDigit.text = digit.ToString();
        }

        LoadText.text = "";

        // Start the timeout countdown
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }
        timeoutCoroutine = StartCoroutine(CodeTimeout());

        yield break;
    }

    // Coroutine for timing out if player doesn't complete the code
    IEnumerator CodeTimeout()
    {
        yield return new WaitForSeconds(codeTimeLimit);

        // Only expire if code entry hasn't been completed or failed yet
        if (isCodeActive && !isProcessingResult && targetCode != "" && currentDigitPosition < 3)
        {
            // Immediately mark as processing result to prevent further input
            isProcessingResult = true;
            isCodeActive = false;

            StartCoroutine(ExpiredInput());
        }
    }

    void OnDigitPressed(int digit)
    {
        // Prevent input if: code not active, processing result, or no target code
        if (targetCode == "" || !isCodeActive || isProcessingResult)
            return;

        audioManager.PlaySound(passwordEnterSound);

        // Get the expected digit at the current position
        char expectedDigit = targetCode[currentDigitPosition];
        bool isCorrect = (digit.ToString() == expectedDigit.ToString());

        if (isCorrect)
        {
            // Update the color of the current digit text to green
            if (currentDigitPosition == 0)
                FirstDigit.color = correctColor;
            else if (currentDigitPosition == 1)
                SecondDigit.color = correctColor;
            else if (currentDigitPosition == 2)
                ThirdDigit.color = correctColor;

            // Add the digit to player input
            playerInput += digit.ToString();

            // Move to the next digit
            currentDigitPosition++;

            // Check if the complete code has been entered correctly
            if (currentDigitPosition == 3)
            {
                // Set flags to prevent further input
                isProcessingResult = true;
                isCodeActive = false;

                // Stop the timeout coroutine since we completed the code
                if (timeoutCoroutine != null)
                {
                    StopCoroutine(timeoutCoroutine);
                    timeoutCoroutine = null;
                }

                StartCoroutine(CorrectInput());
            }
        }
        else
        {
            // Handle wrong input at the current digit position
            playerInput += digit.ToString(); // Still record what they pressed

            // Set flags to prevent further input
            isProcessingResult = true;
            isCodeActive = false;

            // Update the color of the current digit text to red
            if (currentDigitPosition == 0)
                FirstDigit.color = incorrectColor;
            else if (currentDigitPosition == 1)
                SecondDigit.color = incorrectColor;
            else if (currentDigitPosition == 2)
                ThirdDigit.color = incorrectColor;

            // Stop the timeout coroutine since we failed
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }

            // Trigger wrong input sequence
            StartCoroutine(WrongInput());
        }
    }

    IEnumerator CorrectInput()
    {
        GameManager.instance.GainScore(100);

        // 生成奖励prefab
        SpawnPrefab(rewardPrefab);

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = correctColor;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }

        LoadText.color = correctColor;
        LoadText.text = "CORRECT!";

        audioManager.PlaySound(passwordCorrectSound);

        yield return new WaitForSeconds(2f);
        ResetCodeState();
    }

    IEnumerator WrongInput()
    {
        GameManager.instance.GainScore(-50);
        //GameManager.instance.Punish();

        // 生成惩罚prefab
        SpawnPrefab(punishPrefab);

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = incorrectColor;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }

        LoadText.color = incorrectColor;
        LoadText.text = "FALSE!";

        audioManager.PlaySound(passwordWrongSound);

        yield return new WaitForSeconds(2f);
        ResetCodeState();
    }

    IEnumerator ExpiredInput()
    {
        GameManager.instance.GainScore(-50); // Same penalty as wrong input
        //GameManager.instance.Punish();

        // 生成惩罚prefab
        SpawnPrefab(punishPrefab);

        for (int i = 0; i < 4; i++)
        {
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = incorrectColor;
            yield return new WaitForSeconds(0.2f);
            FirstDigit.color = SecondDigit.color = ThirdDigit.color = Color.clear;
            yield return new WaitForSeconds(0.2f);
        }

        LoadText.color = incorrectColor;
        LoadText.text = "Expired!"; // Different message for timeout
        audioManager.PlaySound(passwordWrongSound);
        yield return new WaitForSeconds(2f);
        ResetCodeState();
    }

    void SpawnPrefab(GameObject prefabToSpawn)
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("No prefab assigned for spawning!");
            return;
        }

        GameObject spawnedPrefab = null;

        if (isUIPrefab)
        {
            // UI prefab处理
            if (targetCanvas != null)
            {
                // 在Canvas下生成UI prefab
                spawnedPrefab = Instantiate(prefabToSpawn, targetCanvas.transform);
                
                // 获取RectTransform来设置UI位置
                RectTransform prefabRect = spawnedPrefab.GetComponent<RectTransform>();
                if (prefabRect != null && spawnerTransform != null)
                {
                    // 将世界坐标转换为Canvas内的UI坐标
                    Vector2 uiPosition = WorldToCanvasPosition(spawnerTransform.position);
                    prefabRect.anchoredPosition = uiPosition;
                    
                    Debug.Log($"Spawned UI prefab '{prefabToSpawn.name}' at canvas position: {uiPosition}");
                }
                else if (prefabRect != null)
                {
                    // 如果没有spawner位置，放在Canvas中心
                    prefabRect.anchoredPosition = Vector2.zero;
                    Debug.Log($"Spawned UI prefab '{prefabToSpawn.name}' at canvas center (no spawner position)");
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
                spawnedPrefab = Instantiate(prefabToSpawn, spawnerTransform.position, spawnerTransform.rotation);
                Debug.Log($"Spawned 3D prefab '{prefabToSpawn.name}' at: {spawnerTransform.position}");
            }
            else
            {
                Debug.LogWarning("No spawner transform found for 3D prefab!");
                return;
            }
        }

        if (spawnedPrefab != null)
        {
            Debug.Log($"Successfully spawned prefab: {spawnedPrefab.name}");
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
                Debug.Log("ButtonManager: Auto-selected Canvas: " + canvas.name);
                return;
            }
        }
        
        // 如果没有找到Overlay Canvas，选择第一个Canvas
        if (canvases.Length > 0)
        {
            targetCanvas = canvases[0];
            Debug.Log("ButtonManager: Auto-selected first available Canvas: " + targetCanvas.name);
        }
        else
        {
            Debug.LogWarning("ButtonManager: No Canvas found in scene for UI prefab!");
        }
    }

    // Helper method to reset all code-related state
    private void ResetCodeState()
    {
        LoadText.color = defaultColor;
        LoadText.text = "LOADING...";
        FirstDigit.text = SecondDigit.text = ThirdDigit.text = "";
        targetCode = "";
        playerInput = "";
        currentDigitPosition = 0;

        // Ensure timeout coroutine is stopped
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        // Reset the code active flag, allowing new codes to be generated
        isCodeActive = false;
        isProcessingResult = false;
    }

    private void OnDestroy()
    {
        // Clean up any active coroutines when this object is destroyed
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }

        if (randomTriggerCoroutine != null)
        {
            StopCoroutine(randomTriggerCoroutine);
        }
    }
}