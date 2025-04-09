using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu UI")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI Animation")]
    [SerializeField] private RectTransform uiPanel;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private string animationClipName = "IntroAnimation";
    [SerializeField] private float animationDuration = 1.67f;
    [SerializeField] private Ease popupEase = Ease.OutBack;

    [Header("Optional Effects")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private bool useScale = true;
    [SerializeField] private float popupDuration = 0.3f;
    [SerializeField] private bool playAudioOnPopup = true;

    [Header("Input Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    [Header("Audio Settings")]
    [SerializeField] private string playSceneBGM = "BGM_PlayScene";
    [SerializeField] private string pauseMenuBGM = "BGM_PauseMenu";
    [SerializeField] private float audioFadeDuration = 0.15f;

    private bool isPaused = false;

    // Sound
    [SerializeField] string hoverOverSound = "ButtonHover";
    [SerializeField] string clickButtonSound = "ButtonClick";
    [SerializeField] string popupSoundName = "HandPopup";

    AudioManager audioManager;
    public GameObject HandUIAnimation;

    private void Awake()
    {
        // Get or add CanvasGroup if needed
        if (fadeIn && canvasGroup == null && uiPanel != null)
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

        // Ensure the pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Set up button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        // Play both BGMs at start
        audioManager.PlaySound(playSceneBGM);
        audioManager.PlaySound(pauseMenuBGM);

        // Set initial audio state - playscene BGM normal, pause BGM silent
        SetAudioState(false);
    }

    private void SetAudioState(bool isPaused)
    {
        if (audioManager == null) return;

        // We'll use a coroutine for smooth transitions
        StartCoroutine(TransitionAudio(isPaused));
    }

    private IEnumerator TransitionAudio(bool isPaused)
    {
        // Find audio sources by searching for all sources under the AudioManager
        Sound playSceneSound = null;
        Sound pauseMenuSound = null;

        // Get all Sound components by using reflection or traversing the AudioManager structure
        for (int i = 0; i < audioManager.transform.childCount; i++)
        {
            Transform child = audioManager.transform.GetChild(i);
            string name = child.name;

            if (name.Contains(playSceneBGM))
            {
                AudioSource source = child.GetComponent<AudioSource>();
                if (source != null)
                {
                    // Here's a custom way to control the volume since we don't have direct access to Sound class
                    if (isPaused)
                    {
                        // Fade out play scene BGM
                        float startVolume = source.volume;
                        float elapsed = 0f;

                        while (elapsed < audioFadeDuration)
                        {
                            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / audioFadeDuration);
                            elapsed += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        source.volume = 0f;
                    }
                    else
                    {
                        // Fade in play scene BGM
                        float startVolume = source.volume;
                        float elapsed = 0f;

                        while (elapsed < audioFadeDuration)
                        {
                            source.volume = Mathf.Lerp(startVolume, 1f, elapsed / audioFadeDuration);
                            elapsed += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        source.volume = 1f; // Match inspector value
                    }
                }
            }

            if (name.Contains(pauseMenuBGM))
            {
                AudioSource source = child.GetComponent<AudioSource>();
                if (source != null)
                {
                    if (isPaused)
                    {
                        // Fade in pause menu BGM
                        float startVolume = source.volume;
                        float elapsed = 0f;

                        while (elapsed < audioFadeDuration)
                        {
                            source.volume = Mathf.Lerp(startVolume, 1f, elapsed / audioFadeDuration);
                            elapsed += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        source.volume = 1f; // Match inspector value
                    }
                    else
                    {
                        // Fade out pause menu BGM
                        float startVolume = source.volume;
                        float elapsed = 0f;

                        while (elapsed < audioFadeDuration)
                        {
                            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / audioFadeDuration);
                            elapsed += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        source.volume = 0f;
                    }
                }
            }
        }
    }

    // Play sound when mouse hovering UI 
    public void OnMouseOver()
    {
        audioManager.PlaySound(hoverOverSound);
    }

    private void Update()
    {
        // Check for pause key press
        if (Input.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        // Set the time scale to 0 to freeze the game
        Time.timeScale = 0f;
        isPaused = true;

        // Transition audio to paused state (fade in pause menu music, fade out gameplay music)
        SetAudioState(true);

        // Make sure pauseMenuPanel is active but UI elements start invisible
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);

            if (HandUIAnimation != null)
            {
                // Use the hand animation if available
                HandUIAnimation.SetActive(true);

                // Ensure buttons are inactive
                if (resumeButton != null) resumeButton.gameObject.SetActive(false);
                if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);

                // Play animation
                Animator animator = HandUIAnimation.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                    animator.SetTrigger("PlayAnimation");
                }
                audioManager.PlaySound("HandPopup");

                if (uiPanel != null)
                {
                    StartCoroutine(ShowUIAfterHandAnimation());
                }
            }
            else if (uiPanel != null)
            {
                // Use DOTween animation for UI panel
                PlayIntroAnimation();
            }
        }

        Debug.Log("Game paused");
    }

    private IEnumerator ShowUIAfterHandAnimation()
    {
        // Wait for the animation to complete using unscaled time
        yield return new WaitForSecondsRealtime(animationDuration);

        // Activate buttons
        if (resumeButton != null) resumeButton.gameObject.SetActive(true);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(true);

        // Show the panel with tweening effects
        ShowUIPanel();
    }

    private void PlayIntroAnimation()
    {
        if (uiPanel == null) return;

        if (introAnimator != null)
        {
            // Make sure the animator works in unscaled time (for pause menu)
            introAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            introAnimator.Play(animationClipName);
            StartCoroutine(ShowUIAfterAnimation());
        }
        else
        {
            // If no animator, just show UI with tweening
            ShowUIPanel();
        }
    }

    private IEnumerator ShowUIAfterAnimation()
    {
        // Wait for the animation to complete using unscaled time
        yield return new WaitForSecondsRealtime(animationDuration);

        // Show the panel with tweening effects
        ShowUIPanel();
    }

    private void ShowUIPanel()
    {
        if (uiPanel == null) return;

        // Ensure the panel is active before animating
        uiPanel.gameObject.SetActive(true);

        // Create a sequence of animations
        Sequence popupSequence = DOTween.Sequence();

        // Configure for unscaled time since game is paused
        popupSequence.SetUpdate(true);

        // Add fade-in if enabled
        if (fadeIn && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            popupSequence.Append(canvasGroup.DOFade(1f, popupDuration));
        }

        // Add scale if enabled
        if (useScale)
        {
            uiPanel.localScale = Vector3.zero;
            popupSequence.Join(uiPanel.DOScale(Vector3.one, popupDuration).SetEase(popupEase));
        }

        // Play sound when animation starts
        if (playAudioOnPopup && audioManager != null)
        {
            audioManager.PlaySound(popupSoundName);
        }

        // Start the sequence
        popupSequence.Play();
    }

    private void HideUIPanel()
    {
        if (uiPanel == null) return;

        Sequence hideSequence = DOTween.Sequence();

        // Configure for unscaled time since game is paused
        hideSequence.SetUpdate(true);

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

    public void ResumeGame()
    {
        // Play Click sound
        audioManager.PlaySound(clickButtonSound);

        // Transition audio to gameplay state
        SetAudioState(false);

        // If using the built-in UI animation, hide it with animation
        if (HandUIAnimation == null && uiPanel != null)
        {
            HideUIPanel();
            StartCoroutine(ResumeAfterAnimation());
        }
        else
        {
            // If using the external HandUIAnimation, just deactivate it
            if (HandUIAnimation != null)
            {
                HandUIAnimation.SetActive(false);
            }

            // Hide the pause menu immediately
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }

            // Resume the game
            Time.timeScale = 1f;
            isPaused = false;
        }

        Debug.Log("Game resumed");
    }

    private IEnumerator ResumeAfterAnimation()
    {
        // Wait for hide animation to complete
        yield return new WaitForSecondsRealtime(popupDuration);

        // Hide the pause menu
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Resume the game
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ReturnToMainMenu()
    {
        // Play Click sound
        audioManager.PlaySound(clickButtonSound);

        audioManager.StopSound(pauseMenuBGM);

        // Make sure to reset time scale before changing scenes
        Time.timeScale = 1f;
        isPaused = false;

        // Load the main menu scene
        StartCoroutine(LoadSceneWithDelay(mainMenuSceneName));
        Debug.Log("Returning to main menu");
    }

    private IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Wait a small amount of time for the click sound to play
        yield return new WaitForSecondsRealtime(0.2f);
        audioManager.PlaySound("BGM_MainMenu");

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    // This ensures time scale is reset if the script is disabled or destroyed
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    // Clean up DOTween animations when the object is destroyed
    private void OnDestroy()
    {
        if (uiPanel != null)
            DOTween.Kill(uiPanel);
        if (canvasGroup != null)
            DOTween.Kill(canvasGroup);
    }
}