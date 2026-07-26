using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 血条状态图标数据。
/// </summary>
[Serializable]
public class HealthBarStatusData
{
    [SerializeField] private Sprite icon;
    [SerializeField] private string displayName;

    public Sprite Icon => icon;
    public string DisplayName => displayName;

    public HealthBarStatusData(Sprite icon, string displayName = "")
    {
        this.icon = icon;
        this.displayName = displayName;
    }
}

/// <summary>
/// 血条 Buff 图标数据。
/// </summary>
[Serializable]
public class HealthBarBuffData
{
    [SerializeField] private Sprite icon;
    [SerializeField] private string displayName;
    [SerializeField] private int stackCount = 1;
    [SerializeField] private float remainingTime = -1f;

    public Sprite Icon => icon;
    public string DisplayName => displayName;
    public int StackCount => stackCount;
    public float RemainingTime => remainingTime;

    public HealthBarBuffData(Sprite icon, string displayName = "", int stackCount = 1, float remainingTime = -1f)
    {
        this.icon = icon;
        this.displayName = displayName;
        this.stackCount = Mathf.Max(1, stackCount);
        this.remainingTime = remainingTime;
    }
}

/// <summary>
/// 血条显示所需的完整数据快照。
/// </summary>
[Serializable]
public class HealthBarDisplayData
{
    public string DisplayName;
    public int Level;
    public float CurrentHealth;
    public float MaxHealth = 1f;
    public float Shield;
    public List<HealthBarStatusData> Statuses = new List<HealthBarStatusData>();
    public List<HealthBarBuffData> Buffs = new List<HealthBarBuffData>();

    /// <summary>
    /// 创建数据快照并修正生命值边界。
    /// </summary>
    public HealthBarDisplayData Clone()
    {
        var clone = new HealthBarDisplayData
        {
            DisplayName = DisplayName,
            Level = Level,
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, Mathf.Max(0.01f, MaxHealth)),
            MaxHealth = Mathf.Max(0.01f, MaxHealth),
            Shield = Mathf.Max(0f, Shield),
            Statuses = Statuses == null ? new List<HealthBarStatusData>() : new List<HealthBarStatusData>(Statuses),
            Buffs = Buffs == null ? new List<HealthBarBuffData>() : new List<HealthBarBuffData>(Buffs)
        };
        return clone;
    }
}

/// <summary>
/// 血条目标数据接口。角色、NPC、Boss 或其他可显示对象均可实现此接口。
/// </summary>
public interface IHealthBarDataSource
{
    event Action OnHealthBarDataChanged;
    Transform HealthBarAnchor { get; }
    HealthBarDisplayData GetHealthBarData();
}
