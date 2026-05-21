using System.Collections.Generic;
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
    public string Id;
    /// <summary>
    /// Buff的显示名称
    /// </summary>
    public string DisplayName;
    /// <summary>
    /// Buff的详细描述文本
    /// </summary>
    [TextArea] public string Description;
    /// <summary>
    /// Buff的图标
    /// </summary>
    public Sprite Icon;
    /// <summary>
    /// Buff的持续时间(秒)
    /// </summary>
    [Min(0f)] public float Duration = 5f;
    /// <summary>
    /// 当Buff再次被应用时是否刷新持续时间
    /// true表示刷新持续时间，false表示不刷新
    /// </summary>
    public bool RefreshDuration = true;
    /// <summary>
    /// Buff包含的属性修饰符列表
    /// </summary>
    public List<StatModifier> Modifiers = new List<StatModifier>();
}
