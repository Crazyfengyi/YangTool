using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 当前项目缺少圆形预警资源和播放接口
/// </summary>
[TaskIcon(BehaviorSetting.path + "标识_叹号.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("当前项目缺少圆形预警资源和播放接口")]
public class PlayWarningCircular : Action
{
    public float time = 1; // 保留原节点配置
    public float radius = 5f; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Unsupported(this, "当前项目缺少圆形预警资源和播放接口");
    }
}
