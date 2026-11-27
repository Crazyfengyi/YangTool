using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 通过角色标记接口写入布尔值
/// </summary>
[TaskIcon(BehaviorSetting.path + "Common.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("通过角色标记接口写入布尔值")]
public class CommonSetBool : Action
{
    [HideInInspector] public int boolUseEnum; // 保留旧标记值供迁移时核对
    public RoleFlag roleFlag; // 在编辑器选择当前项目的角色标记
    public bool boolValue; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null) return TaskStatus.Failure;
        if (roleFlag == RoleFlag.None) return BehaviorSetting.Unsupported(this, "请重新选择角色布尔标记");
        role.SetFlag(roleFlag, boolValue ? 1 : 0);
        return TaskStatus.Success;
    }
}
