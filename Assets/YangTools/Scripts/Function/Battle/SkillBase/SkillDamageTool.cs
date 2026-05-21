using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能伤害计算公式类
/// </summary>
public static class SkillDamageTool
{
    /// <summary>
    /// 计算技能伤害
    /// </summary>
    /// <param name="context">技能上下文，包含施法者信息</param>
    /// <param name="target">目标游戏对象</param>
    /// <param name="baseDamage">基础伤害值</param>
    /// <param name="powerScale">力量系数，用于放大力量值的影响</param>
    /// <param name="powerType">技能力量类型，决定使用哪种属性计算力量值</param>
    /// <returns>返回一个SkillDamageResult对象，包含伤害值和是否暴击的信息</returns>
    public static SkillDamageResult Calculate(
        SkillContext context,
        GameObject target,
        float baseDamage,
        float powerScale,
        SkillPowerType powerType)
    {
        // 获取施法者和目标的CharacterStats组件，如果对象不存在则为null
        var casterStats = context.Caster != null ? context.Caster.GetComponent<CharacterStats>() : null;
        var targetStats = target != null ? target.GetComponent<CharacterStats>() : null;

        // 伤害类型
        float power = GetPower(casterStats, powerType);
        // 获取施法者的技能伤害加成
        float multiplier = 1f + GetStat(casterStats, StatType.SkillDamageMultiplier);
        // 计算原始伤害值 = (基础伤害 + 伤害类型值 * 类型倍率) * 伤害倍率
        float rawDamage = (baseDamage + power * powerScale) * multiplier;
        // 应用目标的防御力,计算减伤后的伤害值
        float reducedDamage = ApplyDefense(rawDamage, GetStat(targetStats, StatType.Defense));

        // 获取施法者的暴击率，并限制在0-1之间
        float critRate = Mathf.Clamp01(GetStat(casterStats, StatType.CritRate));
        // 获取施法者的暴击伤害倍率，确保至少为1
        float critDamage = Mathf.Max(1f, GetStat(casterStats, StatType.CritDamage));
        // 判断本次攻击是否暴击
        bool isCritical = Random.value < critRate;

        // 如果是暴击，则伤害值乘以暴击伤害倍率
        if (isCritical)
        {
            reducedDamage *= critDamage;
        }

        // 返回伤害结果，确保伤害值不小于0，并包含是否暴击的信息
        return new SkillDamageResult
        {
            Amount = Mathf.Max(0f, reducedDamage),
            IsCritical = isCritical
        };
    }

    /// <summary>
    /// 根据技能类型获取对应的属性值
    /// </summary>
    /// <param name="stats">角色属性组件</param>
    /// <param name="powerType">技能力量类型</param>
    /// <returns>返回对应的属性值</returns>
    private static float GetPower(CharacterStats stats, SkillPowerType powerType)
    {
        if (stats == null) return 0f;

        if (powerType == SkillPowerType.Attack)
        {
            return stats.GetValue(StatType.Attack);
        }

        if (powerType == SkillPowerType.MagicPower)
        {
            return stats.GetValue(StatType.MagicPower);
        }

        return stats.GetValue(StatType.Attack) + stats.GetValue(StatType.MagicPower);
    }

    /// <summary>
    /// 应用防御力计算减伤后的伤害值
    /// </summary>
    /// <param name="damage">原始伤害值</param>
    /// <param name="defense">防御力值</param>
    /// <returns>减伤后的伤害值</returns>
    private static float ApplyDefense(float damage, float defense)
    {
        if (defense <= 0f) return damage;
        return damage * (100f / (100f + defense));
    }

    /// <summary>
    /// 获取角色属性值
    /// </summary>
    /// <param name="stats">角色属性组件</param>
    /// <param name="type">属性类型</param>
    /// <returns>属性值，如果stats为null则返回0</returns>
    private static float GetStat(CharacterStats stats, StatType type)
    {
        return stats != null ? stats.GetValue(type) : 0f;
    }
}

/// <summary>
/// 属性类型
/// </summary>
public enum SkillPowerType
{
    [LabelText("物理攻击力")]
    Attack,
    [LabelText("魔法攻击力")]
    MagicPower,
    [LabelText("物理攻击力与魔法攻击力综合计算")]
    Both
}

/// <summary>
/// 技能伤害结果类，用于存储技能造成的伤害数据
/// </summary>
public class SkillDamageResult
{
    /// <summary>
    /// 伤害数值
    /// </summary>
    public float Amount;
    /// <summary>
    /// 是否为暴击伤害
    /// </summary>
    public bool IsCritical;
}
