using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 血条状态图标数据
/// </summary>
[Serializable]
public class HealthBarStatusData
{
    // 显示状态或增益的图标
    [SerializeField] private Sprite icon;
    // 面向界面显示的名称
    [SerializeField] private string displayName;

    public Sprite Icon => icon;
    public string DisplayName => displayName;

    /// <summary>
    /// 保存状态图标和显示名称
    /// </summary>
    /// <param name="icon">状态或增益图标</param>
    /// <param name="displayName">界面显示名称</param>
    public HealthBarStatusData(Sprite icon, string displayName = "")
    {
        this.icon = icon;
        this.displayName = displayName;
    }
}

/// <summary>
/// 血条 Buff 图标数据
/// </summary>
[Serializable]
public class HealthBarBuffData
{
    // 显示状态或增益的图标
    [SerializeField] private Sprite icon;
    // 面向界面显示的名称
    [SerializeField] private string displayName;
    // 增益叠加层数
    [SerializeField] private int stackCount = 1;
    // 增益剩余秒数 负值表示不显示倒计时
    [SerializeField] private float remainingTime = -1f;

    public Sprite Icon => icon;
    public string DisplayName => displayName;
    public int StackCount => stackCount;
    public float RemainingTime => remainingTime;

    /// <summary>
    /// 保存增益图标 名称 层数与剩余秒数并保证至少一层
    /// </summary>
    /// <param name="icon">状态或增益图标</param>
    /// <param name="displayName">界面显示名称</param>
    /// <param name="stackCount">增益叠加层数</param>
    /// <param name="remainingTime">增益剩余秒数 负值表示不显示倒计时</param>
    public HealthBarBuffData(Sprite icon, string displayName = "", int stackCount = 1, float remainingTime = -1f)
    {
        this.icon = icon;
        this.displayName = displayName;
        this.stackCount = Mathf.Max(1, stackCount);
        this.remainingTime = remainingTime;
    }
}

/// <summary>
/// 血条显示所需的完整数据快照
/// </summary>
[Serializable]
public class HealthBarDisplayData
{
    // 界面显示名称
    public string DisplayName;
    // 角色等级
    public int Level;
    // 当前生命值
    public float CurrentHealth;
    // 生命上限
    public float MaxHealth = 1f;
    // 当前护盾值
    public float Shield;
    // 状态图标数据列表
    public List<HealthBarStatusData> Statuses = new List<HealthBarStatusData>();
    // 增益图标数据列表
    public List<HealthBarBuffData> Buffs = new List<HealthBarBuffData>();

    /// <summary>
    /// 创建数据快照并修正生命值边界
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
/// 血条目标数据接口 角色 NPC Boss 或其他可显示对象均可实现此接口
/// </summary>
public interface IHealthBarDataSource
{
    // 血条相关数据变化时通知订阅者
    event Action OnHealthBarDataChanged;
    Transform HealthBarAnchor { get; }
    /// <summary>
    /// 提供当前血条显示数据 由管理器复制后用于刷新视图
    /// </summary>
    HealthBarDisplayData GetHealthBarData();
}
