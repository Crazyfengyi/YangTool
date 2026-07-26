using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用血条目标适配器。将外部战斗逻辑转换成血条可读取的数据快照。
/// </summary>
public class HealthBarTarget : MonoBehaviour, IHealthBarDataSource
{
    [SerializeField] private Transform healthBarAnchor;
    [SerializeField] private string displayName;
    [SerializeField] private int level;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float shield;
    [SerializeField] private List<HealthBarStatusData> statuses = new List<HealthBarStatusData>();
    [SerializeField] private List<HealthBarBuffData> buffs = new List<HealthBarBuffData>();

    public event Action OnHealthBarDataChanged;
    public Transform HealthBarAnchor => healthBarAnchor == null ? transform : healthBarAnchor;

    /// <summary>
    /// 获取当前血条数据快照。
    /// </summary>
    public HealthBarDisplayData GetHealthBarData()
    {
        return new HealthBarDisplayData
        {
            DisplayName = displayName,
            Level = level,
            CurrentHealth = currentHealth,
            MaxHealth = maxHealth,
            Shield = shield,
            Statuses = new List<HealthBarStatusData>(statuses),
            Buffs = new List<HealthBarBuffData>(buffs)
        }.Clone();
    }

    /// <summary>
    /// 设置目标名称和等级。
    /// </summary>
    public void SetDisplayInfo(string targetName, int targetLevel)
    {
        displayName = targetName;
        level = Mathf.Max(0, targetLevel);
        NotifyDataChanged();
    }

    /// <summary>
    /// 设置生命值和最大生命值。
    /// </summary>
    public void SetHealth(float health, float maximumHealth)
    {
        maxHealth = Mathf.Max(0.01f, maximumHealth);
        currentHealth = Mathf.Clamp(health, 0f, maxHealth);
        NotifyDataChanged();
    }

    /// <summary>
    /// 扣除生命值并通知血条播放受伤表现。
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(currentHealth - amount, maxHealth);
    }

    /// <summary>
    /// 增加生命值并通知血条播放治疗表现。
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        SetHealth(currentHealth + amount, maxHealth);
    }

    /// <summary>
    /// 设置护盾值。
    /// </summary>
    public void SetShield(float value)
    {
        shield = Mathf.Max(0f, value);
        NotifyDataChanged();
    }

    /// <summary>
    /// 替换状态图标列表。
    /// </summary>
    public void SetStatuses(IList<HealthBarStatusData> values)
    {
        statuses.Clear();
        if (values != null) statuses.AddRange(values);
        NotifyDataChanged();
    }

    /// <summary>
    /// 替换 Buff 图标列表。
    /// </summary>
    public void SetBuffs(IList<HealthBarBuffData> values)
    {
        buffs.Clear();
        if (values != null) buffs.AddRange(values);
        NotifyDataChanged();
    }

    /// <summary>
    /// 手动通知血条重新读取数据。
    /// </summary>
    public void NotifyDataChanged()
    {
        OnHealthBarDataChanged?.Invoke();
    }
}
