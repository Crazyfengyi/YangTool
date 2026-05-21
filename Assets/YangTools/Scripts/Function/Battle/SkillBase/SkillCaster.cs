using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能施放器组件，负责管理技能的施放、冷却和资源消耗
/// </summary>
public class SkillCaster : MonoBehaviour
{
    /// <summary>
    /// 获取只读的技能列表
    /// </summary>
    public IReadOnlyList<SkillDefinition> Skills => skills;
    /// <summary>
    /// 当前施放者技能列表
    /// </summary>
    [SerializeField]
    [LabelText("技能列表")]
    private List<SkillDefinition> skills = new List<SkillDefinition>();
    /// <summary>
    /// 技能资源管理器.技能施放所需的资源消耗
    /// </summary>
    [SerializeField] private SkillResource resource;
    /// <summary>
    /// 技能施放的原点位置
    /// </summary>
    [SerializeField] private Transform castOrigin;
    /// <summary>
    /// 技能施放开始事件
    /// </summary>
    public event Action<SkillDefinition> OnCastStarted;
    /// <summary>
    /// 技能施放完成事件
    /// </summary>
    public event Action<SkillDefinition> OnCastCompleted;
    /// <summary>
    /// 技能施放失败事件
    /// </summary>
    public event Action<SkillDefinition, SkillCastFailReason> OnCastFailed;

    /// <summary>
    /// 存储每个技能冷却结束时间的字典
    /// </summary>
    private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();
    /// <summary>
    /// 当前正在执行的施放协程
    /// </summary>
    private Coroutine castingRoutine;
    /// <summary>
    /// 等待动画事件触发的技能请求
    /// </summary>
    private SkillCastRequest pendingAnimationEventRequest;
    /// <summary>
    /// 是否已接收到动画事件标志
    /// </summary>
    private bool receivedAnimationEvent;
    /// <summary>
    /// 获取当前是否正在施放技能
    /// </summary>
    public bool IsCasting => castingRoutine != null;
    
    /// <summary>
    /// 自动获取必要的引用
    /// </summary>
    private void Reset()
    {
        resource = GetComponent<SkillResource>();
        castOrigin = transform;
    }
    
    #region 技能添加移除管理

    /// <summary>
    /// 检查施放者是否已学会指定技能
    /// </summary>
    /// <param name="skill">要检查的技能</param>
    /// <returns>是否已学会该技能</returns>
    public bool KnowsSkill(SkillDefinition skill)
    {
        return skill != null && skills.Contains(skill);
    }

    /// <summary>
    /// 向技能列表中添加一个新技能
    /// </summary>
    /// <param name="skill">要添加的技能</param>
    public void AddSkill(SkillDefinition skill)
    {
        if (skill != null && !skills.Contains(skill))
        {
            skills.Add(skill);
        }
    }

    /// <summary>
    /// 从技能列表中移除指定技能
    /// </summary>
    /// <param name="skill">要移除的技能</param>
    public void RemoveSkill(SkillDefinition skill)
    {
        if (skill != null)
        {
            skills.Remove(skill);
        }
    }
    
    /// <summary>
    /// 开始指定技能的冷却
    /// </summary>
    private void StartCooldown(SkillDefinition skill)
    {
        if (skill.Cooldown <= 0f) return;
        cooldownEndTimes[skill] = Time.time + skill.Cooldown;
    }
    #endregion

    #region 技能释放

    /// <summary>
    /// 尝试对目标施放指定技能
    /// </summary>
    /// <param name="skill">要施放的技能</param>
    /// <param name="target">目标对象，可为空</param>
    /// <returns>是否成功开始施放</returns>
    public bool TryCast(SkillDefinition skill, GameObject target = null)
    {
        var request = new SkillCastRequest
        {
            Skill = skill,
            Target = target,
            Point = target != null ? target.transform.position : transform.position,
            Direction = transform.forward
        };
        return TryCast(request);
    }

    /// <summary>
    /// 尝试在指定点施放技能
    /// </summary>
    /// <param name="skill">要施放的技能</param>
    /// <param name="point">目标点位置</param>
    /// <returns>是否成功开始施放</returns>
    public bool TryCastAtPoint(SkillDefinition skill, Vector3 point)
    {
        var direction = point - GetOriginPosition();
        var request = new SkillCastRequest
        {
            Skill = skill,
            Point = point,
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward
        };
        return TryCast(request);
    }

    /// <summary>
    /// 尝试在指定方向施放技能
    /// </summary>
    /// <param name="skill">要施放的技能</param>
    /// <param name="direction">施放方向</param>
    /// <returns>是否成功开始施放</returns>
    public bool TryCastInDirection(SkillDefinition skill, Vector3 direction)
    {
        var castDirection = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        var request = new SkillCastRequest
        {
            Skill = skill,
            Direction = castDirection,
            Point = GetOriginPosition() + castDirection * skill.Range
        };

        return TryCast(request);
    }

    /// <summary>
    /// 尝试根据技能请求施放技能
    /// </summary>
    /// <returns>是否成功开始施放</returns>
    public bool TryCast(SkillCastRequest request)
    {
        var failReason = CanCast(request);
        if (failReason != SkillCastFailReason.None)
        {
            OnCastFailed?.Invoke(request != null ? request.Skill : null, failReason);
            return false;
        }

        if (request.Skill.Cost != null && resource != null)
        {
            resource.TryCost(request.Skill.Cost.Resource);
        }

        castingRoutine = StartCoroutine(CastRoutine(request));
        return true;
    }
    #endregion

    #region 技能执行
    
    /// <summary>
    /// 技能施放协程，处理施放过程、等待动画事件和执行技能效果
    /// </summary>
    private IEnumerator CastRoutine(SkillCastRequest request)
    {
        var skill = request.Skill;
        OnCastStarted?.Invoke(skill);

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

        ExecuteSkill(request);
        StartCooldown(skill);
        OnCastCompleted?.Invoke(skill);
        castingRoutine = null;
    }

    /// <summary>
    /// 执行技能的所有效果
    /// </summary>
    /// <param name="request">技能施放请求</param>
    private void ExecuteSkill(SkillCastRequest request)
    {
        var context = new SkillContext
        {
            Caster = this,
            Skill = request.Skill,
            Target = request.Target,
            Point = request.Point,
            Direction = request.Direction
        };

        for (int i = 0; i < request.Skill.Effects.Count; i++)
        {
            var effect = request.Skill.Effects[i];
            if (effect != null)
            {
                effect.Execute(context);
            }
        }
    }

    #endregion
    
    #region 辅助方法

    /// <summary>
    /// 获取指定技能的剩余冷却时间
    /// </summary>
    /// <param name="skill">要查询的技能</param>
    /// <returns>剩余冷却时间(秒)</returns>
    public float GetCooldownRemaining(SkillDefinition skill)
    {
        if (skill == null) return 0f;
        if (!cooldownEndTimes.TryGetValue(skill, out var endTime)) return 0f;

        return Mathf.Max(0f, endTime - Time.time);
    }

    
    /// <summary>
    /// 检查技能是否可以施放
    /// </summary>
    /// <returns>如果可以施放返回None，否则返回失败原因</returns>
    private SkillCastFailReason CanCast(SkillCastRequest request)
    {
        if (request == null || request.Skill == null) return SkillCastFailReason.UnknownSkill;
        if (!KnowsSkill(request.Skill)) return SkillCastFailReason.UnknownSkill;
        if (IsCasting) return SkillCastFailReason.AlreadyCasting;
        if (GetCooldownRemaining(request.Skill) > 0f) return SkillCastFailReason.OnCooldown;

        var cost = request.Skill.Cost != null ? request.Skill.Cost.Resource : 0f;
        if (resource != null && !resource.HasEnough(cost))
        {
            return SkillCastFailReason.NotEnoughResource;
        }

        if (!IsTargetValid(request))
        {
            return SkillCastFailReason.InvalidTarget;
        }

        return SkillCastFailReason.None;
    }

    /// <summary>
    /// 检查技能目标是否有效
    /// </summary>
    /// <param name="request">技能施放请求</param>
    /// <returns>目标是否有效</returns>
    private bool IsTargetValid(SkillCastRequest request)
    {
        var skill = request.Skill;
        if (skill.TargetType == SkillTargetType.Self) return true;

        if (skill.TargetType == SkillTargetType.Unit)
        {
            if (request.Target == null) return false;
            return IsWithinRange(request.Target.transform.position, skill.Range);
        }

        if (skill.TargetType == SkillTargetType.Point)
        {
            return IsWithinRange(request.Point, skill.Range);
        }

        return request.Direction.sqrMagnitude > 0f;
    }

    /// <summary>
    /// 检查指定点是否在技能范围内
    /// </summary>
    /// <param name="point">要检查的点</param>
    /// <param name="range">技能范围</param>
    /// <returns>点是否在范围内</returns>
    private bool IsWithinRange(Vector3 point, float range)
    {
        if (range <= 0f) return true;
        return Vector3.Distance(GetOriginPosition(), point) <= range;
    }

    /// <summary>
    /// 获取技能施放的原点位置
    /// </summary>
    /// <returns>施放原点的世界坐标</returns>
    private Vector3 GetOriginPosition()
    {
        return castOrigin != null ? castOrigin.position : transform.position;
    }
    
    #endregion

    #region 动画事件相关

    /// <summary>
    /// 触发技能效果，通常由动画事件调用
    /// </summary>
    public void TriggerSkillEffect()
    {
        if (pendingAnimationEventRequest != null)
        {
            receivedAnimationEvent = true;
        }
    }

    /// <summary>
    /// 动画事件触发的技能效果方法
    /// </summary>
    public void AnimationEvent_TriggerSkillEffect()
    {
        TriggerSkillEffect();
    }
    
    #endregion

    #region 编辑器测试

#if UNITY_EDITOR
    [Button("测试技能")]
    public void UseSkill()
    {
        SimpleHealth[] trget = GameObject.FindObjectsOfType<SimpleHealth>();
        bool result = TryCast(skills[0], trget[0].gameObject);
        Debug.LogError($"技能释放:{result}");
    }
#endif

    #endregion
}
