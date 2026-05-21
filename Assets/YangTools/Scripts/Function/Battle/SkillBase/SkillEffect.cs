using UnityEngine;

/// <summary>
/// 技能效果
/// </summary>
public abstract class SkillEffect : ScriptableObject
{
    public abstract void Execute(SkillContext context);
}
