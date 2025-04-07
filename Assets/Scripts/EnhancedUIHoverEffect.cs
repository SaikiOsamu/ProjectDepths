using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EnhancedUIHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放效果")]
    [Tooltip("鼠标悬停时的缩放倍数")]
    [SerializeField] private float hoverScale = 1.2f;
    
    [Tooltip("鼠标点击时的缩放倍数")]
    [SerializeField] private float clickScale = 0.95f;
    
    [Tooltip("缩放变化的速度")]
    [SerializeField] private float scaleTransitionSpeed = 10f;
    
    [Tooltip("是否启用弹性效果")]
    [SerializeField] private bool useSpringEffect = true;
    
    [Header("发光效果")]
    [Tooltip("是否启用发光效果")]
    [SerializeField] private bool useGlowEffect = true;
    
    [Tooltip("发光效果组件")]
    [SerializeField] private Outline outlineEffect;
    
    [Tooltip("正常状态下的发光颜色")]
    [SerializeField] private Color normalGlowColor = new Color(1, 1, 1, 0); // 透明白色（无发光）
    
    [Tooltip("悬停状态下的发光颜色")]
    [SerializeField] private Color hoverGlowColor = new Color(1, 0.8f, 0.2f, 1); // 金黄色发光
    
    [Tooltip("点击状态下的发光颜色")]
    [SerializeField] private Color clickGlowColor = new Color(1, 0.5f, 0, 1); // 橙色发光
    
    [Tooltip("发光宽度")]
    [SerializeField] private Vector2 glowWidth = new Vector2(3, 3);
    
    [Header("颜色效果")]
    [Tooltip("是否启用颜色变化效果")]
    [SerializeField] private bool useColorEffect = true;
    
    [Tooltip("悬停状态的颜色")]
    [SerializeField] private Color hoverColor = new Color(1.2f, 1.2f, 1.2f, 1f); // 略微更亮
    
    [Tooltip("点击状态的颜色调整")]
    [SerializeField] [Range(0.1f, 1f)] private float clickDarkenAmount = 0.7f;
    
    [Tooltip("颜色变化速度")]
    [SerializeField] private float colorTransitionSpeed = 10f;
    
    // 内部变量
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;
    private bool isPressed = false;
    
    // 图形组件
    private Graphic[] graphics;
    private Color[] originalColors;
    private Color[] targetColors;
    
    private void Start()
    {
        // 记录原始大小
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        // 获取所有图形组件
        graphics = GetComponentsInChildren<Graphic>();
        originalColors = new Color[graphics.Length];
        targetColors = new Color[graphics.Length];
        
        // 保存所有原始颜色
        for (int i = 0; i < graphics.Length; i++)
        {
            originalColors[i] = graphics[i].color;
            targetColors[i] = originalColors[i];
        }
        
        // 如果启用发光效果但没有指定Outline组件
        if (useGlowEffect && outlineEffect == null)
        {
            // 尝试获取Outline组件
            outlineEffect = GetComponent<Outline>();
            
            // 如果没有，则添加一个
            if (outlineEffect == null)
            {
                outlineEffect = gameObject.AddComponent<Outline>();
                outlineEffect.effectColor = normalGlowColor;
                outlineEffect.effectDistance = Vector2.zero; // 初始无发光
            }
        }
        
        // 初始化发光效果
        if (useGlowEffect && outlineEffect != null)
        {
            outlineEffect.effectColor = normalGlowColor;
            outlineEffect.effectDistance = Vector2.zero;
        }
    }
    
    private void Update()
    {
        // 平滑过渡到目标大小
        if (useSpringEffect)
        {
            // 弹性效果
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleTransitionSpeed);
        }
        else
        {
            // 线性过渡
            transform.localScale = Vector3.MoveTowards(transform.localScale, targetScale, Time.deltaTime * scaleTransitionSpeed);
        }
        
        // 平滑过渡颜色
        if (useColorEffect)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].color = Color.Lerp(graphics[i].color, targetColors[i], Time.unscaledDeltaTime * colorTransitionSpeed);
            }
        }
    }
    
    // 鼠标进入时
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateVisualState();
    }
    
    // 鼠标离开时
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        isPressed = false;
        UpdateVisualState();
    }
    
    // 鼠标按下时
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateVisualState();
    }
    
    // 鼠标释放时
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateVisualState();
    }
    
    // 更新视觉状态
    private void UpdateVisualState()
    {
        // 更新缩放
        if (isPressed)
        {
            // 点击状态
            targetScale = originalScale * clickScale;
        }
        else if (isHovering)
        {
            // 悬停状态
            targetScale = originalScale * hoverScale;
        }
        else
        {
            // 正常状态
            targetScale = originalScale;
        }
        
        // 更新发光效果
        if (useGlowEffect && outlineEffect != null)
        {
            if (isPressed)
            {
                // 点击状态发光
                outlineEffect.effectColor = clickGlowColor;
                outlineEffect.effectDistance = glowWidth;
            }
            else if (isHovering)
            {
                // 悬停状态发光
                outlineEffect.effectColor = hoverGlowColor;
                outlineEffect.effectDistance = glowWidth;
            }
            else
            {
                // 正常状态无发光
                outlineEffect.effectColor = normalGlowColor;
                outlineEffect.effectDistance = Vector2.zero;
            }
        }
        
        // 更新颜色效果
        if (useColorEffect)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                if (isPressed)
                {
                    // 点击状态变暗
                    targetColors[i] = new Color(
                        originalColors[i].r * clickDarkenAmount,
                        originalColors[i].g * clickDarkenAmount,
                        originalColors[i].b * clickDarkenAmount,
                        originalColors[i].a
                    );
                }
                else if (isHovering)
                {
                    // 悬停状态变亮/变色
                    targetColors[i] = new Color(
                        originalColors[i].r * hoverColor.r,
                        originalColors[i].g * hoverColor.g,
                        originalColors[i].b * hoverColor.b,
                        originalColors[i].a * hoverColor.a
                    );
                }
                else
                {
                    // 恢复原始颜色
                    targetColors[i] = originalColors[i];
                }
            }
        }
    }
    
    // 公共方法，可以从外部代码设置选中状态
    public void SetSelected(bool selected)
    {
        isHovering = selected;
        UpdateVisualState();
    }
}