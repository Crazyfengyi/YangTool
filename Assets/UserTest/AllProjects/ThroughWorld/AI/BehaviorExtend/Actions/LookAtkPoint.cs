using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 转向攻击记录点
/// </summary>
[TaskCategory("RoleAI/Motion")]
[TaskDescription("转向攻击记录点")]
public class LookAtkPoint : Action
{
    public float speed = 10f; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        var root = BehaviorSetting.Root(role);
        if (root == null) return TaskStatus.Failure;
        var point = BehaviorSetting.Record(role).Attack;
        return point.HasValue ? BehaviorSetting.Face(root, point.Value - root.position, speed) : TaskStatus.Failure;
    }
}
