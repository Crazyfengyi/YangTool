using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能效果
/// </summary>
public abstract class SkillEffect : ScriptableObject
{
    [LabelText("技能效果名称")] [SerializeField] private string effectName;
    [LabelText("技能效果描述")] [SerializeField] private string effectDescription;

    public abstract void Execute(SkillContext context);
}
