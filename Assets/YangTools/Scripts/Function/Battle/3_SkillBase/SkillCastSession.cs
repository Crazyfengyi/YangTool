using UnityEngine;

/// <summary>
/// 单次技能施放的运行时会话。
/// </summary>
public sealed class SkillCastSession
{
    private int hitCount;

    public int Id { get; }
    public SkillDefinition Skill { get; }
    public bool IsCompleted { get; internal set; }
    public int HitCount => hitCount;
    public bool HasHit => HitCount > 0;
    /// <summary>
    /// 最近一次命中的世界坐标
    /// </summary>
    public Vector3 LastHitPoint { get; private set; }

    internal SkillCastSession(int id, SkillDefinition skill)
    {
        Id = id;
        Skill = skill;
    }

    /// <summary>
    /// 记录本次施放的一次有效命中。
    /// </summary>
    internal bool RegisterHit(GameObject target)
    {
        return RegisterHit(target, target != null ? target.transform.position : Vector3.zero);
    }

    /// <summary>
    /// 记录本次施放的一次有效命中及其世界坐标
    /// </summary>
    internal bool RegisterHit(GameObject target, Vector3 hitPoint)
    {
        if (target == null)
        {
            return false;
        }

        hitCount++;
        LastHitPoint = hitPoint;
        return true;
    }
}
