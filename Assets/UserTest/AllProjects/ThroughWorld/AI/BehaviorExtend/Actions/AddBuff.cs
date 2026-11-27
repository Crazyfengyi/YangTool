using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 按配置概率向角色添加 ThroughWorld Buff
/// </summary>
[TaskIcon(BehaviorSetting.path + "Common.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("按概率向配置的命中目标添加 ThroughWorld Buff")]
public class AddBuff : Action
{
    public BuffID buffId; // 保留原节点配置
    public int percent = 50; // 保留原节点配置
    public SharedGameObject hitTarget; // 由前置命中节点写入的目标

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null || (int)buffId == 0) return TaskStatus.Failure;
        var hit = hitTarget?.Value;
        if (hit == null) return BehaviorSetting.Unsupported(this, "请绑定由前置节点确认的命中目标");
        var target = hit.GetComponentInParent<RoleBase>();
        if (target?.roleBuffControl == null) return TaskStatus.Failure;
        if (Random.Range(0, 100) >= Mathf.Clamp(percent, 0, 100)) return TaskStatus.Success;
        return BehaviorSetting.Result(target.roleBuffControl.Add(buffId, role) != null);
    }
}
