using DataStruct;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 独立演示使用的技能配置 不写入正式技能表
/// </summary>
[CreateAssetMenu(menuName = "ThroughWorld/技能流程测试配置")]
public class SkillFlowDemoDefinition : ScriptableObject
{
    [SerializeField, LabelText("技能编号")] private int skillId = 99001; // 各测试技能使用独立冷却编号
    [SerializeField, Min(0), LabelText("冷却（秒）")] private int cooldown = 3; // 实际开始后计算的冷却
    [SerializeField, LabelText("使用时间线")] private bool useTimeline; // 执行方式
    [SerializeField, LabelText("资源名")] private string resourceName; // Skills 下的测试资源名
    [SerializeField, Min(1), LabelText("伤害")] private int damage = 20; // 两种方式使用相同伤害

    /// <summary>
    /// 创建与正式施法入口兼容的运行时技能数据
    /// </summary>
    internal RunTimeSkillData CreateRuntimeData()
    {
        var skill = new cfg.skill.Skill(skillId, name, "技能流程测试", new cfg.skill.AtkData(0f, damage),
            3, cooldown, useTimeline ? 0 : 1, resourceName, null, false, default, null,
            new System.Collections.Generic.List<cfg.item.ItemExchange>(), null);
        return new RunTimeSkillData { skill = skill, weight = 1 };
    }
}
