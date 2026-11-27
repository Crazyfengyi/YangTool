using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 在单次 Battle 施法会话内播放技能时间线并路由效果节点
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public sealed class SkillTimelinePlayer : MonoBehaviour
{
    [SerializeField] private PlayableDirector director; // 技能时间线播放组件
    [SerializeField] private Animator animator; // 动画轨道显式绑定对象
    private SkillContext context; // 当前会话的独立上下文
    private readonly List<Object> bindings = new List<Object>(); // 本次使用的轨道绑定
    private readonly HashSet<SkillTimelineClip> triggered = new HashSet<SkillTimelineClip>(); // 每个片段只触发一次
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// 在提交施法前校验播放器和动画轨道引用
    /// </summary>
    public bool CanPlay(TimelineAsset timeline)
    {
        if (!isActiveAndEnabled || director == null || timeline == null || IsPlaying ||
            director.state == PlayState.Playing || timeline.duration <= 0) return false;
        foreach (PlayableBinding binding in timeline.outputs)
            if (binding.outputTargetType == typeof(Animator) && animator == null) return false;
        return true;
    }

    /// <summary>
    /// 校验轨道后创建播放图 并等待真实停止事件
    /// </summary>
    public bool Begin(TimelineAsset timeline, SkillContext castContext)
    {
        if (!CanPlay(timeline) || castContext?.Session == null) return false;
        context = castContext;
        triggered.Clear();
        director.playableAsset = timeline;
        director.extrapolationMode = DirectorWrapMode.None;
        foreach (PlayableBinding binding in timeline.outputs)
        {
            if (binding.outputTargetType == typeof(Animator))
            {
                director.SetGenericBinding(binding.sourceObject, animator);
                bindings.Add(binding.sourceObject);
            }
        }
        director.stopped += OnStopped;
        director.time = 0;
        IsPlaying = true;
        director.Play();
        return true;
    }

    /// <summary>
    /// 在当前会话仍有效时只执行一次指定片段
    /// </summary>
    internal void Trigger(SkillTimelineClip clip)
    {
        if (!IsPlaying || context?.Session == null || context.Session.IsCompleted || clip == null || clip.Effect == null) return;
        if (!triggered.Add(clip)) return;
        try
        {
            clip.Effect.Execute(context);
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception, this);
            if (context?.Caster != null) context.Caster.CancelCast();
        }
    }

    /// <summary>
    /// 中止播放并清理当前会话的绑定
    /// </summary>
    public void Stop()
    {
        if (director != null)
        {
            director.stopped -= OnStopped;
            if (IsPlaying) director.Stop();
            foreach (Object binding in bindings) director.ClearGenericBinding(binding);
        }
        bindings.Clear();
        triggered.Clear();
        context = null;
        IsPlaying = false;
    }

    /// <summary>
    /// Director 自然停止时结束本次播放
    /// </summary>
    private void OnStopped(PlayableDirector stopped)
    {
        Stop();
    }

    /// <summary>
    /// 组件停用时释放时间线引用
    /// </summary>
    private void OnDisable()
    {
        if (IsPlaying && context?.Caster != null) context.Caster.CancelCast();
        Stop();
    }
}
