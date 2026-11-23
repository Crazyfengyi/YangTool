using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Buff类,基本属性和效果
/// </summary>
[CreateAssetMenu(menuName = "Game/Buffs/Buff Definition")]
public class BuffDefinition : ScriptableObject
{
    /// <summary>
    /// Buff的唯一标识符
    /// </summary>
    [LabelText("Buff 标识")]
    public string Id;
    /// <summary>
    /// Buff的显示名称
    /// </summary>
    [LabelText("显示名称")]
    public string DisplayName;
    /// <summary>
    /// Buff的详细描述文本
    /// </summary>
    [LabelText("Buff 描述")]
    [MultiLineProperty] public string Description;
    /// <summary>
    /// Buff的图标
    /// </summary>
    [LabelText("Buff 图标")]
    public Sprite Icon;
    /// <summary>
    /// Buff的持续时间(秒)
    /// </summary>
    [LabelText("持续时间（秒）")]
    [MinValue(0f)] public float Duration = 5f;
    /// <summary>
    /// 当Buff再次被应用时是否刷新持续时间
    /// true表示刷新持续时间，false表示不刷新
    /// </summary>
    [LabelText("重复施加时刷新持续时间")]
    [PropertyTooltip("同一 Buff 再次施加时是否重新计算剩余持续时间")]
    public bool RefreshDuration = true;
    /// <summary>
    /// Buff包含的属性修饰符列表
    /// </summary>
    [LabelText("属性修饰列表")]
    public List<StatModifier> Modifiers = new List<StatModifier>();
}
