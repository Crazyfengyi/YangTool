using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 检查目标是否在当前技能水平范围内
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("检查目标是否在当前技能水平范围内")]
public class CheckTargetInAttckRang : Conditional
{
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Result(BehaviorSetting.InSkillRange(GetComponent<RoleBase>()));
    }
}
