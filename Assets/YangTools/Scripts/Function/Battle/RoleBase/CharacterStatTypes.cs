using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum StatType
{
    [LabelText("最大生命值")]
    MaxHealth,
    [LabelText("攻击力")]
    Attack,
    [LabelText("魔法力量")]
    MagicPower,
    [LabelText("防御力")]
    Defense,
    [LabelText("暴击率")]
    CritRate,
    [LabelText("暴击伤害")]
    CritDamage,
    [LabelText("技能伤害倍率")]
    SkillDamageMultiplier
}

public enum StatModifierType
{
    [LabelText("固定值")]
    Flat,
    [LabelText("百分比")]
    Percent
}

/// <summary>
/// 属性修饰符类
/// </summary>
[Serializable]
public class StatModifier
{
    /// <summary>
    /// 要修饰的属性类型，表示需要被修改的具体属性
    /// </summary>
    public StatType Stat;
    /// <summary>
    /// 修饰符类型，默认为Flat（固定值）类型
    /// 可以是固定值(Flat)、百分比(Percent)或倍率(Multiply)等类型
    /// </summary>
    public StatModifierType Type = StatModifierType.Flat;
    /// <summary>
    /// 修饰符的数值，根据Type的不同，有不同的计算方式
    /// </summary>
    public float Value;
}

/**
 * 角色属性类，用于管理和计算角色的各种属性值
 * 包含基础值、修饰符，并提供计算最终值的功能
 */
[Serializable]
public class CharacterStat
{
    [SerializeField] 
    private StatType type; //属性类型
    [SerializeField] 
    private float baseValue;//基础值

    //运行时修饰符列表，用于临时修改属性值
    private readonly List<RuntimeStatModifier> modifiers = new List<RuntimeStatModifier>();
    //标记位，表示值是否需要重新计算
    private bool dirty = true;
    //缓存的计算结果，避免重复计算
    private float cachedValue;

    //属性类型只读访问器
    public StatType Type => type;
    //基础值只读访问器
    public float BaseValue => baseValue;

    /**
     * 构造函数，初始化角色属性
     * @param type 属性类型
     * @param baseValue 基础值
     */
    public CharacterStat(StatType type, float baseValue)
    {
        this.type = type;
        this.baseValue = baseValue;
    }

    /**
     * 设置基础值
     */
    public void SetBaseValue(float value)
    {
        // 如果新值与当前值近似，则不做修改
        if (Mathf.Approximately(baseValue, value)) return;
        baseValue = value;
        dirty = true;  // 值被修改，标记为需要重新计算
    }

    /**
     * 获取属性最终值
     */
    public float GetValue()
    {
        // 如果值未被修改，直接返回缓存值
        if (!dirty) return cachedValue;

        float flat = 0f;    // 平加值总和
        float percent = 0f; // 百分比加成总和

        // 遍历所有修饰符，计算平加值和百分比加成
        for (int i = 0; i < modifiers.Count; i++)
        {
            var modifier = modifiers[i];
            if (modifier.Type == StatModifierType.Flat) flat += modifier.Value;
            else percent += modifier.Value;
        }

        // 计算最终值：(基础值 + 平加值) * (1 + 百分比加成)
        cachedValue = (baseValue + flat) * (1f + percent);
        dirty = false;  // 标记为已计算
        return cachedValue;
    }

    /**
     * 添加属性修饰符
     * @param modifier 要添加的修饰符
     */
    public void AddModifier(RuntimeStatModifier modifier)
    {
        modifiers.Add(modifier);
        dirty = true;  // 添加了新修饰符，标记为需要重新计算
    }

    /**
     * 移除指定来源的所有修饰符
     */
    public void RemoveModifiersFrom(object source)
    {
        // 移除所有与指定来源匹配的修饰符
        modifiers.RemoveAll(modifier => modifier.Source == source);
        dirty = true;  // 移除了修饰符，标记为需要重新计算
    }
}

/// <summary>
/// 运行时状态修饰器
/// </summary>
public class RuntimeStatModifier
{
    /// <summary>
    /// 状态修改器类型
    /// </summary>
    public StatModifierType Type;
    /// <summary>
    /// 修改器的数值
    /// </summary>
    public float Value;
    /// <summary>
    /// 修改器的来源对象，可以是技能、装备、buff等
    /// </summary>
    public object Source;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="type">修改器的类型</param>
    /// <param name="value">修改器的数值</param>
    /// <param name="source">修改器的来源对象</param>
    public RuntimeStatModifier(StatModifierType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
}
