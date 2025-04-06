using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class SimplePopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private float animationDuration = 0.3f;

    [Header("Popup Behavior")]
    [SerializeField] private bool canBeDragged = true;
    [SerializeField] private float autoCloseTime = 0f; // Set to 0 to disable auto-close

    // Event that fires when the popup is closed
    public event Action OnPopupClosed;

    private CanvasGroup canvasGroup;
    private bool isDragging = false;
    private Vector2 dragOffset;

    void Awake()
    {
        // Get or add a CanvasGroup component for fading
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Set initial state
        canvasGroup.alpha = 0f;

        // Add click event to the close button
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);

        // Start the appearance animation
        StartCoroutine(AppearAnimation());
    }

    // Animation for when the popup appears
    private IEnumerator AppearAnimation()
    {
        // Starting scale and alpha
        popupRect.localScale = Vector3.zero;

        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            // Calculate progress (0 to 1)
            float progress = elapsedTime / animationDuration;

            // Apply scale and alpha
            popupRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);
            canvasGroup.alpha = progress;

            // Wait for next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we end at exactly the target values
        popupRect.localScale = Vector3.one;
        canvasGroup.alpha = 1f;

        // If auto-close is enabled, start the timer
        if (autoCloseTime > 0)
        {
            StartCoroutine(AutoCloseRoutine());
        }
    }

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseTime);
        ClosePopup();
    }

    // Close the popup with an animation
    public void ClosePopup()
    {
        StartCoroutine(CloseAnimation());
    }

    private IEnumerator CloseAnimation()
    {
        float elapsedTime = 0f;
        Vector3 startScale = popupRect.localScale;
        float startAlpha = canvasGroup.alpha;

        while (elapsedTime < animationDuration)
        {
            // Calculate progress (0 to 1)
            float progress = elapsedTime / animationDuration;

            // Apply scale and alpha
            popupRect.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);

            // Wait for next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Invoke the closed event before destroying the object
        OnPopupClosed?.Invoke();

        // Destroy the popup when animation is complete
        Destroy(gameObject);
    }

    // Dragging functionality
    public void OnBeginDrag()
    {
        if (!canBeDragged) return;

        isDragging = true;
        dragOffset = popupRect.anchoredPosition - (Vector2)Input.mousePosition;
    }

    public void OnDrag()
    {
        if (!isDragging) return;

        popupRect.anchoredPosition = (Vector2)Input.mousePosition + dragOffset;
    }

    public void OnEndDrag()
    {
        isDragging = false;
    }

    void OnDestroy()
    {
        // Clean up the event listener
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePopup);
    }
}