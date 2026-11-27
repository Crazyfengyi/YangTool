using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 可在时间线上配置的 Battle 技能效果触发片段
/// </summary>
[Serializable]
public sealed class SkillTimelineClip : PlayableAsset, ITimelineClipAsset
{
    [SerializeField, LabelText("技能效果")] private SkillEffect effect; // 触发时使用的效果资产
    public SkillEffect Effect => effect;
    public ClipCaps clipCaps => ClipCaps.None;

    /// <summary>
    /// 将当前效果片段与施法者的时间线播放器关联
    /// </summary>
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<SkillTimelineBehaviour>.Create(graph);
        playable.GetBehaviour().player = owner.GetComponent<SkillTimelinePlayer>();
        playable.GetBehaviour().clip = this;
        return playable;
    }
}

/// <summary>
/// 首次进入片段时触发效果 编辑器预览不造成战斗伤害
/// </summary>
public sealed class SkillTimelineBehaviour : PlayableBehaviour
{
    internal SkillTimelinePlayer player; // 当前施法者播放器
    internal SkillTimelineClip clip; // 当前效果片段

    /// <summary>
    /// 仅运行时执行技能效果
    /// </summary>
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (Application.isPlaying && player != null) player.Trigger(clip);
    }
}

/// <summary>
/// 承载可视化技能效果片段的 Timeline 轨道
/// </summary>
[TrackColor(0.2f, 0.65f, 0.75f)]
[TrackClipType(typeof(SkillTimelineClip))]
public sealed class SkillTimelineTrack : TrackAsset
{
}
