using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimplePopupSpawner : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private RectTransform rightInteractionZone;

    [Header("Difficulty Curve")]
    [SerializeField] private float initialSpawnInterval = 30f; // Seconds between spawns at start (longer = easier)
    [SerializeField] private float minimumSpawnInterval = 3f; // Shortest possible spawn interval (seconds)
    [SerializeField] private float difficultyRampTime = 180f; // Time (in seconds) until maximum difficulty
    [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Difficulty progression curve

    [Header("Spawn Limits")]
    [SerializeField] private int maxSimultaneousPopups = 3; // Maximum number of popups active at once

    [Header("Debug")]
    [SerializeField] private bool spawnOnKey = true;
    [SerializeField] private KeyCode spawnKey = KeyCode.P;
    [SerializeField] private bool showDebugInfo = true;

    // Private variables
    private float gameTimer = 0f;
    private float nextSpawnTime = 0f;
    private int activePopupCount = 0;

    // Sound variables
    [SerializeField] string errorPopupSound = "ErrorPopup";

    AudioManager audioManager;

    void Start()
    {
        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }

        // Set the first spawn time
        CalculateNextSpawnTime();

        // Begin the automatic spawn routine
        StartCoroutine(AutomaticSpawnRoutine());
    }

    void Update()
    {
        // Update game timer
        gameTimer += Time.deltaTime;

        // Debug key to manually spawn popups
        if (spawnOnKey && Input.GetKeyDown(spawnKey))
        {
            SpawnPopup();
        }

        // Show debug info
        if (showDebugInfo && Time.frameCount % 30 == 0) // Only update every 30 frames to reduce spam
        {
            float difficulty = CalculateDifficulty();
            float currentInterval = Mathf.Lerp(initialSpawnInterval, minimumSpawnInterval, difficulty);
            float timeUntilNextSpawn = nextSpawnTime - gameTimer;
            // Difficulty curve
            /*Debug.Log($"Game Time: {gameTimer:F1}s | " +
                     $"Difficulty: {difficulty:P0} | " +
                     $"Spawn Interval: {currentInterval:F1}s | " +
                     $"Next Spawn: {timeUntilNextSpawn:F1}s | " +
                     $"Active Popups: {activePopupCount}/{maxSimultaneousPopups}");*/
        }
    }

    private IEnumerator AutomaticSpawnRoutine()
    {
        while (true)
        {
            // Wait until it's time to spawn
            while (gameTimer < nextSpawnTime)
            {
                yield return null;
            }

            // Check if we can spawn more popups
            if (activePopupCount < maxSimultaneousPopups)
            {
                SpawnPopup();
            }

            // Calculate next spawn time
            CalculateNextSpawnTime();
        }
    }

    private void CalculateNextSpawnTime()
    {
        // Calculate current difficulty (0 to 1)
        float difficulty = CalculateDifficulty();

        // Calculate the current spawn interval based on difficulty
        float currentInterval = Mathf.Lerp(initialSpawnInterval, minimumSpawnInterval, difficulty);

        // Add some randomness to the interval (¡À20%)
        float randomVariation = Random.Range(0.8f, 1.2f);
        currentInterval *= randomVariation;

        // Set the next spawn time
        nextSpawnTime = gameTimer + currentInterval;
    }

    private float CalculateDifficulty()
    {
        // Calculate normalized progress (0 to 1) along the difficulty curve
        float normalizedTime = Mathf.Clamp01(gameTimer / difficultyRampTime);

        // Evaluate the difficulty curve at the current time
        return difficultyCurve.Evaluate(normalizedTime);
    }

    public void SpawnPopup()
    {
        if (popupPrefab == null || rightInteractionZone == null)
        {
            Debug.LogError("Popup prefab or RightInteractionZone not assigned!");
            return;
        }

        // Instantiate the popup as a child of the RightInteractionZone
        GameObject popup = Instantiate(popupPrefab, rightInteractionZone);
        audioManager.PlaySound(errorPopupSound);

        // Get the RectTransform of the popup
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            // Set the anchor to the center of the parent
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);

            // Position at center with some random offset to make it interesting
            float maxOffsetX = rightInteractionZone.rect.width / 2f - popupRect.rect.width / 2f;
            float maxOffsetY = rightInteractionZone.rect.height / 2f - popupRect.rect.height / 2f;

            float randomX = Random.Range(-maxOffsetX, maxOffsetX);
            float randomY = Random.Range(-maxOffsetY, maxOffsetY);
            popupRect.anchoredPosition = new Vector2(randomX, randomY);

            Debug.Log($"Popup spawned at position: {popupRect.anchoredPosition}");
        }

        // Increment active popup counter
        activePopupCount++;

        // Register for popup closed event
        SimplePopup popupScript = popup.GetComponent<SimplePopup>();
        if (popupScript != null)
        {
            popupScript.OnPopupClosed += HandlePopupClosed;
        }
    }

    public void HandlePopupClosed()
    {
        // Decrement active popup counter
        activePopupCount = Mathf.Max(0, activePopupCount - 1);
    }
}