using UnityEngine;
using System.Collections;

public class HandUIAnimation : MonoBehaviour
{
    // Animation parameters
    [Header("Animation Settings")]
    [SerializeField] private string animationTriggerName = "PlayAnimation";
    [SerializeField] private float animationDuration = 1.67f; // 100 frames at 60fps

    // Reference to the animator component
    private Animator animator;

    // Event that will be fired when the animation completes
    public delegate void AnimationCompleteDelegate();
    public event AnimationCompleteDelegate OnAnimationComplete;

    private void Awake()
    {
        // Get reference to the animator component
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("No Animator component found on HandUIAnimation!");
        }
    }

    // Call this method to play the animation
    public void PlayAnimation()
    {
        if (animator != null)
        {
            // Make sure the animator works in unscaled time (for pause menu)
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            // Play the animation
            animator.SetTrigger(animationTriggerName);

            // Start the coroutine to wait for animation completion
            StartCoroutine(WaitForAnimationToComplete());
        }
        else
        {
            // If no animator, just fire the event immediately
            OnAnimationComplete?.Invoke();
        }
    }

    private IEnumerator WaitForAnimationToComplete()
    {
        // Wait for the animation to complete
        yield return new WaitForSecondsRealtime(animationDuration);

        // Fire the event to notify listeners that animation is complete
        OnAnimationComplete?.Invoke();
    }

    // You can add this method to your animation event keyframe
    // to call from the animation timeline directly
    public void AnimationCompleted()
    {
        OnAnimationComplete?.Invoke();
    }
}