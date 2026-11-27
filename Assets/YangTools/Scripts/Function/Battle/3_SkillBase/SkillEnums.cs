using System;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能目标
/// </summary>
public enum SkillTargetType
{
    [LabelText("自身")]
    Self,
    [LabelText("单位")]
    Unit,
    [LabelText("点")]
    Point,
    [LabelText("方向")]
    Direction
}

/// <summary>
/// 消耗失败原因
/// </summary>
public enum SkillCastFailReason
{
    [LabelText("无")]
    None,
    [LabelText("未知技能")]
    UnknownSkill,
    [LabelText("冷却中")]
    OnCooldown,
    [LabelText("资源不足")]
    NotEnoughResource,
    [LabelText("无效目标")]
    InvalidTarget,
    [LabelText("正在施法")]
    AlreadyCasting,
    [LabelText("时间线配置无效")]
    InvalidTimeline
}

/// <summary>
/// 技能消耗类.所需的资源消耗
/// </summary>
[Serializable]
public class SkillCost
{
    // 技能消耗的资源值
    [MinValue(0f)]
    [LabelText("资源消耗")]
    public float Resource = 0f;
}

/// <summary>
/// 技能施法请求类
/// </summary>
public class SkillCastRequest
{
    // 可选的整合技能时间线及表数值快照
    public UnityEngine.Timeline.TimelineAsset Timeline;
    public SkillTableDamage? TableDamage;
    public float? RangeOverride;
    public float? CooldownOverride;
    // 要施放的技能定义
    public SkillDefinition Skill;
    // 技能施法的目标对象（如果有）
    public GameObject Target;
    // 技能施法的位置点（如果是区域技能）
    public Vector3 Point;
    // 技能施法的方向向量
    public Vector3 Direction;
    // 本次施放的武器来源
    public WeaponDefinition SourceWeapon;
    // 武器实例句柄 用于区分同配置的不同实例
    public int SourceInstanceId;
}

/// <summary>
/// 技能执行相关信息
/// </summary>
public class SkillContext
{
    // 本次施法从技能表读取的伤害快照
    public SkillTableDamage? TableDamage;
    // 技能施放者
    public SkillCaster Caster;
    // 技能定义 包含技能的各种属性和行为
    public SkillDefinition Skill;
    // 技能目标对象
    public GameObject Target;
    // 技能目标点位置
    public Vector3 Point;
    // 技能施放方向
    public Vector3 Direction;
    // 技能施放原点
    public Vector3 OriginPoint;
    // 本次施放的武器来源
    public WeaponDefinition SourceWeapon;
    // 武器实例句柄
    public int SourceInstanceId;

    // 本次技能施放对应的运行时会话
    public SkillCastSession Session;

    /// <summary>
    /// 向技能施放器报告一个实际受到伤害的目标
    /// </summary>
    public void ReportHit(GameObject target)
    {
        if (Caster != null && target != null)
        {
            ReportHit(target, target.transform.position);
        }
    }

    /// <summary>
    /// 向技能施放器报告实际命中目标与世界坐标
    /// </summary>
    public void ReportHit(GameObject target, Vector3 hitPoint)
    {
        if (Caster != null && target != null)
        {
            Caster.ReportSkillHit(this, target, hitPoint);
        }
    }
}
