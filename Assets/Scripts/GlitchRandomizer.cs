using UnityEngine;

public class GlitchRandomizer : MonoBehaviour
{
    private Renderer rend;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        rend = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();

        // Get current property values
        rend.GetPropertyBlock(propBlock);

        // Set a random seed value
        propBlock.SetFloat("_GlitchSeed", Random.Range(0f, 1000f));

        // Apply the property block to the renderer
        rend.SetPropertyBlock(propBlock);
    }
}