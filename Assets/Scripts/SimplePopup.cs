using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimplePopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button closeButton;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private float animationDuration = 0.3f;

    private CanvasGroup canvasGroup;

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

        // Destroy the popup when animation is complete
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Clean up the event listener
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePopup);
    }
}