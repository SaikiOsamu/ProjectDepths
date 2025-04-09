using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class HandUIPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform uiPanel;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private string animationClipName = "IntroAnimation";

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1.67f; // 140 frames at 30fps ¡Ö 4.67 seconds
    [SerializeField] private Ease popupEase = Ease.OutBack;

    [Header("Optional Effects")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private bool useScale = true;
    [SerializeField] private float popupDuration = 0.3f; // How long the popup animation takes
    [SerializeField] private bool playAudioOnPopup = true;
    [SerializeField] private string popupSoundName = "UIPopup";

    private AudioManager audioManager;

    private void Awake()
    {
        // Get or add CanvasGroup if needed
        if (fadeIn && canvasGroup == null)
        {
            canvasGroup = uiPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = uiPanel.gameObject.AddComponent<CanvasGroup>();
        }

        // Initially hide the panel but keep it in its final position
        if (uiPanel != null)
        {
            // Only hide with alpha/scale, not position
            if (fadeIn && canvasGroup != null)
                canvasGroup.alpha = 0f;

            if (useScale)
                uiPanel.localScale = Vector3.zero;
            else if (fadeIn && canvasGroup != null)
                canvasGroup.alpha = 0f;
            else
                uiPanel.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Get audio manager reference
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("No audio manager found");
        }
        audioManager.PlaySound("HandPopup");
        // Begin the sequence
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // Play the intro animation if we have an animator
        if (introAnimator != null)
        {
            introAnimator.Play(animationClipName);

            // Wait for the animation to complete (140 frames)
            yield return new WaitForSeconds(animationDuration);
        }

        // Immediately show the popup
        ShowUIPanel();
    }

    private void ShowUIPanel()
    {
        if (uiPanel == null) return;

        // Create a sequence of animations
        Sequence popupSequence = DOTween.Sequence();

        // No position change, only reveal effects

        // Add fade-in if enabled
        if (fadeIn && canvasGroup != null)
        {
            popupSequence.Append(canvasGroup.DOFade(1f, popupDuration));
        }

        // Add scale if enabled
        if (useScale)
        {
            popupSequence.Join(uiPanel.DOScale(Vector3.one, popupDuration).SetEase(popupEase));
        }
        // If neither fade nor scale is used, just activate the panel
        else if (!fadeIn)
        {
            uiPanel.gameObject.SetActive(true);
        }

        // Play sound when animation starts
        if (playAudioOnPopup && audioManager != null)
        {
            audioManager.PlaySound(popupSoundName);
        }

        // Start the sequence
        popupSequence.Play();
    }

    // Public method to hide the panel with animation (can be called from buttons, etc.)
    public void HideUIPanel()
    {
        if (uiPanel == null) return;

        Sequence hideSequence = DOTween.Sequence();

        // Add fade-out if enabled
        if (fadeIn && canvasGroup != null)
        {
            hideSequence.Append(canvasGroup.DOFade(0f, popupDuration));
        }

        // Add scale if enabled
        if (useScale)
        {
            hideSequence.Join(uiPanel.DOScale(Vector3.zero, popupDuration).SetEase(Ease.InBack));
        }

        // Play the sequence
        hideSequence.Play();
    }

    // Clean up DOTween animations when the object is destroyed
    private void OnDestroy()
    {
        DOTween.Kill(uiPanel);
        if (canvasGroup != null)
            DOTween.Kill(canvasGroup);
    }
}