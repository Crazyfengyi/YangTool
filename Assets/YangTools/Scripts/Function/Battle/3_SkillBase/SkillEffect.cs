using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能效果
/// </summary>
public abstract class SkillEffect : ScriptableObject
{
    // 技能效果名称
    [LabelText("技能效果名称")] [SerializeField] private string effectName;
    // 技能效果描述
    [LabelText("技能效果描述")] [SerializeField] private string effectDescription;

    /// <summary>
    /// 使用技能上下文执行由具体效果实现的行为
    /// </summary>
    /// <param name="context">本次效果使用的施法上下文</param>
    public abstract void Execute(SkillContext context);
}
