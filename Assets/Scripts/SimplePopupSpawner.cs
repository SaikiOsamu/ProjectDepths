using UnityEngine;
using UnityEngine.UI;

public class SimplePopupSpawner : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private RectTransform rightInteractionZone;

    [Header("Spawn Settings")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private bool spawnOnKey = true;
    [SerializeField] private KeyCode spawnKey = KeyCode.P;

    void Start()
    {
        // If we should spawn a popup on start
        if (spawnOnStart)
        {
            SpawnPopup();
        }
    }

    void Update()
    {
        // If we should spawn a popup on key press
        if (spawnOnKey && Input.GetKeyDown(spawnKey))
        {
            SpawnPopup();
        }
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

        // Get the RectTransform of the popup
        RectTransform popupRect = popup.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            // Set the anchor to the center of the parent
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);

            // Position at center with some random offset to make it interesting
            float randomX = Random.Range(-100f, 100f);
            float randomY = Random.Range(-100f, 100f);
            popupRect.anchoredPosition = new Vector2(randomX, randomY);

            Debug.Log("Popup spawned at position: " + popupRect.anchoredPosition);
        }
    }
}