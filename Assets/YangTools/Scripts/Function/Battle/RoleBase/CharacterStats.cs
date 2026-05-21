using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 角色属性类，用于管理游戏角色的各种属性值
/// </summary>
public class CharacterStats : MonoBehaviour
{
    /// <summary>
    /// 初始属性列表，包含角色创建时的基础属性值
    /// </summary>
    [SerializeField] private List<CharacterStat> initialStats = new List<CharacterStat>
    {
        new CharacterStat(StatType.MaxHealth, 100f),      // 最大生命值
        new CharacterStat(StatType.Attack, 10f),          // 攻击力
        new CharacterStat(StatType.MagicPower, 10f),      // 魔法强度
        new CharacterStat(StatType.Defense, 0f),         // 防御力
        new CharacterStat(StatType.CritRate, 0.05f),      // 暴击率
        new CharacterStat(StatType.CritDamage, 1.5f),     // 暴击伤害
        new CharacterStat(StatType.SkillDamageMultiplier, 0f) // 技能伤害倍率
    };

    /// <summary>
    /// 属性字典
    /// </summary>
    private readonly Dictionary<StatType, CharacterStat> statsDic = new Dictionary<StatType, CharacterStat>();
    /// <summary>
    /// 属性值变更事件
    /// </summary>
    public event Action<StatType, float> OnStatChanged;

    /// <summary>
    /// 初始化属性查找表
    /// </summary>
    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 初始属性列表
    /// </summary>
    private void Init()
    {
        statsDic.Clear();
        for (int i = 0; i < initialStats.Count; i++)
        {
            var stat = initialStats[i];
            if (stat != null)
            {
                statsDic[stat.Type] = stat;
            }
        }
    }
    
    /// <summary>
    /// 确保类型属性存在，如果不存在则创建
    /// </summary>
    private void CheckStat(StatType type)
    {
        if (statsDic.Count == 0)
        {
            Init();
        }

        if (!statsDic.ContainsKey(type))
        {
            statsDic[type] = new CharacterStat(type, 0f);
        }
    }
    
    /// <summary>
    /// 获取指定类型的属性值
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <returns>属性值</returns>
    public float GetValue(StatType type)
    {
        CheckStat(type);
        return statsDic[type].GetValue();
    }

    /// <summary>
    /// 设置指定类型属性的基础值
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <param name="value">要设置的基础值</param>
    public void SetBaseValue(StatType type, float value)
    {
        CheckStat(type);
        statsDic[type].SetBaseValue(value);
        OnStatChanged?.Invoke(type, statsDic[type].GetValue());
    }

    /// <summary>
    /// 为指定类型属性添加修饰器
    /// </summary>
    /// <param name="type">属性类型</param>
    /// <param name="modifierType">修饰器类型</param>
    /// <param name="value">修饰值</param>
    /// <param name="source">修饰器来源</param>
    public void AddModifier(StatType type, StatModifierType modifierType, float value, object source)
    {
        CheckStat(type);
        statsDic[type].AddModifier(new RuntimeStatModifier(modifierType, value, source));
        OnStatChanged?.Invoke(type, statsDic[type].GetValue());
    }

    /// <summary>
    /// 移除来自特定来源的所有修饰器
    /// </summary>
    /// <param name="source">要移除的修饰器来源</param>
    public void RemoveModifiersFrom(object source)
    {
        foreach (var pair in statsDic)
        {
            pair.Value.RemoveModifiersFrom(source);
            OnStatChanged?.Invoke(pair.Key, pair.Value.GetValue());
        }
    }
}
