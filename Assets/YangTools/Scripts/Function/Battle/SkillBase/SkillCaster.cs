using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能施放器，负责处理技能学习、冷却、资源和施放时机。
/// </summary>
public class SkillCaster : MonoBehaviour
{
    public IReadOnlyList<SkillDefinition> Skills => skills;

    [SerializeField]
    [LabelText("技能列表")]
    private List<SkillDefinition> skills = new List<SkillDefinition>();

    [SerializeField] private SkillResource resource;
    [SerializeField] private Transform castOrigin;

    public event Action<SkillDefinition> OnCastStarted;
    public event Action<SkillDefinition> OnCastCompleted;
    public event Action<SkillDefinition, SkillCastFailReason> OnCastFailed;

    private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();
    private Coroutine castingRoutine;
    private SkillCastRequest pendingAnimationEventRequest;
    private bool receivedAnimationEvent;

    public bool IsCasting => castingRoutine != null;

    private void Reset()
    {
        resource = GetComponent<SkillResource>();
        castOrigin = transform;
    }

    public bool KnowsSkill(SkillDefinition skill)
    {
        return skill != null && skills.Contains(skill);
    }

    public void AddSkill(SkillDefinition skill)
    {
        if (skill != null && !skills.Contains(skill))
        {
            skills.Add(skill);
        }
    }

    public void RemoveSkill(SkillDefinition skill)
    {
        if (skill != null)
        {
            skills.Remove(skill);
        }
    }

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

    public bool TryCast(SkillCastRequest request)
    {
        SkillCastFailReason failReason = CanCast(request);
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

    public float GetCooldownRemaining(SkillDefinition skill)
    {
        if (skill == null || !cooldownEndTimes.TryGetValue(skill, out float endTime))
        {
            return 0f;
        }

        return Mathf.Max(0f, endTime - Time.time);
    }

    public Vector3 GetOriginPosition()
    {
        return castOrigin != null ? castOrigin.position : transform.position;
    }

    public void TriggerSkillEffect()
    {
        if (pendingAnimationEventRequest != null)
        {
            receivedAnimationEvent = true;
        }
    }

    public void AnimationEvent_TriggerSkillEffect()
    {
        TriggerSkillEffect();
    }

    private IEnumerator CastRoutine(SkillCastRequest request)
    {
        SkillDefinition skill = request.Skill;
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
        if (request.SourceWeapon == null)
        {
            StartCooldown(skill);
        }
        OnCastCompleted?.Invoke(skill);
        castingRoutine = null;
    }

    private void ExecuteSkill(SkillCastRequest request)
    {
        var context = new SkillContext
        {
            Caster = this,
            Skill = request.Skill,
            Target = request.Target,
            Point = request.Point,
            Direction = SafeDirection(request.Direction),
            OriginPoint = GetOriginPosition(),
            SourceWeapon = request.SourceWeapon,
            SourceInstanceId = request.SourceInstanceId
        };

        for (int i = 0; i < request.Skill.Effects.Count; i++)
        {
            SkillEffect effect = request.Skill.Effects[i];
            if (effect != null)
            {
                effect.Execute(context);
            }
        }
    }

    private void StartCooldown(SkillDefinition skill)
    {
        if (skill != null && skill.Cooldown > 0f)
        {
            cooldownEndTimes[skill] = Time.time + skill.Cooldown;
        }
    }

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

    private bool IsTargetValid(SkillCastRequest request)
    {
        SkillDefinition skill = request.Skill;
        if (skill.TargetType == SkillTargetType.Self)
        {
            return true;
        }

        if (skill.TargetType == SkillTargetType.Unit)
        {
            return request.Target != null && IsWithinRange(request.Target.transform.position, skill.Range);
        }

        if (skill.TargetType == SkillTargetType.Point)
        {
            return IsWithinRange(request.Point, skill.Range);
        }

        return request.Direction.sqrMagnitude > 0f;
    }

    private bool IsWithinRange(Vector3 point, float range)
    {
        if (range <= 0f)
        {
            return true;
        }

        return Vector3.Distance(GetOriginPosition(), point) <= range;
    }

    private Vector3 SafeDirection(Vector3 direction)
    {
        return direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
    }

#if UNITY_EDITOR
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
