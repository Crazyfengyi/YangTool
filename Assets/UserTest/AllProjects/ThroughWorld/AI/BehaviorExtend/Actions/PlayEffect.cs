using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 在角色模型挂点播放跟随或世界特效
/// </summary>
[TaskIcon(BehaviorSetting.path + "effect.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("在角色模型挂点播放跟随或世界特效")]
public class PlayEffect : Action
{
    public string name; // 保留原节点配置
    public bool isSelf; // 保留原节点配置
    public ModelPointType effectPointType; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.PlayRoleEffect(this, GetComponent<RoleBase>(), name, effectPointType, isSelf);
    }
}
