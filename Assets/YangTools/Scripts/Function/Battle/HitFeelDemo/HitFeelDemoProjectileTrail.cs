using UnityEngine;

/// <summary>
/// 管理打击感 Demo 专用弹体拖尾
/// </summary>
[DisallowMultipleComponent]
public sealed class HitFeelDemoProjectileTrail : MonoBehaviour
{
    private static bool isEnabled;
    private static HitFeelDemoFeedbackPreset activePreset;

    [Header("拖尾引用")]
    /// <summary>弹体拖尾渲染器</summary>
    [SerializeField] private TrailRenderer trailRenderer;

    [Header("强打击拖尾")]
    /// <summary>强打击拖尾宽度倍率</summary>
    [SerializeField, Min(1f)] private float strongWidthMultiplier = 1.35f;
    /// <summary>强打击拖尾时长倍率</summary>
    [SerializeField, Min(1f)] private float strongTimeMultiplier = 1.25f;

    private float originalWidth;
    private float originalTime;
    private bool hasDetachedTrail;

    /// <summary>
    /// 设置所有 Demo 弹体拖尾的开关
    /// </summary>
    public static void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
        HitFeelDemoProjectileTrail[] trails = FindObjectsByType<HitFeelDemoProjectileTrail>(FindObjectsSortMode.None);
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].ApplySettings();
        }
    }

    /// <summary>
    /// 设置后续弹体使用的强度预设
    /// </summary>
    public static void SetPreset(HitFeelDemoFeedbackPreset preset)
    {
        activePreset = preset;
        HitFeelDemoProjectileTrail[] trails = FindObjectsByType<HitFeelDemoProjectileTrail>(FindObjectsSortMode.None);
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].ApplySettings();
        }
    }

    /// <summary>
    /// 缓存拖尾原始参数并应用 Demo 当前设置
    /// </summary>
    private void Awake()
    {
        if (trailRenderer == null)
        {
            trailRenderer = GetComponentInChildren<TrailRenderer>();
        }

        if (trailRenderer == null)
        {
            return;
        }

        originalWidth = trailRenderer.widthMultiplier;
        originalTime = trailRenderer.time;
        ApplySettings();
    }

    /// <summary>
    /// 订阅 Demo 拖尾设置变化
    /// </summary>
    private void OnEnable()
    {
        ApplySettings();
    }

    /// <summary>
    /// 拖尾在弹体销毁后继续渐隐
    /// </summary>
    private void OnDestroy()
    {
        if (trailRenderer == null || !isEnabled || hasDetachedTrail || trailRenderer.positionCount < 2)
        {
            return;
        }

        hasDetachedTrail = true;
        trailRenderer.emitting = false;
        Transform trailTransform = trailRenderer.transform;
        trailTransform.SetParent(null, true);
        Destroy(trailTransform.gameObject, Mathf.Max(0.1f, trailRenderer.time));
    }

    /// <summary>
    /// 应用普通或强打击的拖尾参数
    /// </summary>
    private void ApplySettings()
    {
        if (trailRenderer == null)
        {
            return;
        }

        bool strong = activePreset == HitFeelDemoFeedbackPreset.Strong;
        trailRenderer.widthMultiplier = originalWidth * (strong ? strongWidthMultiplier : 1f);
        trailRenderer.time = originalTime * (strong ? strongTimeMultiplier : 1f);
        trailRenderer.emitting = isEnabled;
        trailRenderer.enabled = isEnabled;
    }
}
