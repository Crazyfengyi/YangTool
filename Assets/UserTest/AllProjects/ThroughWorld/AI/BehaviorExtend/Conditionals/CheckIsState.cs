using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 当前角色没有旧控制状态查询接口
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("当前角色没有旧控制状态查询接口")]
public class CheckIsState : Conditional
{
    public ControlStateType controlStateType; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Unsupported(this, "当前角色没有旧控制状态查询接口");
    }
}
