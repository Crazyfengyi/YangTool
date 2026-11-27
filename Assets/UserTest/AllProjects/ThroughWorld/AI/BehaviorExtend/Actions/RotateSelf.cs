using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 恢复记录的水平朝向
/// </summary>
[TaskCategory("RoleAI/Motion")]
[TaskDescription("恢复记录的水平朝向")]
public class RotateSelf : Action
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
        var point = BehaviorSetting.Record(role).Direction;
        return point.HasValue ? BehaviorSetting.Face(root, point.Value, speed) : TaskStatus.Failure;
    }
}
