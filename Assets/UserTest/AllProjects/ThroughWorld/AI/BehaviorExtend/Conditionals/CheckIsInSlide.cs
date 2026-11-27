using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 当前角色没有旧滑步状态接口
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("当前角色没有旧滑步状态接口")]
public class CheckIsInSlide : Conditional
{
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Unsupported(this, "当前角色没有旧滑步状态接口");
    }
}
