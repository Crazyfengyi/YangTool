using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能施放器 负责处理技能学习 冷却 资源和施放时机
/// </summary>
public class SkillCaster : MonoBehaviour
{
    public IReadOnlyList<SkillDefinition> Skills => skills;

    // 技能列表
    [SerializeField]
    [LabelText("技能列表")]
    private List<SkillDefinition> skills = new List<SkillDefinition>();

    // 施法时校验与扣除的资源组件
    [SerializeField] private SkillResource resource;
    // 施法世界坐标起点
    [SerializeField] private Transform castOrigin;

    // 技能开始施放时通知
    public event Action<SkillDefinition> OnCastStarted;
    // 技能效果执行完成时通知
    public event Action<SkillDefinition> OnCastCompleted;
    // 技能请求校验失败时通知
    public event Action<SkillDefinition, SkillCastFailReason> OnCastFailed;
    // 运行时施法会话创建时通知
    public event Action<SkillCastSession> OnCastSessionStarted;
    // 运行时施法会话完成时通知
    public event Action<SkillCastSession> OnCastSessionCompleted;
    // 技能效果上报实际命中时通知
    public event Action<SkillCastSession, GameObject> OnSkillHit;

    // 按技能记录基于游戏时间的冷却结束时刻
    private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();
    // 当前施法协程句柄
    private Coroutine castingRoutine;
    // 等待动画事件生效的技能请求
    private SkillCastRequest pendingAnimationEventRequest;
    // 本次等待是否已收到动画事件
    private bool receivedAnimationEvent;
    // 当前尚未完成的施法会话
    private SkillCastSession activeSession;
    // 为每次施法分配递增会话标识
    private int castSessionSeed;
    private SkillTimelinePlayer timelinePlayer; // 当前施法使用的时间线执行器

    public bool IsCasting => activeSession != null && !activeSession.IsCompleted;

    /// <summary>
    /// 在编辑器添加或重置组件时取得资源组件和默认施法起点
    /// </summary>
    private void Reset()
    {
        resource = GetComponent<SkillResource>();
        castOrigin = transform;
    }

    /// <summary>
    /// 判断技能是否存在于已学习列表
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    /// <returns>已学习时返回 true</returns>
    public bool KnowsSkill(SkillDefinition skill)
    {
        return skill != null && skills.Contains(skill);
    }

    /// <summary>
    /// 将有效且未重复的技能加入已学习列表
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    public void AddSkill(SkillDefinition skill)
    {
        if (skill != null && !skills.Contains(skill))
        {
            skills.Add(skill);
        }
    }

    /// <summary>
    /// 从已学习列表移除指定技能
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    public void RemoveSkill(SkillDefinition skill)
    {
        if (skill != null)
        {
            skills.Remove(skill);
        }
    }

    /// <summary>
    /// 校验技能请求并尝试开始一次施放
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    /// <param name="target">目标对象</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCast(SkillDefinition skill, GameObject target = null)
    {
        var origin = GetOriginPosition();
        var direction = target != null
            ? SafeDirection(target.transform.position - origin)
            : transform.forward;

        return TryCast(new SkillCastRequest
        {
            Skill = skill,
            Target = target,
            Point = target != null ? target.transform.position : origin,
            Direction = direction,
            SourceInstanceId = 0
        });
    }

    /// <summary>
    /// 构造指定世界坐标的技能请求并尝试施放
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    /// <param name="point">世界空间目标坐标</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCastAtPoint(SkillDefinition skill, Vector3 point)
    {
        return TryCast(new SkillCastRequest
        {
            Skill = skill,
            Point = point,
            Direction = SafeDirection(point - GetOriginPosition()),
            SourceInstanceId = 0
        });
    }

    /// <summary>
    /// 构造指定方向的技能请求并尝试施放
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    /// <param name="direction">世界空间方向</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCastInDirection(SkillDefinition skill, Vector3 direction)
    {
        Vector3 castDirection = SafeDirection(direction);
        return TryCast(new SkillCastRequest
        {
            Skill = skill,
            Direction = castDirection,
            Point = GetOriginPosition() + castDirection * (skill != null ? skill.Range : 0f),
            SourceInstanceId = 0
        });
    }

    /// <summary>
    /// 携带武器与实例标识向目标施放技能
    /// </summary>
    /// <param name="weapon">发起施法的武器定义</param>
    /// <param name="sourceInstanceId">来源武器的运行时实例标识</param>
    /// <param name="skill">本次操作的技能定义</param>
    /// <param name="target">目标对象</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCastFromWeapon(WeaponDefinition weapon, int sourceInstanceId, SkillDefinition skill, GameObject target = null)
    {
        var origin = GetOriginPosition();
        return TryCast(new SkillCastRequest
        {
            Skill = skill,
            Target = target,
            Point = target != null ? target.transform.position : origin,
            Direction = target != null ? SafeDirection(target.transform.position - origin) : transform.forward,
            SourceWeapon = weapon,
            SourceInstanceId = sourceInstanceId
        });
    }

    /// <summary>
    /// 携带武器与实例标识向世界坐标施放技能
    /// </summary>
    /// <param name="weapon">发起施法的武器定义</param>
    /// <param name="sourceInstanceId">来源武器的运行时实例标识</param>
    /// <param name="skill">本次操作的技能定义</param>
    /// <param name="point">世界空间目标坐标</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCastFromWeaponAtPoint(WeaponDefinition weapon, int sourceInstanceId, SkillDefinition skill, Vector3 point)
    {
        return TryCast(new SkillCastRequest
        {
            Skill = skill,
            Point = point,
            Direction = SafeDirection(point - GetOriginPosition()),
            SourceWeapon = weapon,
            SourceInstanceId = sourceInstanceId
        });
    }

    /// <summary>
    /// 携带武器与实例标识沿指定方向施放技能
    /// </summary>
    /// <param name="weapon">发起施法的武器定义</param>
    /// <param name="sourceInstanceId">来源武器的运行时实例标识</param>
    /// <param name="skill">本次操作的技能定义</param>
    /// <param name="direction">世界空间方向</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCastFromWeaponInDirection(WeaponDefinition weapon, int sourceInstanceId, SkillDefinition skill, Vector3 direction)
    {
        Vector3 castDirection = SafeDirection(direction);
        return TryCast(new SkillCastRequest
        {
            Skill = skill,
            Direction = castDirection,
            Point = GetOriginPosition() + castDirection * (skill != null ? skill.Range : 0f),
            SourceWeapon = weapon,
            SourceInstanceId = sourceInstanceId
        });
    }

    /// <summary>
    /// 校验技能请求并尝试开始一次施放
    /// </summary>
    /// <param name="request">包含技能来源与瞄准信息的请求</param>
    /// <returns>请求被接受时返回 true 不代表已经命中目标</returns>
    public bool TryCast(SkillCastRequest request)
    {
        return TryCast(request, out _);
    }

    /// <summary>
    /// 尝试施放技能 并返回本次施放的运行时会话
    /// </summary>
    public bool TryCast(SkillCastRequest request, out SkillCastSession session)
    {
        session = null;
        SkillCastFailReason failReason = CanCast(request);
        if (failReason != SkillCastFailReason.None)
        {
            OnCastFailed?.Invoke(request != null ? request.Skill : null, failReason);
            return false;
        }

        if (request.Timeline != null)
        {
            SkillTimelinePlayer player = GetComponent<SkillTimelinePlayer>();
            if (player == null || !player.CanPlay(request.Timeline))
            {
                OnCastFailed?.Invoke(request.Skill, SkillCastFailReason.InvalidTimeline);
                return false;
            }
        }

        if (request.Skill.Cost != null && resource != null)
        {
            resource.TryCost(request.Skill.Cost.Resource);
        }

        session = new SkillCastSession(++castSessionSeed, request.Skill);
        activeSession = session;
        OnCastSessionStarted?.Invoke(session);

        if (session.IsCompleted || activeSession != session) return true;
        Coroutine startedRoutine = StartCoroutine(CastRoutine(request, session));
        if (activeSession == session && !session.IsCompleted) castingRoutine = startedRoutine;
        return true;
    }

    /// <summary>
    /// 取得技能剩余冷却秒数 未处于冷却时返回零
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    public float GetCooldownRemaining(SkillDefinition skill)
    {
        if (skill == null || !cooldownEndTimes.TryGetValue(skill, out float endTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, endTime - Time.time);
    }

    /// <summary>
    /// 在明确重置战斗状态时清除该施法器的技能冷却
    /// </summary>
    public void ResetCooldowns()
    {
        cooldownEndTimes.Clear();
    }

    /// <summary>
    /// 取得配置的施法起点 缺失时使用角色位置
    /// </summary>
    public Vector3 GetOriginPosition()
    {
        return castOrigin != null ? castOrigin.position : transform.position;
    }

    /// <summary>
    /// 标记收到待处理施法请求的动画事件
    /// </summary>
    public void TriggerSkillEffect()
    {
        if (pendingAnimationEventRequest != null)
        {
            receivedAnimationEvent = true;
        }
    }

    /// <summary>
    /// 提供可供动画事件调用的技能生效入口
    /// </summary>
    public void AnimationEvent_TriggerSkillEffect()
    {
        TriggerSkillEffect();
    }

    /// <summary>
    /// 处理伤害效果上报的实际命中
    /// </summary>
    internal void ReportSkillHit(SkillContext context, GameObject target)
    {
        ReportSkillHit(context, target, target != null ? target.transform.position : Vector3.zero);
    }

    /// <summary>
    /// 处理伤害效果上报的实际命中与命中坐标
    /// </summary>
    internal void ReportSkillHit(SkillContext context, GameObject target, Vector3 hitPoint)
    {
        SkillCastSession session = context != null ? context.Session : null;
        if (session != null && session.RegisterHit(target, hitPoint))
        {
            OnSkillHit?.Invoke(session, target);
        }
    }

    /// <summary>
    /// 等待施法时间或动画事件后执行效果并结束施法会话
    /// </summary>
    /// <param name="request">包含技能来源与瞄准信息的请求</param>
    /// <param name="session">本次施法的运行时会话</param>
    private IEnumerator CastRoutine(SkillCastRequest request, SkillCastSession session)
    {
        SkillDefinition skill = request.Skill;
        OnCastStarted?.Invoke(skill);
        if (session.IsCompleted || activeSession != session) yield break;

        if (request.Timeline != null)
        {
            timelinePlayer = GetComponent<SkillTimelinePlayer>();
            if (timelinePlayer == null || !timelinePlayer.Begin(request.Timeline, CreateContext(request, session)))
            {
                session.WasInterrupted = true;
                session.IsCompleted = true;
                if (activeSession == session) activeSession = null;
                OnCastSessionCompleted?.Invoke(session);
                yield break;
            }
            yield return new WaitUntil(() => session.IsCompleted || !timelinePlayer.IsPlaying);
            if (session.IsCompleted) yield break;
            if (request.SourceWeapon == null) StartCooldown(skill, request.CooldownOverride);
            session.IsCompleted = true;
            if (activeSession == session) activeSession = null;
            castingRoutine = null;
            OnCastCompleted?.Invoke(skill);
            OnCastSessionCompleted?.Invoke(session);
            yield break;
        }

        if (skill.ExecuteOnAnimationEvent)
        {
            pendingAnimationEventRequest = request;
            receivedAnimationEvent = false;

            if (skill.AnimationEventTimeout > 0f)
            {
                float timeoutEnd = Time.time + skill.AnimationEventTimeout;
                yield return new WaitUntil(() => receivedAnimationEvent || Time.time >= timeoutEnd);
            }
            else
            {
                yield return new WaitUntil(() => receivedAnimationEvent);
            }

            pendingAnimationEventRequest = null;
        }
        else if (skill.CastTime > 0f)
        {
            yield return new WaitForSeconds(skill.CastTime);
        }

        ExecuteSkill(request, session);
        if (request.SourceWeapon == null)
        {
            StartCooldown(skill);
        }
        session.IsCompleted = true;
        if (activeSession == session)
        {
            activeSession = null;
        }
        castingRoutine = null;
        OnCastCompleted?.Invoke(skill);
        OnCastSessionCompleted?.Invoke(session);
    }

    /// <summary>
    /// 为本次请求创建技能上下文并依次执行配置的效果
    /// </summary>
    /// <param name="request">包含技能来源与瞄准信息的请求</param>
    /// <param name="session">本次施法的运行时会话</param>
    private void ExecuteSkill(SkillCastRequest request, SkillCastSession session)
    {
        var context = CreateContext(request, session);

        for (int i = 0; i < request.Skill.Effects.Count; i++)
        {
            SkillEffect effect = request.Skill.Effects[i];
            if (effect != null)
            {
                effect.Execute(context);
            }
        }
    }

    /// <summary>
    /// 为普通技能和时间线技能构建相同的独立施法上下文
    /// </summary>
    private SkillContext CreateContext(SkillCastRequest request, SkillCastSession session)
    {
        return new SkillContext
        {
            Caster = this,
            Skill = request.Skill,
            Target = request.Target,
            Point = request.Point,
            Direction = SafeDirection(request.Direction),
            OriginPoint = GetOriginPosition(),
            SourceWeapon = request.SourceWeapon,
            SourceInstanceId = request.SourceInstanceId,
            Session = session,
            TableDamage = request.TableDamage
        };
    }

    /// <summary>
    /// 按当前游戏时间记录技能冷却结束时刻
    /// </summary>
    /// <param name="skill">本次操作的技能定义</param>
    private void StartCooldown(SkillDefinition skill, float? duration = null)
    {
        if (skill != null && (duration ?? skill.Cooldown) > 0f)
        {
            cooldownEndTimes[skill] = Time.time + (duration ?? skill.Cooldown);
        }
    }

    /// <summary>
    /// 依次校验技能学习状态 施法占用 冷却 资源与目标
    /// </summary>
    /// <param name="request">包含技能来源与瞄准信息的请求</param>
    private SkillCastFailReason CanCast(SkillCastRequest request)
    {
        if (request == null || request.Skill == null)
        {
            return SkillCastFailReason.UnknownSkill;
        }

        if (!KnowsSkill(request.Skill))
        {
            return SkillCastFailReason.UnknownSkill;
        }

        if (IsCasting)
        {
            return SkillCastFailReason.AlreadyCasting;
        }

        if (request.SourceWeapon == null && GetCooldownRemaining(request.Skill) > 0f)
        {
            return SkillCastFailReason.OnCooldown;
        }

        float cost = request.Skill.Cost != null ? request.Skill.Cost.Resource : 0f;
        if (resource != null && !resource.HasEnough(cost))
        {
            return SkillCastFailReason.NotEnoughResource;
        }

        return IsTargetValid(request) ? SkillCastFailReason.None : SkillCastFailReason.InvalidTarget;
    }

    /// <summary>
    /// 根据技能目标类型校验目标对象 坐标或方向
    /// </summary>
    /// <param name="request">包含技能来源与瞄准信息的请求</param>
    /// <returns>满足目标类型要求时返回 true</returns>
    private bool IsTargetValid(SkillCastRequest request)
    {
        SkillDefinition skill = request.Skill;
        if (skill.TargetType == SkillTargetType.Self)
        {
            return true;
        }

        if (skill.TargetType == SkillTargetType.Unit)
        {
            return request.Target != null && IsWithinRange(request.Target.transform.position, request.RangeOverride ?? skill.Range);
        }

        if (skill.TargetType == SkillTargetType.Point)
        {
            return IsWithinRange(request.Point, request.RangeOverride ?? skill.Range);
        }

        return request.Direction.sqrMagnitude > 0f;
    }

    /// <summary>
    /// 判断世界坐标是否在施法距离内 非正距离表示不限制
    /// </summary>
    /// <param name="point">世界空间目标坐标</param>
    /// <param name="range">施法距离 非正值表示不限距离</param>
    /// <returns>在范围内或不限制距离时返回 true</returns>
    private bool IsWithinRange(Vector3 point, float range)
    {
        if (range <= 0f)
        {
            return true;
        }

        return Vector3.Distance(GetOriginPosition(), point) <= range;
    }

    /// <summary>
    /// 归一化非零方向 零向量回退到角色前方
    /// </summary>
    /// <param name="direction">世界空间方向</param>
    private Vector3 SafeDirection(Vector3 direction)
    {
        return direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
    }

    /// <summary>
    /// 停止施法协程并清理当前会话与动画事件等待状态
    /// </summary>
    private void OnDisable()
    {
        CancelCast();
    }

    /// <summary>
    /// 中止当前施法并通知等待该会话的调用方
    /// </summary>
    public void CancelCast()
    {
        SkillCastSession session = activeSession;
        if (session == null || session.IsCompleted) return;
        if (castingRoutine != null)
        {
            StopCoroutine(castingRoutine);
        }

        session.WasInterrupted = true;
        session.IsCompleted = true;

        castingRoutine = null;
        activeSession = null;
        pendingAnimationEventRequest = null;
        receivedAnimationEvent = false;
        if (timelinePlayer != null) timelinePlayer.Stop();
        timelinePlayer = null;
        OnCastSessionCompleted?.Invoke(session);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器中使用列表首个技能向首个生命组件目标测试施放
    /// </summary>
    [Button("测试技能")]
    public void UseSkill()
    {
        SimpleHealth[] targets = GameObject.FindObjectsOfType<SimpleHealth>();
        if (targets.Length == 0 || skills.Count == 0)
        {
            return;
        }

        bool result = TryCast(skills[0], targets[0].gameObject);
        Debug.Log($"技能释放:{result}");
    }
#endif
}
