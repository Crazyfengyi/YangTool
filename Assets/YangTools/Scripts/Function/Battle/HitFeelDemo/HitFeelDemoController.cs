using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HitFeelDemo 的反馈强度预设
/// </summary>
public enum HitFeelDemoFeedbackPreset
{
    Normal,
    Strong
}

/// <summary>
/// 打击感测试场景控制器
/// </summary>
public class HitFeelDemoController : MonoBehaviour
{
    #region 引用与参数

    [Header("战斗引用")]
    /// <summary>玩家技能施放器</summary>
    [SerializeField] private SkillCaster playerCaster;
    /// <summary>测试攻击技能</summary>
    [SerializeField] private SkillDefinition attackSkill;
    /// <summary>测试敌人对象</summary>
    [SerializeField] private GameObject targetEnemy;
    /// <summary>敌人生命组件</summary>
    [SerializeField] private SimpleHealth targetHealth;
    /// <summary>敌人移动根节点</summary>
    [SerializeField] private Transform targetTransform;
    /// <summary>敌人显示组件</summary>
    [SerializeField] private Renderer targetRenderer;
    /// <summary>测试场景摄像机</summary>
    [SerializeField] private Camera demoCamera;

    [Header("界面")]
    /// <summary>测试攻击按钮</summary>
    [SerializeField] private Button attackButton;
    /// <summary>测试状态文本</summary>
    [SerializeField] private Text statusText;
    /// <summary>敌人生命文本</summary>
    [SerializeField] private Text healthText;
    /// <summary>命中顿帧开关</summary>
    [SerializeField] private Toggle hitStopToggle;
    /// <summary>命中音效开关</summary>
    [SerializeField] private Toggle hitSoundToggle;
    /// <summary>命中特效开关</summary>
    [SerializeField] private Toggle hitVfxToggle;
    /// <summary>敌人闪白开关</summary>
    [SerializeField] private Toggle enemyFlashToggle;
    /// <summary>敌人击退开关</summary>
    [SerializeField] private Toggle enemyKnockbackToggle;
    /// <summary>镜头震动开关</summary>
    [SerializeField] private Toggle cameraShakeToggle;
    /// <summary>弹体拖尾开关</summary>
    [SerializeField] private Toggle projectileTrailToggle;
    /// <summary>局部径向模糊开关</summary>
    [SerializeField] private Toggle radialBlurToggle;
    /// <summary>黑白效果开关</summary>
    [SerializeField] private Toggle grayscaleToggle;
    /// <summary>反色与色彩失真开关</summary>
    [SerializeField] private Toggle colorDistortionToggle;
    /// <summary>冲击光圈开关</summary>
    [SerializeField] private Toggle shockwaveToggle;
    /// <summary>水墨效果开关</summary>
    [SerializeField] private Toggle inkEffectToggle;

    [Header("命中效果组件")]
    /// <summary>主摄像机上的屏幕反馈组件</summary>
    [SerializeField] private HitFeelDemoScreenEffects screenEffects;
    /// <summary>水墨命中特效预制体</summary>
    [SerializeField] private HitFeelDemoInkEffect inkEffectPrefab;

    [Header("反馈资源")]
    /// <summary>命中特效预制体</summary>
    [SerializeField] private GameObject hitVfxPrefab;
    /// <summary>命中音源</summary>
    [SerializeField] private AudioSource hitAudioSource;
    /// <summary>命中音效资源</summary>
    [SerializeField] private AudioClip hitAudioClip;

    [Header("反馈参数")]
    /// <summary>命中顿帧时长</summary>
    [SerializeField, Min(0f)] private float hitStopDuration = 0.06f;
    /// <summary>命中特效清理时长</summary>
    [SerializeField, Min(0.1f)] private float hitVfxLifetime = 2f;
    /// <summary>敌人闪白时长</summary>
    [SerializeField, Min(0f)] private float enemyFlashDuration = 0.12f;
    /// <summary>敌人击退距离</summary>
    [SerializeField, Min(0f)] private float enemyKnockbackDistance = 0.25f;
    /// <summary>敌人击退时长</summary>
    [SerializeField, Min(0f)] private float enemyKnockbackDuration = 0.12f;
    /// <summary>镜头震动幅度</summary>
    [SerializeField, Min(0f)] private float cameraShakeAmplitude = 0.08f;
    /// <summary>镜头震动时长</summary>
    [SerializeField, Min(0f)] private float cameraShakeDuration = 0.12f;

    [Header("强度预设")]
    /// <summary>选择用于下一次测试攻击的命中强度</summary>
    [SerializeField] private HitFeelDemoFeedbackPreset feedbackPreset = HitFeelDemoFeedbackPreset.Normal;
    /// <summary>强打击顿帧时长</summary>
    [SerializeField, Min(0f)] private float strongHitStopDuration = 0.16f;
    /// <summary>强打击命中特效缩放</summary>
    [SerializeField, Min(0f)] private float strongHitVfxScale = 1.25f;
    /// <summary>强打击音量倍率</summary>
    [SerializeField, Min(0f)] private float strongHitSoundVolumeScale = 1.15f;
    /// <summary>强打击敌人闪白时长</summary>
    [SerializeField, Min(0f)] private float strongEnemyFlashDuration = 0.16f;
    /// <summary>强打击击退距离</summary>
    [SerializeField, Min(0f)] private float strongEnemyKnockbackDistance = 0.4f;
    /// <summary>强打击击退时长</summary>
    [SerializeField, Min(0f)] private float strongEnemyKnockbackDuration = 0.16f;
    /// <summary>强打击镜头震动幅度</summary>
    [SerializeField, Min(0f)] private float strongCameraShakeAmplitude = 0.15f;
    /// <summary>强打击镜头震动时长</summary>
    [SerializeField, Min(0f)] private float strongCameraShakeDuration = 0.16f;

    private Material targetMaterial;
    private Color targetOriginalColor;
    private Vector3 targetOriginalPosition;
    private Vector3 cameraOriginalLocalPosition;
    private Coroutine hitStopRoutine;
    private Coroutine flashRoutine;
    private Coroutine knockbackRoutine;
    private Coroutine cameraShakeRoutine;
    private float previousTimeScale = 1f;
    private int attackCount;
    private int hitCount;

    #endregion

    #region 生命周期与状态恢复

    /// <summary>
    /// 缓存场景引用并注册攻击技能
    /// </summary>
    private void Awake()
    {
        if (playerCaster != null && attackSkill != null)
        {
            playerCaster.AddSkill(attackSkill);
        }

        if (targetTransform != null)
        {
            targetOriginalPosition = targetTransform.position;
        }

        if (targetRenderer != null)
        {
            targetMaterial = targetRenderer.material;
            targetOriginalColor = targetMaterial.color;
        }

        if (demoCamera != null)
        {
            cameraOriginalLocalPosition = demoCamera.transform.localPosition;
        }
    }

    /// <summary>
    /// 初始化状态显示
    /// </summary>
    private void Start()
    {
        if (statusText != null)
        {
            statusText.text = "等待测试";
        }

        if (targetHealth != null)
        {
            UpdateHealthDisplay(targetHealth.Current, targetHealth.Max);
        }
    }

    /// <summary>
    /// 注册界面与战斗事件
    /// </summary>
    private void OnEnable()
    {
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(StartAttack);
        }

        if (projectileTrailToggle != null)
        {
            projectileTrailToggle.onValueChanged.AddListener(HandleProjectileTrailToggleChanged);
        }

        if (playerCaster != null)
        {
            playerCaster.OnSkillHit += HandleSkillHit;
        }

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += HandleHealthChanged;
        }

        ApplyProjectileTrailSettings();
    }

    /// <summary>
    /// 解除事件并恢复场景状态
    /// </summary>
    private void OnDisable()
    {
        if (attackButton != null)
        {
            attackButton.onClick.RemoveListener(StartAttack);
        }

        if (projectileTrailToggle != null)
        {
            projectileTrailToggle.onValueChanged.RemoveListener(HandleProjectileTrailToggleChanged);
        }

        if (playerCaster != null)
        {
            playerCaster.OnSkillHit -= HandleSkillHit;
        }

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= HandleHealthChanged;
        }

        bool restoreTimeScale = hitStopRoutine != null;
        StopAllCoroutines();
        hitStopRoutine = null;
        flashRoutine = null;
        knockbackRoutine = null;
        cameraShakeRoutine = null;

        if (restoreTimeScale)
        {
            Time.timeScale = previousTimeScale;
        }

        if (targetMaterial != null)
        {
            targetMaterial.color = targetOriginalColor;
        }

        if (targetTransform != null)
        {
            targetTransform.position = targetOriginalPosition;
        }

        if (demoCamera != null)
        {
            demoCamera.transform.localPosition = cameraOriginalLocalPosition;
        }

        HitFeelDemoProjectileTrail.SetEnabled(false);
        HitFeelDemoProjectileTrail.SetPreset(HitFeelDemoFeedbackPreset.Normal);
    }

    #endregion

    #region 攻击与命中反馈

    /// <summary>
    /// 向测试敌人发起一次攻击
    /// </summary>
    public void StartAttack()
    {
        if (playerCaster == null || attackSkill == null || targetEnemy == null)
        {
            SetStatus("攻击配置缺失");
            return;
        }

        ApplyProjectileTrailSettings();

        if (!playerCaster.TryCast(attackSkill, targetEnemy))
        {
            SetStatus("攻击未发出");
            return;
        }

        attackCount++;
        SetStatus($"已发射 #{attackCount}");
    }

    /// <summary>
    /// 根据实际技能命中触发独立反馈
    /// </summary>
    private void HandleSkillHit(SkillCastSession session, GameObject hitTarget)
    {
        if (hitTarget != targetEnemy)
        {
            return;
        }

        hitCount++;
        bool strong = feedbackPreset == HitFeelDemoFeedbackPreset.Strong;
        Vector3 hitPoint = session != null ? session.LastHitPoint : targetEnemy.transform.position;
        SetStatus(strong ? $"强打击命中 #{hitCount}" : $"命中 #{hitCount}");

        if (IsEffectEnabled(hitVfxToggle))
        {
            PlayHitVfx(hitPoint, strong ? strongHitVfxScale : 1f);
        }

        if (IsEffectEnabled(hitSoundToggle) && hitAudioSource != null && hitAudioClip != null)
        {
            hitAudioSource.PlayOneShot(hitAudioClip, strong ? strongHitSoundVolumeScale : 1f);
        }

        if (IsEffectEnabled(enemyFlashToggle))
        {
            StartEnemyFlash(strong ? strongEnemyFlashDuration : enemyFlashDuration);
        }

        if (IsEffectEnabled(enemyKnockbackToggle))
        {
            StartEnemyKnockback(
                strong ? strongEnemyKnockbackDistance : enemyKnockbackDistance,
                strong ? strongEnemyKnockbackDuration : enemyKnockbackDuration);
        }

        if (IsEffectEnabled(cameraShakeToggle))
        {
            StartCameraShake(
                strong ? strongCameraShakeAmplitude : cameraShakeAmplitude,
                strong ? strongCameraShakeDuration : cameraShakeDuration);
        }

        if (IsEffectEnabled(hitStopToggle))
        {
            StartHitStop(strong ? strongHitStopDuration : hitStopDuration);
        }

        if (screenEffects != null)
        {
            screenEffects.PlayHit(
                hitPoint,
                feedbackPreset,
                IsEffectEnabled(radialBlurToggle),
                IsEffectEnabled(grayscaleToggle),
                IsEffectEnabled(colorDistortionToggle),
                IsEffectEnabled(shockwaveToggle));
        }

        if (IsEffectEnabled(inkEffectToggle) && inkEffectPrefab != null)
        {
            inkEffectPrefab.PlayAt(hitPoint, demoCamera, feedbackPreset);
        }
    }

    /// <summary>
    /// 更新敌人生命显示
    /// </summary>
    private void HandleHealthChanged(float current, float maximum)
    {
        UpdateHealthDisplay(current, maximum);
    }

    /// <summary>
    /// 显示当前敌人生命值
    /// </summary>
    private void UpdateHealthDisplay(float current, float maximum)
    {
        if (healthText != null)
        {
            healthText.text = $"敌人生命：{current:0} / {maximum:0}";
        }
    }

    /// <summary>
    /// 更新测试状态文本
    /// </summary>
    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    /// <summary>
    /// 判断单项反馈是否启用
    /// </summary>
    private bool IsEffectEnabled(Toggle toggle)
    {
        return toggle == null || toggle.isOn;
    }

    /// <summary>
    /// 在实际命中坐标播放命中特效
    /// </summary>
    private void PlayHitVfx(Vector3 hitPoint, float scale)
    {
        if (hitVfxPrefab == null)
        {
            return;
        }

        GameObject effect = Instantiate(hitVfxPrefab, hitPoint, Quaternion.identity);
        effect.transform.localScale *= scale;
        Destroy(effect, hitVfxLifetime);
    }

    #endregion

    #region 反馈动画

    /// <summary>
    /// 开始短暂命中顿帧
    /// </summary>
    private void StartHitStop(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (hitStopRoutine != null)
        {
            StopCoroutine(hitStopRoutine);
        }
        else
        {
            previousTimeScale = Time.timeScale;
        }

        Time.timeScale = 0f;
        hitStopRoutine = StartCoroutine(ResumeTimeScale(duration));
    }

    /// <summary>
    /// 使用非缩放时间结束顿帧
    /// </summary>
    private IEnumerator ResumeTimeScale(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = previousTimeScale;
        hitStopRoutine = null;
    }

    /// <summary>
    /// 开始敌人颜色闪烁
    /// </summary>
    private void StartEnemyFlash(float duration)
    {
        if (targetMaterial == null || duration <= 0f)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        targetMaterial.color = Color.white;
        flashRoutine = StartCoroutine(RestoreEnemyColor(duration));
    }

    /// <summary>
    /// 恢复敌人原始颜色
    /// </summary>
    private IEnumerator RestoreEnemyColor(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (targetMaterial != null)
        {
            targetMaterial.color = targetOriginalColor;
        }

        flashRoutine = null;
    }

    /// <summary>
    /// 开始敌人短暂击退
    /// </summary>
    private void StartEnemyKnockback(float distance, float duration)
    {
        if (targetTransform == null || playerCaster == null || duration <= 0f)
        {
            return;
        }

        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }

        Vector3 direction = targetTransform.position - playerCaster.transform.position;
        direction.y = 0f;
        direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
        knockbackRoutine = StartCoroutine(AnimateEnemyKnockback(direction, distance, duration));
    }

    /// <summary>
    /// 执行击退并返回初始位置
    /// </summary>
    private IEnumerator AnimateEnemyKnockback(Vector3 direction, float distance, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float offset = Mathf.Sin(progress * Mathf.PI) * distance;
            targetTransform.position = targetOriginalPosition + direction * offset;
            yield return null;
        }

        targetTransform.position = targetOriginalPosition;
        knockbackRoutine = null;
    }

    /// <summary>
    /// 开始短暂镜头震动
    /// </summary>
    private void StartCameraShake(float amplitude, float duration)
    {
        if (demoCamera == null || duration <= 0f)
        {
            return;
        }

        if (cameraShakeRoutine != null)
        {
            StopCoroutine(cameraShakeRoutine);
        }

        demoCamera.transform.localPosition = cameraOriginalLocalPosition;
        cameraShakeRoutine = StartCoroutine(AnimateCameraShake(amplitude, duration));
    }

    /// <summary>
    /// 执行镜头震动并恢复位置
    /// </summary>
    private IEnumerator AnimateCameraShake(float amplitude, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float strength = 1f - Mathf.Clamp01(elapsed / duration);
            demoCamera.transform.localPosition = cameraOriginalLocalPosition + Random.insideUnitSphere * amplitude * strength;
            yield return null;
        }

        demoCamera.transform.localPosition = cameraOriginalLocalPosition;
        cameraShakeRoutine = null;
    }

    #endregion

    #region 拖尾控制

    /// <summary>
    /// 同步测试面板与弹体 Prefab 的拖尾状态
    /// </summary>
    private void ApplyProjectileTrailSettings()
    {
        HitFeelDemoProjectileTrail.SetPreset(feedbackPreset);
        HitFeelDemoProjectileTrail.SetEnabled(IsEffectEnabled(projectileTrailToggle));
    }

    /// <summary>
    /// 响应拖尾开关变化
    /// </summary>
    private void HandleProjectileTrailToggleChanged(bool enabled)
    {
        HitFeelDemoProjectileTrail.SetEnabled(enabled);
    }

    #endregion
}
