using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 检查当前技能或公共间隔是否冷却中
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("检查当前技能或公共间隔是否冷却中")]
public class CheckIsInCooling : Conditional
{
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var control = GetComponent<RoleBase>()?.SkillControl;
        if (control == null) return TaskStatus.Failure;
        var skill = control.CurrentRunTimeSkillData ?? control.LastResult.Skill;
        return BehaviorSetting.Result(control.CommonCooldownRemaining > 0 || control.GetCooldownRemaining(skill) > 0);
    }
}
