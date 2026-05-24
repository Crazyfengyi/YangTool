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
    AlreadyCasting
}

/// <summary>
/// 技能消耗类.所需的资源消耗
/// </summary>
[Serializable]
public class SkillCost
{
    /// <summary>
    /// 技能消耗的资源值
    /// </summary>
    [Min(0f)]
    public float Resource = 0f;
}

/// <summary>
/// 技能施法请求类
/// </summary>
public class SkillCastRequest
{
    /// <summary>
    /// 要施放的技能定义
    /// </summary>
    public SkillDefinition Skill;
    /// <summary>
    /// 技能施法的目标对象（如果有）
    /// </summary>
    public GameObject Target;
    /// <summary>
    /// 技能施法的位置点（如果是区域技能）
    /// </summary>
    public Vector3 Point;
    /// <summary>
    /// 技能施法的方向向量
    /// </summary>
    public Vector3 Direction;
    /// <summary>
    /// 本次施放的武器来源
    /// </summary>
    public WeaponDefinition SourceWeapon;
    /// <summary>
    /// 武器实例句柄，用于区分同配置的不同实例
    /// </summary>
    public int SourceInstanceId;
}

/// <summary>
/// 技能执行相关信息
/// </summary>
public class SkillContext
{
    /// <summary>
    /// 技能施放者
    /// </summary>
    public SkillCaster Caster;
    /// <summary>
    /// 技能定义，包含技能的各种属性和行为
    /// </summary>
    public SkillDefinition Skill;
    /// <summary>
    /// 技能目标对象
    /// </summary>
    public GameObject Target;
    /// <summary>
    /// 技能目标点位置
    /// </summary>
    public Vector3 Point;
    /// <summary>
    /// 技能施放方向
    /// </summary>
    public Vector3 Direction;
    /// <summary>
    /// 技能施放原点
    /// </summary>
    public Vector3 OriginPoint;
    /// <summary>
    /// 本次施放的武器来源
    /// </summary>
    public WeaponDefinition SourceWeapon;
    /// <summary>
    /// 武器实例句柄
    /// </summary>
    public int SourceInstanceId;
}
