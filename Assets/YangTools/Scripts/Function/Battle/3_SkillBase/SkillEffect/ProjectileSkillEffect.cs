using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 投射物技能效果 支持排列 轨迹和生命周期派生
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Effects/Projectile")]
public class ProjectileSkillEffect : SkillEffect
{
    // 投射物预制体
    [Title("基础配置"), LabelText("投射物预制体")]
    [SerializeField] private SkillProjectile projectilePrefab;
    // 弹体飞行速度
    [LabelText("飞行速度"), MinValue(0)] [SerializeField] private float speed = 12f;
    // 基础伤害
    [LabelText("基础伤害")] [SerializeField] private float baseDamage = 10f;
    // 属性加成系数
    [LabelText("属性加成系数")] [SerializeField] private float powerScale = 1f;
    // 加成属性
    [LabelText("加成属性")] [SerializeField] private SkillPowerType powerType = SkillPowerType.Attack;
    // 弹体最大存活秒数 非正值不按寿命消亡
    [LabelText("存活时间（秒）"), PropertyTooltip("小于等于零时不按寿命消亡 且不触发飞行节点")]
    [SerializeField] private float lifeSeconds = 5f;
    // 投射物出生位置与排列方式配置
    [Title("发射配置"), InlineProperty, HideLabel] [SerializeField] private ProjectileEmissionSettings emission = new ProjectileEmissionSettings();
    // 投射物运动轨迹配置
    [Title("飞行配置"), InlineProperty, HideLabel] [SerializeField] private ProjectileFlightSettings flight = new ProjectileFlightSettings();
    // 目标层级
    [Title("命中配置"), LabelText("目标层级")] [SerializeField] private LayerMask targetMask = ~0;
    // 阻挡层级
    [LabelText("阻挡层级")] [SerializeField] private LayerMask blockingMask;
    // 额外穿透次数
    [LabelText("额外穿透次数"), MinValue(0)] [SerializeField] private int piercing;
    // 扫掠半径
    [LabelText("扫掠半径"), MinValue(0.001f)] [SerializeField] private float sweepRadius = 0.1f;
    // 投射物生命周期触发配置
    [Title("生命周期"), InlineProperty, HideLabel, InfoBox("无限寿命不会执行飞行节点效果", "InvalidNodes")]
    [SerializeField] private ProjectileLifecycleSettings lifecycle = new ProjectileLifecycleSettings();
    private bool InvalidNodes => lifeSeconds <= 0 && lifecycle.onNode.Count > 0;
    internal ProjectileEmissionSettings Emission => emission;
    internal ProjectileFlightSettings Flight => flight;

    /// <summary>
    /// 开始一条独立的根施放链
    /// </summary>
    public override void Execute(SkillContext context)
    {
        if (context == null) return;
        var budget = new ProjectileChainBudget { remaining = Mathf.Max(1, lifecycle.maxSpawnCount), maxDepth = Mathf.Max(0, lifecycle.maxDepth) };
        Emit(context, budget, 0, new HashSet<GameObject>());
    }

    /// <summary>
    /// 按模式排列生成弹体并共享派生预算
    /// </summary>
    internal void Emit(SkillContext context, ProjectileChainBudget budget, int depth, HashSet<GameObject> history)
    {
        if (projectilePrefab == null || context == null || depth > budget.maxDepth) return;
        int count = Mathf.Clamp(emission.count, 1, 512);
        bool ringRows = emission.mode == ProjectileEmissionMode.CircleRow || emission.mode == ProjectileEmissionMode.LaunchCircleRow;
        int rows = ringRows ? Mathf.Clamp(emission.rows, 1, 512) : 1;
        Vector3 forward = ProjectileMotion.Horizontal(context.Direction);
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        for (int row = 0; row < rows; row++)
        for (int i = 0; i < count; i++)
        {
            if (!budget.Consume(depth)) return;
            Vector3 position = context.OriginPoint;
            Vector3 direction = forward;
            float radius = Mathf.Max(0.01f, emission.radius + row * emission.rowSpacing);
            float angle = 360f * i / count;
            switch (emission.mode)
            {
                case ProjectileEmissionMode.Linear: position += right * ((i - (count - 1) * 0.5f) * emission.spacing); break;
                case ProjectileEmissionMode.Arc: direction = Quaternion.AngleAxis(count == 1 ? 0 : -emission.angle * 0.5f + emission.angle * i / (count - 1), Vector3.up) * forward; break;
                case ProjectileEmissionMode.AreaRandom:
                    Vector2 offset = Random.insideUnitCircle * emission.randomRadius;
                    position += new Vector3(offset.x, 0, offset.y);
                    direction = Quaternion.AngleAxis(Random.Range(-emission.angle, emission.angle) * 0.5f, Vector3.up) * forward;
                    break;
                default:
                    direction = Quaternion.AngleAxis(angle, Vector3.up) * forward;
                    if (emission.mode == ProjectileEmissionMode.OutsideIn || emission.mode == ProjectileEmissionMode.CircleRow) position += direction * radius;
                    if (emission.mode == ProjectileEmissionMode.OutsideIn) direction = -direction;
                    break;
            }
            var runtime = new ProjectileRuntime { context = context, speed = speed, damage = baseDamage, scale = powerScale,
                power = powerType, lifetime = lifeSeconds, flight = JsonUtility.FromJson<ProjectileFlightSettings>(JsonUtility.ToJson(flight)),
                lifecycle = lifecycle.Snapshot(), targetMask = targetMask,
                blockingMask = blockingMask, piercing = Mathf.Max(0, piercing), sweepRadius = Mathf.Max(0.001f, sweepRadius),
                center = context.OriginPoint, direction = direction, budget = budget, depth = depth,
                stopDistance = emission.mode == ProjectileEmissionMode.LaunchCircleRow ? radius : 0,
                history = new HashSet<GameObject>(history) };
            var projectile = Instantiate(projectilePrefab, position, Quaternion.LookRotation(direction));
            projectile.Initialize(runtime);
        }
    }
}
