using UnityEngine;
using DG.Tweening;

/// <summary>
/// Prefab缩放动画脚本
/// 在Start时自动播放从小到大再到小的缩放动画
/// </summary>
public class PrefabScaleAnimation : MonoBehaviour
{
    [Header("动画设置")]
    public float scaleUpTime = 0.3f;        // 放大时间
    public float holdTime = 1.5f;           // 保持时间
    public float scaleDownTime = 0.3f;      // 缩小时间
    
    [Header("缩放设置")]
    public Vector3 startScale = Vector3.zero;     // 开始缩放
    public Vector3 targetScale = Vector3.one;     // 目标缩放
    
    [Header("其他设置")]
    public bool autoDestroy = true;               // 动画结束后是否自动销毁
    public Ease scaleUpEase = Ease.OutBack;       // 放大缓动类型
    public Ease scaleDownEase = Ease.InBack;      // 缩小缓动类型
    
    void Start()
    {
        // 设置初始缩放
        transform.localScale = startScale;
        
        // 开始动画序列
        StartScaleAnimation();
    }
    
    void StartScaleAnimation()
    {
        // 创建动画序列
        Sequence scaleSequence = DOTween.Sequence();
        
        // 1. 从小变大
        scaleSequence.Append(transform.DOScale(targetScale, scaleUpTime).SetEase(scaleUpEase));
        
        // 2. 保持一段时间
        scaleSequence.AppendInterval(holdTime);
        
        // 3. 从大变小
        scaleSequence.Append(transform.DOScale(startScale, scaleDownTime).SetEase(scaleDownEase));
        
        // 4. 动画完成后处理
        scaleSequence.OnComplete(() => {
            if (autoDestroy)
            {
                Destroy(gameObject);
            }
        });
    }
} 