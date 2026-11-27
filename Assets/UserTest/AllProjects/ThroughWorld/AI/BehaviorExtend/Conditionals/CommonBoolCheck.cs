using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 读取现有角色标记 旧系统专用标记需重新配置
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("读取现有角色标记 旧系统专用标记需重新配置")]
public class CommonBoolCheck : Conditional
{
    [HideInInspector] public int boolUseEnum; // 保留旧标记值供迁移时核对
    public RoleFlag roleFlag; // 在编辑器选择当前项目的角色标记

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null) return TaskStatus.Failure;
        if (roleFlag == RoleFlag.None) return BehaviorSetting.Unsupported(this, "请重新选择角色布尔标记");
        return BehaviorSetting.Result(role.GetFlag(roleFlag) != 0);
    }
}
