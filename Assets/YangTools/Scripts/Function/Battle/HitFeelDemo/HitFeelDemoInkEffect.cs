using System.Collections;
using UnityEngine;

/// <summary>
/// 在实际命中点播放墨迹主体与少量墨点粒子
/// </summary>
[DisallowMultipleComponent]
public sealed class HitFeelDemoInkEffect : MonoBehaviour
{
    [Header("资源引用")]
    /// <summary>水墨主体渲染器</summary>
    [SerializeField] private SpriteRenderer inkRenderer;
    /// <summary>墨点粒子系统</summary>
    [SerializeField] private ParticleSystem inkParticles;

    [Header("普通命中")]
    /// <summary>普通命中墨迹尺寸</summary>
    [SerializeField, Min(0.1f)] private float normalSize = 0.8f;
    /// <summary>普通命中墨迹持续时间</summary>
    [SerializeField, Min(0.1f)] private float normalLifetime = 0.5f;
    /// <summary>普通命中墨点数量</summary>
    [SerializeField, Min(0)] private int normalParticleCount = 5;

    [Header("强打击")]
    /// <summary>强打击墨迹尺寸倍率</summary>
    [SerializeField, Min(1f)] private float strongSizeMultiplier = 1.45f;
    /// <summary>强打击墨迹持续时间倍率</summary>
    [SerializeField, Min(1f)] private float strongLifetimeMultiplier = 1.25f;
    /// <summary>强打击额外墨点数量</summary>
    [SerializeField, Min(0)] private int strongExtraParticleCount = 5;

    /// <summary>
    /// 生成并播放一份墨迹效果实例
    /// </summary>
    public void PlayAt(Vector3 hitPoint, Camera viewCamera, HitFeelDemoFeedbackPreset preset)
    {
        if (inkRenderer == null)
        {
            inkRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (inkParticles == null)
        {
            inkParticles = GetComponentInChildren<ParticleSystem>();
        }

        if (inkRenderer == null)
        {
            return;
        }

        Quaternion rotation = viewCamera != null
            ? Quaternion.LookRotation(viewCamera.transform.forward, viewCamera.transform.up)
            : Quaternion.identity;
        HitFeelDemoInkEffect effect = Instantiate(this, hitPoint, rotation);
        effect.PlayInstance(viewCamera, preset);
    }

    /// <summary>
    /// 配置粒子并启动墨迹淡出
    /// </summary>
    private void PlayInstance(Camera viewCamera, HitFeelDemoFeedbackPreset preset)
    {
        bool strong = preset == HitFeelDemoFeedbackPreset.Strong;
        float size = normalSize * (strong ? strongSizeMultiplier : 1f);
        float lifetime = normalLifetime * (strong ? strongLifetimeMultiplier : 1f);
        int particleCount = normalParticleCount + (strong ? strongExtraParticleCount : 0);

        transform.localScale = Vector3.one * size;
        if (viewCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(viewCamera.transform.forward, viewCamera.transform.up);
        }

        if (inkParticles != null)
        {
            ConfigureParticles(inkParticles, size, lifetime, particleCount);
        }

        StartCoroutine(FadeAndDestroy(lifetime));
    }

    /// <summary>
    /// 设置墨点数量、颜色与散开速度
    /// </summary>
    private static void ConfigureParticles(ParticleSystem particles, float size, float lifetime, int count)
    {
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.useUnscaledTime = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.45f, lifetime);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.08f, size * 0.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(size * 1.2f, size * 2.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.015f, 0.018f, 0.02f, 0.9f));
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;
        particles.Play(true);
        particles.Emit(Mathf.Max(0, count));
    }

    /// <summary>
    /// 使用非缩放时间淡出墨迹并销毁实例
    /// </summary>
    private IEnumerator FadeAndDestroy(float lifetime)
    {
        float elapsed = 0f;
        Color baseColor = inkRenderer != null ? inkRenderer.color : Color.black;
        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            if (inkRenderer != null)
            {
                Color color = baseColor;
                color.a = 1f - Mathf.Clamp01(elapsed / lifetime);
                inkRenderer.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
