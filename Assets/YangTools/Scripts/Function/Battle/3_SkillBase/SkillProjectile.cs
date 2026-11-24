using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能弹体 统一处理运动 扫掠碰撞和生命周期
/// </summary>
[RequireComponent(typeof(Collider))]
public class SkillProjectile : MonoBehaviour
{
    private ProjectileRuntime data; //独立弹体参数
    private ProjectileMotion motion; //运动状态
    private readonly HashSet<GameObject> hitTargets = new HashSet<GameObject>(); //本弹体命中去重
    private float age; //累计飞行时间
    private int nextNode = 1; //下一节点序号
    private int hitCount; //已成功命中数量
    private bool ended; //消亡去重

    /// <summary>
    /// 判断弹体是否属于指定施法者
    /// </summary>
    internal bool BelongsTo(SkillCaster caster) => data != null && data.context.Caster == caster;

    /// <summary>
    /// 保留原有直线发射入口
    /// </summary>
    public void Launch(SkillContext context, Vector3 direction, float speed, float baseDamage, float powerScale, SkillPowerType powerType, float lifeSeconds)
    {
        if (context == null) return;
        Initialize(new ProjectileRuntime { context = context, direction = direction, speed = speed, damage = baseDamage,
            scale = powerScale, power = powerType, lifetime = lifeSeconds, center = transform.position,
            flight = new ProjectileFlightSettings(), lifecycle = new ProjectileLifecycleSettings(),
            budget = new ProjectileChainBudget { remaining = 512, maxDepth = 4 } });
    }

    /// <summary>
    /// 初始化单发弹体 不修改共享配置
    /// </summary>
    internal void Initialize(ProjectileRuntime runtime)
    {
        data = runtime;
        motion = new ProjectileMotion(data, transform.position);
        age = 0; nextNode = 1; hitCount = 0; ended = false;
        hitTargets.Clear();
    }

    /// <summary>
    /// 逐帧推进运动并补发跨越的生命周期节点
    /// </summary>
    private void Update()
    {
        if (data == null || ended) return;
        float frameEnd = age + Time.deltaTime;
        if (data.lifetime > 0) frameEnd = Mathf.Min(frameEnd, data.lifetime);
        int segments = Mathf.Max(1, data.lifecycle.segments);
        while (!ended && data.lifetime > 0 && nextNode < segments && data.lifecycle.onNode.Count > 0
            && data.lifetime * nextNode / segments <= frameEnd)
        {
            float nodeAge = data.lifetime * nextNode / segments;
            Advance(nodeAge);
            nextNode++;
            if (!ended) RunEffects(data.lifecycle.onNode, null, Quaternion.AngleAxis(data.lifecycle.nodeAngle, Vector3.up) * motion.Direction, false);
        }
        if (!ended) Advance(frameEnd);
        if (!ended && data.lifetime > 0 && age >= data.lifetime) End();
    }

    /// <summary>
    /// 沿运动段检测命中并应用位移
    /// </summary>
    private void Advance(float time)
    {
        Vector3 previous = transform.position;
        motion.Step(time, Mathf.Max(0, time - age));
        age = time;
        Vector3 movement = motion.Position - previous;
        if (movement.sqrMagnitude > 0.000001f)
        {
            RaycastHit[] hits = Physics.SphereCastAll(previous, data.sweepRadius, movement.normalized,
                movement.magnitude, data.targetMask.value | data.blockingMask.value, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                transform.position = previous + movement.normalized * hit.distance;
                ProcessHit(hit.collider, hit.point);
                if (ended) return;
            }
        }
        transform.position = motion.Position;
        if (motion.Direction.sqrMagnitude > 0.000001f) transform.rotation = Quaternion.LookRotation(motion.Direction);
        if (motion.Finished) End();
    }

    /// <summary>
    /// 物理触发回调与扫掠共用去重入口
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other, other.ClosestPoint(transform.position));
    }

    /// <summary>
    /// 判断施法者及弹体自身碰撞体
    /// </summary>
    private bool Ignore(Collider other)
    {
        return other == null || other.transform.IsChildOf(transform)
            || (data.context.Caster != null && other.transform.IsChildOf(data.context.Caster.transform))
            || other.GetComponentInParent<SkillProjectile>() != null;
    }

    /// <summary>
    /// 单个伤害对象仅结算一次 并按穿透次数决定消亡
    /// </summary>
    private void ProcessHit(Collider other, Vector3 point)
    {
        if (data == null || ended || Ignore(other)) return;
        bool targetLayer = (data.targetMask.value & (1 << other.gameObject.layer)) != 0;
        var damageable = other.GetComponentInParent<IDamageable>();
        if (targetLayer && damageable != null)
        {
            var component = damageable as Component;
            var target = component != null ? component.gameObject : other.gameObject;
            if (data.history.Contains(target)) return;
            if (!hitTargets.Add(target)) return;
            var damage = SkillDamageTool.Calculate(data.context, target, data.damage, data.scale, data.power);
            if (damageable.TakeDamage(damage.Amount, data.context))
            {
                data.history.Add(target);
                data.context.ReportHit(target, point);
                Vector3 original = transform.position;
                transform.position = point;
                RunEffects(data.lifecycle.onHit, target, motion.Direction, true);
                transform.position = original;
                hitCount++;
                if (hitCount > data.piercing) End();
            }
        }
        else if ((data.blockingMask.value & (1 << other.gameObject.layer)) != 0) End();
    }

    /// <summary>
    /// 按层级与链历史选择最近可伤害目标
    /// </summary>
    private GameObject FindNextTarget()
    {
        GameObject nearest = null;
        float distance = float.PositiveInfinity;
        foreach (var collider in Physics.OverlapSphere(transform.position, Mathf.Max(0, data.lifecycle.searchRadius), data.targetMask, QueryTriggerInteraction.Collide))
        {
            if (Ignore(collider)) continue;
            var damageable = collider.GetComponentInParent<IDamageable>() as Component;
            if (damageable == null || data.history.Contains(damageable.gameObject)) continue;
            var health = damageable as SimpleHealth;
            if (health != null && health.Current <= 0) continue;
            float d = (collider.ClosestPoint(transform.position) - transform.position).sqrMagnitude;
            if (d < distance) { distance = d; nearest = damageable.gameObject; }
        }
        return nearest;
    }

    /// <summary>
    /// 派生投射物共享预算 普通效果保留触发目标
    /// </summary>
    private void RunEffects(List<SkillEffect> effects, GameObject target, Vector3 direction, bool retarget)
    {
        if (data.depth >= data.budget.maxDepth) return;
        foreach (var effect in effects)
        {
            if (effect == null) continue;
            var child = data.ContextAt(transform.position, direction, target);
            if (effect is ProjectileSkillEffect projectile)
            {
                if (retarget)
                {
                    child.Target = FindNextTarget();
                    if (child.Target == null) continue;
                    child.Point = child.Target.transform.position;
                    child.Direction = ProjectileMotion.Horizontal(child.Point - child.OriginPoint);
                }
                projectile.Emit(child, data.budget, data.depth + 1, data.history);
            }
            else effect.Execute(child);
        }
    }

    /// <summary>
    /// 自然消亡仅执行一次 外部销毁不触发派生
    /// </summary>
    private void End()
    {
        if (ended) return;
        ended = true;
        RunEffects(data.lifecycle.onDeath, null, motion.Direction, false);
        Destroy(gameObject);
    }

    /// <summary>
    /// 禁用后清理运行状态 防止场景卸载派生
    /// </summary>
    private void OnDisable()
    {
        ended = true;
        data = null;
    }
}
