using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 在前置节点确认的命中目标上播放特效
/// </summary>
[TaskIcon(BehaviorSetting.path + "effect.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("在配置的命中目标上播放特效")]
public class PlayHittingEffect : Action
{
    public string name; // 保留原节点配置
    public bool isFollow; // 保留原节点配置
    public ModelPointType effectPointType = ModelPointType.Foot; // 保留原节点配置
    public SharedGameObject hitTarget; // 由前置命中节点写入的目标

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var hit = hitTarget?.Value;
        if (hit == null) return BehaviorSetting.Unsupported(this, "请绑定由前置节点确认的命中目标");
        return BehaviorSetting.PlayRoleEffect(this, hit.GetComponentInParent<RoleBase>(), name, effectPointType, isFollow);
    }
}
