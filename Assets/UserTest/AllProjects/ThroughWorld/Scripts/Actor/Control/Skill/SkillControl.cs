/* 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-07-30 
*/
using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using DataStruct;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 统一管理角色技能的提交 校验 执行 冷却与退出
/// </summary>
[Serializable]
public class SkillControl
{
    private readonly RoleBase role; // 当前施法角色
    private readonly BehaviorTree skillTree; // 独立技能执行树
    private readonly PlayableDirector skillTimeLine; // 技能时间线执行器
    private readonly ExternalBehavior legacyBehavior; // 未填写资源名的旧配置回退
    private readonly Dictionary<int, RunTimeSkillData> registeredSkills = new Dictionary<int, RunTimeSkillData>(); // 按技能编号维护冷却
    private readonly Dictionary<string, PlayableAsset> timelines = new Dictionary<string, PlayableAsset>(); // 已加载的时间线
    private readonly Dictionary<string, ExternalBehavior> behaviors = new Dictionary<string, ExternalBehavior>(); // 已加载的技能树
    private readonly List<UnityEngine.Object> bindings = new List<UnityEngine.Object>(); // 当前时间线的绑定键
    private Action<PlayableDirector> timelineStopped; // 本次时间线结束回调
    private BehaviorDesigner.Runtime.Behavior.BehaviorHandler behaviorEnded; // 本次技能树结束回调
    private PlayableAsset preparedTimeline; // 本次校验后的时间线
    private ExternalBehavior preparedBehavior; // 本次校验后的技能树
    private GameActor requestedTarget; // 提交时确定的目标
    private float requestElapsed; // 等待调度的累计秒数
    private float requestTimeout = 5f; // 等待调度的最长秒数
    private float commonCooldownDuration; // 结束后需要等待的公共间隔
    private int executionSeed; // 请求编号种子

    public RunTimeSkillData CurrentRunTimeSkillData { get; private set; }
    public SkillRunState State { get; private set; }
    public bool NeedUseSkill => State != SkillRunState.Idle;
    public bool IsExecuting => State == SkillRunState.Executing;
    public bool IsTimeLine => CurrentRunTimeSkillData?.skill.TimelineORbehavior == 0;
    public BehaviorTree SkillTree => skillTree;
    public PlayableDirector SkillTimeLine => skillTimeLine;
    public int ExecutionId { get; private set; }
    public float CommonCooldownRemaining { get; private set; }
    public SkillExecutionResult LastResult { get; private set; }
    public event Action<SkillExecutionResult> OnSkillEnded; // 每个请求只发出一次结束通知

    /// <summary>
    /// 保存执行引用 原技能树用于兼容没有资源名的旧技能
    /// </summary>
    public SkillControl(RoleBase role, BehaviorTree skillTree, PlayableDirector timeline)
    {
        this.role = role;
        this.skillTree = skillTree;
        skillTimeLine = timeline;
        legacyBehavior = skillTree != null ? skillTree.ExternalBehavior : null;
    }

    #region 请求与校验
    /// <summary>
    /// 设置调度等待时限与结束后的公共间隔
    /// </summary>
    public void Configure(float timeout, float commonCooldown)
    {
        requestTimeout = Mathf.Max(0.1f, timeout);
        commonCooldownDuration = Mathf.Max(0, commonCooldown);
    }

    /// <summary>
    /// 注册技能以便无目标或移动期间继续更新冷却 同编号共享一份状态
    /// </summary>
    public void RegisterSkills(IEnumerable<RunTimeSkillData> skills)
    {
        if (skills == null) return;
        foreach (RunTimeSkillData data in skills) RegisterSkill(data);
    }

    /// <summary>
    /// 注册一个有效技能并返回该编号的统一运行时状态
    /// </summary>
    private RunTimeSkillData RegisterSkill(RunTimeSkillData data)
    {
        if (data?.skill == null) return null;
        if (registeredSkills.TryGetValue(data.skill.Id, out RunTimeSkillData registered)) return registered;
        registeredSkills.Add(data.skill.Id, data);
        return data;
    }

    /// <summary>
    /// 查询技能编号对应的剩余冷却
    /// </summary>
    public float GetCooldownRemaining(RunTimeSkillData data)
    {
        if (data?.skill == null) return 0;
        return registeredSkills.TryGetValue(data.skill.Id, out RunTimeSkillData registered)
            ? Mathf.Max(0, registered.currentCD) : Mathf.Max(0, data.currentCD);
    }

    /// <summary>
    /// 选技追击距离只考虑配置完整的技能
    /// </summary>
    internal bool HasSkillResources(RunTimeSkillData data)
    {
        return data?.skill != null && PrepareResources(data, out _, out _) == SkillUseFailReason.None;
    }

    /// <summary>
    /// 保留旧调用入口 目标使用角色当前选中的对象
    /// </summary>
    public void UseSkill(RunTimeSkillData skillData)
    {
        if (!TryUseSkill(skillData, role != null ? role.Target : null, out SkillUseFailReason reason))
            Debug.LogWarning($"[ThroughWorld技能] 请求失败 {reason}", role);
    }

    /// <summary>
    /// 通过全部提交校验后进入等待状态 此时不扣除技能冷却
    /// </summary>
    public bool TryUseSkill(RunTimeSkillData data, GameObject target, out SkillUseFailReason reason)
    {
        reason = CanUseSkill(data, target);
        if (reason != SkillUseFailReason.None) return false;
        PrepareResources(data, out preparedTimeline, out preparedBehavior);
        CurrentRunTimeSkillData = RegisterSkill(data);
        requestedTarget = target.GetComponentInParent<GameActor>();
        requestElapsed = 0;
        ExecutionId = ++executionSeed;
        State = SkillRunState.Pending;
        return true;
    }

    /// <summary>
    /// 查询技能是否可提交 不改变执行状态或消费冷却
    /// </summary>
    public SkillUseFailReason CanUseSkill(RunTimeSkillData data, GameObject target)
    {
        if (NeedUseSkill) return SkillUseFailReason.Busy;
        SkillUseFailReason reason = ValidateRoleAndTarget(data, target);
        if (reason != SkillUseFailReason.None) return reason;
        if (GetCooldownRemaining(data) > 0) return SkillUseFailReason.OnCooldown;
        if (CommonCooldownRemaining > 0) return SkillUseFailReason.OnCommonCooldown;
        return PrepareResources(data, out _, out _);
    }

    /// <summary>
    /// 校验施法角色 目标存活与当前水平距离
    /// </summary>
    private SkillUseFailReason ValidateRoleAndTarget(RunTimeSkillData data, GameObject target)
    {
        if (role == null || !role.isActiveAndEnabled || !role.IsCanAtk()) return SkillUseFailReason.InvalidRole;
        if (data?.skill == null || data.skill.TimelineORbehavior < 0 || data.skill.TimelineORbehavior > 1)
            return SkillUseFailReason.InvalidSkill;
        GameActor actor = target != null ? target.GetComponentInParent<GameActor>() : null;
        if (actor == null || actor == role || !actor.isActiveAndEnabled || !actor.IsCanBeHit())
            return SkillUseFailReason.InvalidTarget;
        Vector3 offset = actor.transform.position - role.transform.position;
        offset.y = 0;
        if (offset.sqrMagnitude > Mathf.Pow(Mathf.Max(0, data.skill.AtkRang), 2)) return SkillUseFailReason.OutOfRange;
        return SkillUseFailReason.None;
    }

    /// <summary>
    /// 按资源名校验并缓存执行资产 空名称才允许回退到旧技能树
    /// </summary>
    private SkillUseFailReason PrepareResources(RunTimeSkillData data, out PlayableAsset timeline, out ExternalBehavior behavior)
    {
        timeline = null;
        behavior = null;
        string name = data.skill.ResName;
        if (data.skill.TimelineORbehavior == 0)
        {
            if (skillTimeLine == null) return SkillUseFailReason.InvalidExecutor;
            if (string.IsNullOrWhiteSpace(name)) return SkillUseFailReason.MissingResource;
            if (!timelines.TryGetValue(name, out timeline))
            {
                timeline = Resources.Load<PlayableAsset>($"Skills/{name}");
                if (timeline == null) return SkillUseFailReason.MissingResource;
                timelines.Add(name, timeline);
            }
            if (timeline.duration <= 0 || double.IsInfinity(timeline.duration) || double.IsNaN(timeline.duration))
                return SkillUseFailReason.InvalidExecutor;
            foreach (PlayableBinding binding in timeline.outputs)
            {
                if (binding.outputTargetType == typeof(Animator) && role.Animator == null)
                    return SkillUseFailReason.MissingBinding;
                if (binding.outputTargetType != null && binding.outputTargetType != typeof(Animator) &&
                    !binding.outputTargetType.IsInstanceOfType(role)) return SkillUseFailReason.MissingBinding;
            }
        }
        else
        {
            if (skillTree == null || !skillTree.isActiveAndEnabled || skillTree == role.AIBehavior)
                return SkillUseFailReason.InvalidExecutor;
            if (string.IsNullOrWhiteSpace(name)) behavior = legacyBehavior;
            else if (!behaviors.TryGetValue(name, out behavior))
            {
                behavior = Resources.Load<ExternalBehavior>($"Skills/{name}");
                if (behavior == null) return SkillUseFailReason.MissingResource;
                behaviors.Add(name, behavior);
            }
            BehaviorSource source = behavior != null ? behavior.GetBehaviorSource() : skillTree.GetBehaviorSource();
            source.Owner = behavior != null ? (IBehavior)behavior : skillTree;
            source.CheckForSerialization(false, null);
            if (source.RootTask == null) return SkillUseFailReason.InvalidExecutor;
        }
        return SkillUseFailReason.None;
    }
    #endregion

    #region 执行与结束
    /// <summary>
    /// AI 调度节点启动指定编号的请求 再次校验后才计入技能冷却
    /// </summary>
    public bool TryBeginSkill(int executionId)
    {
        if (State != SkillRunState.Pending || ExecutionId != executionId) return false;
        SkillUseFailReason reason = ValidateRoleAndTarget(CurrentRunTimeSkillData,
            requestedTarget != null ? requestedTarget.gameObject : null);
        if (reason == SkillUseFailReason.None && GetCooldownRemaining(CurrentRunTimeSkillData) > 0)
            reason = SkillUseFailReason.OnCooldown;
        if (reason == SkillUseFailReason.None)
            reason = PrepareResources(CurrentRunTimeSkillData, out preparedTimeline, out preparedBehavior);
        if (reason != SkillUseFailReason.None)
        {
            EndExecution(executionId, SkillRunResult.Cancelled, reason);
            return false;
        }
        RunTimeSkillData data = CurrentRunTimeSkillData;
        float previousCooldown = data.currentCD;
        try
        {
            if (IsTimeLine)
            {
                skillTimeLine.playableAsset = preparedTimeline;
                skillTimeLine.extrapolationMode = DirectorWrapMode.None;
                foreach (PlayableBinding binding in preparedTimeline.outputs)
                {
                    if (binding.outputTargetType == null) continue;
                    skillTimeLine.SetGenericBinding(binding.sourceObject,
                        binding.outputTargetType == typeof(Animator) ? (UnityEngine.Object)role.Animator : role);
                    bindings.Add(binding.sourceObject);
                }
                timelineStopped = director => EndExecution(executionId, SkillRunResult.Completed, SkillUseFailReason.None, false);
                skillTimeLine.stopped += timelineStopped;
                skillTimeLine.time = 0;
            }
            else
            {
                skillTree.DisableBehavior(false);
                skillTree.ExternalBehavior = preparedBehavior;
                skillTree.RestartWhenComplete = false;
                skillTree.PauseWhenDisabled = false;
                behaviorEnded = behavior => EndExecution(executionId,
                    behavior.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Success ? SkillRunResult.Completed : SkillRunResult.Failed,
                    behavior.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Success ? SkillUseFailReason.None : SkillUseFailReason.ExecutionFailed, false);
                skillTree.OnBehaviorEnd += behaviorEnded;
            }
            State = SkillRunState.Executing;
            data.ToCd();
            if (IsTimeLine) skillTimeLine.Play();
            else skillTree.EnableBehavior();
            return true;
        }
        catch (Exception exception)
        {
            data.currentCD = previousCooldown;
            EndExecution(executionId, SkillRunResult.Failed, SkillUseFailReason.ExecutionFailed, true, false);
            Debug.LogException(exception, role);
            return false;
        }
    }

    /// <summary>
    /// 正常结束兼容入口 等待阶段调用则视为取消
    /// </summary>
    public void SkillEnd()
    {
        EndExecution(ExecutionId, IsExecuting ? SkillRunResult.Completed : SkillRunResult.Cancelled, SkillUseFailReason.None);
    }

    /// <summary>
    /// 主动打断 等待阶段取消不会启动公共间隔
    /// </summary>
    public void BreakSkill()
    {
        EndExecution(ExecutionId, IsExecuting ? SkillRunResult.Interrupted : SkillRunResult.Cancelled, SkillUseFailReason.None);
    }

    /// <summary>
    /// 只有匹配的当前请求能结束 先移除事件再停止执行器
    /// </summary>
    internal void EndExecution(int executionId, SkillRunResult result, SkillUseFailReason reason,
        bool stopExecutor = true, bool? startedOverride = null)
    {
        if (!NeedUseSkill || ExecutionId != executionId) return;
        bool started = startedOverride ?? IsExecuting;
        bool wasTimeline = IsTimeLine;
        var outcome = new SkillExecutionResult(executionId, CurrentRunTimeSkillData, result, reason, started);
        State = SkillRunState.Idle;
        CurrentRunTimeSkillData = null;
        requestedTarget = null;
        if (skillTimeLine != null && timelineStopped != null) skillTimeLine.stopped -= timelineStopped;
        if (skillTree != null && behaviorEnded != null) skillTree.OnBehaviorEnd -= behaviorEnded;
        timelineStopped = null;
        behaviorEnded = null;
        if (stopExecutor)
        {
            if (wasTimeline && skillTimeLine != null) skillTimeLine.Stop();
            if (!wasTimeline && skillTree != null) skillTree.DisableBehavior(false);
        }
        if (skillTimeLine != null)
            foreach (UnityEngine.Object binding in bindings) skillTimeLine.ClearGenericBinding(binding);
        bindings.Clear();
        preparedTimeline = null;
        preparedBehavior = null;
        if (started) StartCommonCooldown();
        LastResult = outcome;
        OnSkillEnded?.Invoke(outcome);
    }

    /// <summary>
    /// 兼容公共间隔入口 已处于间隔时不重复延长
    /// </summary>
    public void StartCommonCooldown()
    {
        if (!NeedUseSkill && CommonCooldownRemaining <= 0) CommonCooldownRemaining = commonCooldownDuration;
    }

    /// <summary>
    /// 角色重置时清除冷却与结果 保留递增编号以隔离旧回调
    /// </summary>
    public void Reset()
    {
        BreakSkill();
        foreach (RunTimeSkillData data in registeredSkills.Values) data.currentCD = 0;
        CommonCooldownRemaining = 0;
        LastResult = default;
    }

    /// <summary>
    /// 每个有效帧更新冷却并检查等待期间的目标和时限
    /// </summary>
    public void Tick(float deltaTime)
    {
        deltaTime = Mathf.Max(0, deltaTime);
        foreach (RunTimeSkillData data in registeredSkills.Values) data.UpdateCd(deltaTime);
        CommonCooldownRemaining = Mathf.Max(0, CommonCooldownRemaining - deltaTime);
        if (State != SkillRunState.Pending) return;
        requestElapsed += deltaTime;
        SkillUseFailReason reason = ValidateRoleAndTarget(CurrentRunTimeSkillData,
            requestedTarget != null ? requestedTarget.gameObject : null);
        if (reason == SkillUseFailReason.None && requestElapsed >= requestTimeout) reason = SkillUseFailReason.RequestTimeout;
        if (reason != SkillUseFailReason.None) EndExecution(ExecutionId, SkillRunResult.Cancelled, reason);
    }
    #endregion
}
