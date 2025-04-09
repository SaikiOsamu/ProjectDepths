using UnityEngine;
using TMPro;
using System.Collections;

public class TMPGlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    [SerializeField] private float glitchIntensity = 0.01f;
    [SerializeField] private float glitchInterval = 0.1f;
    [SerializeField] private float colorGlitchChance = 0.3f;

    [Header("Randomness")]
    [SerializeField] private float glitchActivationChance = 0.1f; // 10% chance for this object to glitch

    private TMP_Text tmpText;
    private Vector3 originalPosition;
    private Color originalColor;

    void Start()
    {
        // Get the TMP_Text component (base class that both TextMeshPro and TextMeshProUGUI inherit from)
        tmpText = GetComponent<TMP_Text>();

        if (tmpText == null)
        {
            Debug.LogError("No TextMeshPro Text component found!");
            return;
        }

        originalPosition = tmpText.transform.localPosition;
        originalColor = tmpText.color;

        // Add random delay before starting the coroutine to desynchronize objects
        float randomDelay = Random.Range(0f, 1f);
        StartCoroutine(GlitchEffectWithDelay(randomDelay));
    }

    IEnumerator GlitchEffectWithDelay(float delay)
    {
        // Wait for random delay to stagger the start times
        yield return new WaitForSeconds(delay);

        // Start the main glitch effect
        StartCoroutine(GlitchEffect());
    }

    IEnumerator GlitchEffect()
    {
        while (true)
        {
            // First check if this particular object should attempt to glitch at all
            if (Random.value < glitchActivationChance)
            {
                // If we passed the activation check, now determine if we apply the effect
                if (Random.value < 0.2f)
                {
                    // Position jitter
                    Vector3 glitchOffset = new Vector3(
                        Random.Range(-glitchIntensity, glitchIntensity),
                        Random.Range(-glitchIntensity, glitchIntensity),
                        0
                    );

                    tmpText.transform.localPosition = originalPosition + glitchOffset;

                    // Color variation
                    if (Random.value < colorGlitchChance)
                    {
                        Color glitchColor = new Color(
                            originalColor.r + Random.Range(-0.1f, 0.1f),
                            originalColor.g + Random.Range(-0.1f, 0.1f),
                            originalColor.b + Random.Range(-0.1f, 0.1f),
                            originalColor.a
                        );

                        tmpText.color = glitchColor;
                    }

                    // Brief wait
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));

                    // Reset to normal
                    tmpText.transform.localPosition = originalPosition;
                    tmpText.color = originalColor;
                }
            }

            // Wait for next effect attempt
            yield return new WaitForSeconds(glitchInterval);
        }
    }

    void OnDisable()
    {
        if (tmpText != null)
        {
            tmpText.transform.localPosition = originalPosition;
            tmpText.color = originalColor;
        }
    }
}