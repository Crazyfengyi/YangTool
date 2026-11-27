using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 记录当前目标的指定模型挂点
/// </summary>
[TaskIcon(BehaviorSetting.path + "红点.png")]
[TaskCategory("RoleAI/Record")]
[TaskDescription("记录当前目标的指定模型挂点")]
public class RecordAtkPoint : Conditional
{
    public ModelPointType attackPointType = ModelPointType.Body; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null) return TaskStatus.Failure;
        var record = BehaviorSetting.Record(role);
        record.Attack = null;
        var target = role.Target != null ? role.Target.GetComponentInParent<RoleBase>() : null;
        var point = target != null && target.ModelInfo != null ? target.ModelInfo.GetEffectPoint(attackPointType) : null;
        if (point == null) return TaskStatus.Failure;
        record.Attack = point.position;
        return TaskStatus.Success;
    }
}
