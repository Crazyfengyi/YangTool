using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 检查角色生命是否归零
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("检查角色生命是否归零")]
public class CheckIsDie : Conditional
{
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Result(GetComponent<RoleBase>()?.HealthControl is { } health && health.GetCurrentHp() <= 0);
    }
}
