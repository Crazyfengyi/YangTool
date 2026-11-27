using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;
using YangTools.Scripts.Core.YangAudio;

/// <summary>
/// 播放一次或循环角色音效
/// </summary>
[TaskIcon(BehaviorSetting.path + "music.jpeg")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("播放一次或循环角色音效")]
public class PlayMusic : Action
{
    public string name; // 保留原节点配置
    public bool loop = false; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null || string.IsNullOrWhiteSpace(name)) return TaskStatus.Failure;
        YangAudioManager.Instance.PlayWorldSound(role.gameObject, name, loop);
        return TaskStatus.Success;
    }
}
