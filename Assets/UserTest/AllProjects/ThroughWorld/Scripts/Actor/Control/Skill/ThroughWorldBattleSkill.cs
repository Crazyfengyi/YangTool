using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Timeline;

/// <summary>
/// 将 ThroughWorld 技能表编号映射到 Battle 技能与表现时间线
/// </summary>
[CreateAssetMenu(menuName = "ThroughWorld/Battle技能映射")]
public sealed class ThroughWorldBattleSkill : ScriptableObject
{
    [LabelText("技能表编号")] public int skillId; // Luban 技能编号
    [LabelText("Battle技能定义")] public SkillDefinition skill; // 效果与目标类型配置
    [LabelText("技能时间线")] public TimelineAsset timeline; // 表现与效果触发时序
}
