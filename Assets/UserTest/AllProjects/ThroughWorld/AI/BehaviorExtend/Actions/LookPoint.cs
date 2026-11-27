using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 转向普通记录点
/// </summary>
[TaskIcon(BehaviorSetting.path + "眼睛.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("转向普通记录点")]
public class LookPoint : Action
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
        var point = BehaviorSetting.Record(role).Position;
        return point.HasValue ? BehaviorSetting.Face(root, point.Value - root.position, speed) : TaskStatus.Failure;
    }
}
