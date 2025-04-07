using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuinPrefab : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeDuration = 0.5f; // Time to fade out at the end

    [Header("Physics Settings")]
    [SerializeField] private string playerLayerName = "Player"; // Layer name for the player
    [SerializeField] private string ruinLayerName = "Ruins"; // Layer name for ruins

    private float lifetime;
    private float currentLifetime = 0f;
    private bool isFading = false;

    // This gets called by DestroyableObject script when instantiated
    public void Initialize(float duration)
    {
        lifetime = duration - fadeDuration; // Reserve time for fading
    }

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Get the BoxCollider2D component
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            // Make sure it's not a trigger (so it can land on other cubes)
            boxCollider.isTrigger = false;

            // Ignore collisions with the player layer
            int playerLayer = LayerMask.NameToLayer(playerLayerName);
            int ruinLayer = LayerMask.NameToLayer(ruinLayerName);
            int currentLayer = gameObject.layer;

            // Set this object to the Ruins layer if it's not already
            if (ruinLayer >= 0 && currentLayer != ruinLayer)
            {
                gameObject.layer = ruinLayer;
            }

            // Check if the player layer exists
            if (playerLayer >= 0) // Valid layer found
            {
                // Ignore collisions between player and ruins
                Physics2D.IgnoreLayerCollision(playerLayer, ruinLayer, true);
            }
            else
            {
                Debug.LogWarning("Player layer '" + playerLayerName + "' not found! Make sure to set up your layers correctly.");
            }

            // Check if the ruins layer exists
            if (ruinLayer >= 0) // Valid layer found
            {
                // Ignore collisions between ruins and other ruins
                Physics2D.IgnoreLayerCollision(ruinLayer, ruinLayer, true);
            }
            else
            {
                Debug.LogWarning("Ruins layer '" + ruinLayerName + "' not found! Make sure to set up your layers correctly.");
            }
        }
    }

    void Update()
    {
        // Increment the current lifetime
        currentLifetime += Time.deltaTime;

        // Start fading when we reach the fade point
        if (currentLifetime >= lifetime && !isFading)
        {
            isFading = true;
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        // Get the starting color
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // Fully transparent

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // Calculate progress (0 to 1)
            float t = elapsed / fadeDuration;

            // Update the color with interpolated alpha
            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

            // Wait for next frame
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure we end with full transparency
        spriteRenderer.color = endColor;
    }
}