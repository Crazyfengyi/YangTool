using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 效果使用的伤害参数来源 保持旧资产默认采用资产值
/// </summary>
public enum SkillDamageSource
{
    [LabelText("效果资产数值")] Asset,
    [LabelText("技能表数值")] SkillTable
}

/// <summary>
/// 施法时读取的表数值快照 不修改共享技能资产
/// </summary>
public readonly struct SkillTableDamage
{
    public readonly float BaseDamage;
    public readonly float AttackScale;

    /// <summary>
    /// 保存表中固定伤害与攻击倍率
    /// </summary>
    public SkillTableDamage(float baseDamage, float attackScale)
    {
        BaseDamage = baseDamage;
        AttackScale = attackScale;
    }
}

/// <summary>
/// 可选的角色属性桥接 用于复用 Battle 伤害公式
/// </summary>
public interface ISkillStatProvider
{
    float GetSkillStat(StatType type);
}

/// <summary>
/// 向统一伤害计算提供当前效果的数值来源
/// </summary>
public static class SkillDamageBridge
{
    /// <summary>
    /// 按显式来源选择数值 缺失表数据时停止效果并提示
    /// </summary>
    public static bool Resolve(SkillContext context, SkillDamageSource source,
        ref float baseDamage, ref float powerScale, ref SkillPowerType powerType)
    {
        if (source == SkillDamageSource.Asset) return true;
        if (context == null || !context.TableDamage.HasValue)
        {
            Debug.LogError("技能效果要求表伤害数据 但当前施法上下文未提供", context?.Caster);
            return false;
        }
        baseDamage = context.TableDamage.Value.BaseDamage;
        powerScale = context.TableDamage.Value.AttackScale;
        powerType = SkillPowerType.Attack;
        return true;
    }
}
