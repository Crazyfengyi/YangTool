using System;
using UnityEngine;

/// <summary>
/// 命中后向 Demo URP 屏幕效果渲染器提供短时参数
/// </summary>
[DisallowMultipleComponent]
public sealed class HitFeelDemoScreenEffects : MonoBehaviour
{
    /// <summary>
    /// 单种屏幕反馈的强度和时长配置
    /// </summary>
    [Serializable]
    private sealed class EffectProfile
    {
        // 径向模糊持续秒数
        [Header("局部径向模糊")]
        /// <summary>径向模糊时长</summary>
        [Min(0f)] public float radialBlurDuration = 0.2f;
        // 径向模糊强度
        [Range(0f, 1f)] public float radialBlurIntensity = 0.22f;

        // 灰度效果持续秒数
        [Header("黑白")]
        /// <summary>黑白效果时长</summary>
        [Min(0f)] public float grayscaleDuration = 0.16f;
        // 黑白效果强度
        [Range(0f, 1f)] public float grayscaleIntensity = 0.45f;

        // 色彩扭曲持续秒数
        [Header("反色与色彩失真")]
        /// <summary>色彩失真时长</summary>
        [Min(0f)] public float colorDistortionDuration = 0.18f;
        // 色彩失真强度
        [Range(0f, 1f)] public float colorDistortionIntensity = 0.2f;

        // 冲击波持续秒数
        [Header("冲击光圈")]
        /// <summary>冲击光圈时长</summary>
        [Min(0f)] public float shockwaveDuration = 0.28f;
        // 冲击光圈强度
        [Range(0f, 0.1f)] public float shockwaveStrength = 0.025f;
        // 冲击光圈宽度
        [Range(0.005f, 0.08f)] public float shockwaveWidth = 0.025f;
    }

    // 屏幕效果中心点着色器属性标识
    private static readonly int CenterId = Shader.PropertyToID("_HitFeelCenter");
    // 画面宽高比着色器属性标识
    private static readonly int AspectId = Shader.PropertyToID("_HitFeelAspect");
    // 径向模糊着色器属性标识
    private static readonly int RadialBlurId = Shader.PropertyToID("_HitFeelRadialBlur");
    // 灰度着色器属性标识
    private static readonly int GrayscaleId = Shader.PropertyToID("_HitFeelGrayscale");
    // 色彩扭曲着色器属性标识
    private static readonly int ColorDistortionId = Shader.PropertyToID("_HitFeelColorDistortion");
    // 冲击波强度着色器属性标识
    private static readonly int ShockwaveId = Shader.PropertyToID("_HitFeelShockwave");
    // 冲击波半径着色器属性标识
    private static readonly int ShockwaveRadiusId = Shader.PropertyToID("_HitFeelShockwaveRadius");
    // 冲击波宽度着色器属性标识
    private static readonly int ShockwaveWidthId = Shader.PropertyToID("_HitFeelShockwaveWidth");

    // 普通命中反馈参数组
    [Header("强度预设")]
    /// <summary>普通命中屏幕效果参数</summary>
    [SerializeField] private EffectProfile normalProfile = new EffectProfile();
    // 强打击屏幕效果参数
    [SerializeField] private EffectProfile strongProfile = new EffectProfile
    {
        radialBlurDuration = 0.24f,
        radialBlurIntensity = 0.32f,
        grayscaleDuration = 0.2f,
        grayscaleIntensity = 0.65f,
        colorDistortionDuration = 0.22f,
        colorDistortionIntensity = 0.3f,
        shockwaveDuration = 0.36f,
        shockwaveStrength = 0.045f,
        shockwaveWidth = 0.035f
    };

    // 提供命中坐标投影的相机
    private Camera effectCamera;
    // 屏幕空间命中效果中心
    private Vector2 hitCenter = new Vector2(0.5f, 0.5f);
    // 径向模糊剩余时间
    private float radialBlurTime;
    // 径向模糊持续秒数
    private float radialBlurDuration;
    // 径向模糊峰值强度
    private float radialBlurPeak;
    // 灰度效果剩余时间
    private float grayscaleTime;
    // 灰度效果持续秒数
    private float grayscaleDuration;
    // 灰度效果峰值强度
    private float grayscalePeak;
    // 色彩扭曲剩余时间
    private float colorDistortionTime;
    // 色彩扭曲持续秒数
    private float colorDistortionDuration;
    // 色彩扭曲峰值强度
    private float colorDistortionPeak;
    // 冲击波剩余时间
    private float shockwaveTime;
    // 冲击波持续秒数
    private float shockwaveDuration;
    // 冲击波峰值强度
    private float shockwavePeak;
    // 冲击波环带宽度
    private float shockwaveWidth;

    /// <summary>
    /// 判断是否仍有屏幕效果需要渲染
    /// </summary>
    public bool IsPlaying => radialBlurTime > 0f || grayscaleTime > 0f || colorDistortionTime > 0f || shockwaveTime > 0f;

    /// <summary>
    /// 缓存所在相机
    /// </summary>
    private void Awake()
    {
        effectCamera = GetComponent<Camera>();
    }

    /// <summary>
    /// 使用非缩放时间推进效果动画
    /// </summary>
    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        radialBlurTime = Mathf.Max(0f, radialBlurTime - deltaTime);
        grayscaleTime = Mathf.Max(0f, grayscaleTime - deltaTime);
        colorDistortionTime = Mathf.Max(0f, colorDistortionTime - deltaTime);
        shockwaveTime = Mathf.Max(0f, shockwaveTime - deltaTime);
    }

    /// <summary>
    /// 从实际命中点启动已开启的屏幕效果
    /// </summary>
    public void PlayHit(Vector3 hitPoint, HitFeelDemoFeedbackPreset preset, bool radialBlur, bool grayscale, bool colorDistortion, bool shockwave)
    {
        if (effectCamera == null)
        {
            return;
        }

        Vector3 viewportPoint = effectCamera.WorldToViewportPoint(hitPoint);
        if (viewportPoint.z <= 0f)
        {
            return;
        }

        hitCenter = new Vector2(Mathf.Clamp01(viewportPoint.x), Mathf.Clamp01(viewportPoint.y));
        EffectProfile profile = preset == HitFeelDemoFeedbackPreset.Strong ? strongProfile : normalProfile;
        StartEffect(radialBlur, profile.radialBlurDuration, profile.radialBlurIntensity, ref radialBlurTime, ref radialBlurDuration, ref radialBlurPeak);
        StartEffect(grayscale, profile.grayscaleDuration, profile.grayscaleIntensity, ref grayscaleTime, ref grayscaleDuration, ref grayscalePeak);
        StartEffect(colorDistortion, profile.colorDistortionDuration, profile.colorDistortionIntensity, ref colorDistortionTime, ref colorDistortionDuration, ref colorDistortionPeak);
        StartEffect(shockwave, profile.shockwaveDuration, profile.shockwaveStrength, ref shockwaveTime, ref shockwaveDuration, ref shockwavePeak);
        shockwaveWidth = profile.shockwaveWidth;
    }

    /// <summary>
    /// 将本帧屏幕效果参数写入渲染属性块
    /// </summary>
    public void WriteShaderProperties(MaterialPropertyBlock properties)
    {
        float aspect = effectCamera != null && effectCamera.pixelHeight > 0
            ? (float)effectCamera.pixelWidth / effectCamera.pixelHeight
            : 1f;
        properties.SetVector(CenterId, hitCenter);
        properties.SetFloat(AspectId, aspect);
        properties.SetFloat(RadialBlurId, Evaluate(radialBlurTime, radialBlurDuration, radialBlurPeak));
        properties.SetFloat(GrayscaleId, Evaluate(grayscaleTime, grayscaleDuration, grayscalePeak));
        properties.SetFloat(ColorDistortionId, Evaluate(colorDistortionTime, colorDistortionDuration, colorDistortionPeak));
        properties.SetFloat(ShockwaveId, EvaluateShockwave());
        properties.SetFloat(ShockwaveRadiusId, ShockwaveProgress() * 0.75f);
        properties.SetFloat(ShockwaveWidthId, shockwaveWidth);
    }

    /// <summary>
    /// 清理组件禁用时可能残留的效果状态
    /// </summary>
    private void OnDisable()
    {
        radialBlurTime = 0f;
        grayscaleTime = 0f;
        colorDistortionTime = 0f;
        shockwaveTime = 0f;
    }

    /// <summary>
    /// 启动单项屏幕效果
    /// </summary>
    private static void StartEffect(bool enabled, float duration, float intensity, ref float time, ref float totalDuration, ref float peak)
    {
        if (!enabled || duration <= 0f || intensity <= 0f)
        {
            time = 0f;
            totalDuration = 0f;
            peak = 0f;
            return;
        }

        time = duration;
        totalDuration = duration;
        peak = intensity;
    }

    /// <summary>
    /// 计算短时反馈的淡入淡出包络
    /// </summary>
    private static float Evaluate(float remaining, float duration, float peak)
    {
        if (remaining <= 0f || duration <= 0f)
        {
            return 0f;
        }

        float progress = 1f - remaining / duration;
        float envelope = Mathf.Min(Mathf.Clamp01(progress * 10f), Mathf.Clamp01(remaining / (duration * 0.65f)));
        return peak * envelope;
    }

    /// <summary>
    /// 计算冲击光圈强度
    /// </summary>
    private float EvaluateShockwave()
    {
        if (shockwaveTime <= 0f || shockwaveDuration <= 0f)
        {
            return 0f;
        }

        return shockwavePeak * Mathf.Sin(ShockwaveProgress() * Mathf.PI);
    }

    /// <summary>
    /// 取得冲击光圈传播进度
    /// </summary>
    private float ShockwaveProgress()
    {
        return shockwaveDuration <= 0f ? 0f : 1f - shockwaveTime / shockwaveDuration;
    }
}
