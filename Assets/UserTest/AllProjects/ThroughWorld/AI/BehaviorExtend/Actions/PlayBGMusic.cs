using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 播放背景音乐 旧叠加移除语义需要游戏音乐栈
/// </summary>
[TaskIcon(BehaviorSetting.path + "music.jpeg")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("播放背景音乐 旧叠加移除语义需要游戏音乐栈")]
public class PlayBGMusic : Action
{
    public string name; // 保留原节点配置
    public bool isAdd = true; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (string.IsNullOrWhiteSpace(name)) return TaskStatus.Failure;
        if (!isAdd) return BehaviorSetting.Unsupported(this, "当前项目没有按名称移除的背景音乐栈");
        GameSoundManager.Instance.PlayMusic(name);
        return TaskStatus.Success;
    }
}
