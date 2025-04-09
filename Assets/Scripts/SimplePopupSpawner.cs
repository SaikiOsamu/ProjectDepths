using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimplePopupSpawner : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject[] popupPrefabs; // Array of different popup prefabs
    [SerializeField] private RectTransform rightInteractionZone;

    [Header("Difficulty Curve")]
    [SerializeField] private float initialSpawnInterval = 20f; // Reduced from 30s
    [SerializeField] private float minimumSpawnInterval = 1.5f; // Reduced from 3s
    [SerializeField] private float difficultyRampTime = 120f; // Reduced from 180s
    [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Spawn Limits")]
    [SerializeField] private int maxSimultaneousPopups = 6; // Increased from 3

    [Header("Debug")]
    [SerializeField] private bool spawnOnKey = true;
    [SerializeField] private KeyCode spawnKey = KeyCode.P;
    [SerializeField] private bool showDebugInfo = true;

    // Private variables
    private float gameTimer = 0f;
    private float nextSpawnTime = 0f;
    private int activePopupCount = 0;

    // Sound variables
    [SerializeField] string[] popupSounds = { "ErrorPopup", "AlertPopup", "NotificationPopup" };

    AudioManager audioManager;

    void Start()
    {
        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }

        // Verify we have popup prefabs
        if (popupPrefabs == null || popupPrefabs.Length == 0)
        {
            Debug.LogError("No popup prefabs assigned!");
            popupPrefabs = new GameObject[1] { null };
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
            SpawnRandomPopup();
        }

        // Show debug info
        if (showDebugInfo && Time.frameCount % 30 == 0) // Only update every 30 frames to reduce spam
        {
            float difficulty = CalculateDifficulty();
            float currentInterval = Mathf.Lerp(initialSpawnInterval, minimumSpawnInterval, difficulty);
            float timeUntilNextSpawn = nextSpawnTime - gameTimer;
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
                SpawnRandomPopup();
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

        // Make the curve steeper for a more challenging experience
        return Mathf.Pow(difficultyCurve.Evaluate(normalizedTime), 0.7f);
    }

    public void SpawnRandomPopup()
    {
        if (popupPrefabs.Length == 0 || rightInteractionZone == null)
        {
            Debug.LogError("Popup prefabs or RightInteractionZone not assigned!");
            return;
        }

        // Select a random popup prefab
        int prefabIndex = Random.Range(0, popupPrefabs.Length);
        GameObject selectedPrefab = popupPrefabs[prefabIndex];

        if (selectedPrefab == null)
        {
            Debug.LogError("Selected popup prefab is null!");
            return;
        }

        // Instantiate the popup as a child of the RightInteractionZone
        GameObject popup = Instantiate(selectedPrefab, rightInteractionZone);

        // Play a random popup sound or the appropriate one for this type
        int soundIndex = Mathf.Min(prefabIndex, popupSounds.Length - 1);
        string soundToPlay = popupSounds[soundIndex];
        //audioManager.PlaySound(soundToPlay); // I give sound to each prefab to play on awake.

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

            Debug.Log($"Popup type {prefabIndex} spawned at position: {popupRect.anchoredPosition}");
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