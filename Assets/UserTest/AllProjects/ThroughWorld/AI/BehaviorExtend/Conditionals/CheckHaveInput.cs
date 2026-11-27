using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 检查角色的移动或跳跃输入
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("检查角色的移动或跳跃输入")]
public class CheckHaveInput : Conditional
{
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Result(GetComponent<CMF.CharacterInput>() is { } input && (Mathf.Abs(input.GetHorizontalMovementInput()) > 0.01f || Mathf.Abs(input.GetVerticalMovementInput()) > 0.01f || input.IsJumpKeyPressed()));
    }
}
