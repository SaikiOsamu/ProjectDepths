using UnityEngine;
using TMPro;
using System.Collections;

public class TMPGlitchEffect : MonoBehaviour
{
    [SerializeField] private float glitchIntensity = 0.01f;
    [SerializeField] private float glitchInterval = 0.1f;
    [SerializeField] private float colorGlitchChance = 0.3f;
    
    private TextMeshProUGUI tmpText;
    private Vector3 originalPosition;
    private Color originalColor;
    
    void Start()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        
        if (tmpText == null)
        {
            tmpText = GetComponent<TextMeshProUGUI>();
        }
        
        if (tmpText != null)
        {
            originalPosition = tmpText.transform.localPosition;
            originalColor = tmpText.color;
            StartCoroutine(GlitchEffect());
        }
        else
        {
            Debug.LogError("没有找到TextMeshPro组件！");
        }
    }
    
    IEnumerator GlitchEffect()
    {
        while (true)
        {
            // 随机决定是否应用效果
            if (Random.value < 0.2f)
            {
                // 位置抖动
                Vector3 glitchOffset = new Vector3(
                    Random.Range(-glitchIntensity, glitchIntensity),
                    Random.Range(-glitchIntensity, glitchIntensity),
                    0
                );
                
                tmpText.transform.localPosition = originalPosition + glitchOffset;
                
                // 颜色变化
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
                
                // 短暂等待
                yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
                
                // 恢复正常
                tmpText.transform.localPosition = originalPosition;
                tmpText.color = originalColor;
            }
            
            // 等待下一次效果
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