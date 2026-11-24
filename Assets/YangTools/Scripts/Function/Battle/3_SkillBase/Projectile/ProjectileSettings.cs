using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 子弹初始排列方式
/// </summary>
public enum ProjectileEmissionMode
{
    [LabelText("平行排列")] Linear,
    [LabelText("全周等分")] Directional,
    [LabelText("扇形")] Arc,
    [LabelText("范围随机")] AreaRandom,
    [LabelText("由外向内")] OutsideIn,
    [LabelText("多排圆环")] CircleRow,
    [LabelText("发射到环后停留")] LaunchCircleRow
}

/// <summary>
/// 子弹运动方式
/// </summary>
public enum ProjectileFlightMode
{
    [LabelText("直线")] Linear,
    [LabelText("定点抛物线")] Lobbed,
    [LabelText("螺旋")] Orbit,
    [LabelText("正弦")] Sine,
    [LabelText("追踪")] Tracking,
    [LabelText("重力抛射")] Lobbed2,
    [LabelText("圆周")] Circle,
    [LabelText("直飞转圆周")] LaunchCircle,
    [LabelText("直去直回")] FlyAwayAndBack,
    [LabelText("发射时瞄准")] SpecifyTargetFlyLine
}

/// <summary>
/// 发射排列参数
/// </summary>
[Serializable]
public sealed class ProjectileEmissionSettings
{
    [LabelText("发射模式"), InfoBox("到环后停留优先于飞行模式", "StopsAtRing")] public ProjectileEmissionMode mode;
    [LabelText("每排数量"), MinValue(1)] public int count = 1;
    [LabelText("横向间距"), ShowIf("mode", ProjectileEmissionMode.Linear)] public float spacing = 0.5f;
    [LabelText("展开角度"), ShowIf("UsesAngle"), Range(0, 360)] public float angle = 60f;
    [LabelText("首环半径"), ShowIf("UsesRadius"), MinValue(0.01f)] public float radius = 3f;
    [LabelText("排数"), ShowIf("UsesRows"), MinValue(1)] public int rows = 1;
    [LabelText("半径间距"), ShowIf("UsesRows"), MinValue(0)] public float rowSpacing = 2f;
    [LabelText("随机位置半径"), ShowIf("mode", ProjectileEmissionMode.AreaRandom), MinValue(0)] public float randomRadius = 1f;
    private bool StopsAtRing => mode == ProjectileEmissionMode.LaunchCircleRow;
    private bool UsesAngle => mode == ProjectileEmissionMode.Arc || mode == ProjectileEmissionMode.AreaRandom;
    private bool UsesRows => mode == ProjectileEmissionMode.CircleRow || mode == ProjectileEmissionMode.LaunchCircleRow;
    private bool UsesRadius => UsesRows || mode == ProjectileEmissionMode.OutsideIn;
}

/// <summary>
/// 飞行模式参数
/// </summary>
[Serializable]
public sealed class ProjectileFlightSettings
{
    [LabelText("飞行模式")] public ProjectileFlightMode mode;
    [LabelText("加速度"), ShowIf("mode", ProjectileFlightMode.Linear)] public float acceleration;
    [LabelText("弧线时长（秒）"), ShowIf("mode", ProjectileFlightMode.Lobbed), MinValue(0.01f)] public float duration = 1.5f;
    [LabelText("弧高"), ShowIf("mode", ProjectileFlightMode.Lobbed)] public float height = 3f;
    [LabelText("半径"), ShowIf("UsesCircle"), MinValue(0.01f)] public float radius = 3f;
    [LabelText("角速度（度每秒）"), ShowIf("UsesCircle")] public float angularSpeed = 120f;
    [LabelText("径向速度"), ShowIf("mode", ProjectileFlightMode.Orbit)] public float radialSpeed = 1f;
    [LabelText("振幅"), ShowIf("mode", ProjectileFlightMode.Sine)] public float amplitude = 1f;
    [LabelText("频率（赫兹）"), ShowIf("mode", ProjectileFlightMode.Sine), MinValue(0)] public float frequency = 2f;
    [LabelText("追踪转向速度（度每秒）"), ShowIf("mode", ProjectileFlightMode.Tracking), MinValue(0)] public float turnSpeed = 900f;
    [LabelText("抛射仰角"), ShowIf("mode", ProjectileFlightMode.Lobbed2), Range(1, 89)] public float elevation = 45f;
    [LabelText("重力"), ShowIf("mode", ProjectileFlightMode.Lobbed2), MinValue(0.01f)] public float gravity = 9.81f;
    [LabelText("直飞距离"), ShowIf("UsesDistance"), MinValue(0.01f)] public float distance = 6f;
    [LabelText("转向时长（秒）"), ShowIf("mode", ProjectileFlightMode.LaunchCircle), MinValue(0.01f)] public float turnDuration = 0.5f;
    private bool UsesCircle => mode == ProjectileFlightMode.Circle || mode == ProjectileFlightMode.Orbit || mode == ProjectileFlightMode.LaunchCircle;
    private bool UsesDistance => mode == ProjectileFlightMode.LaunchCircle || mode == ProjectileFlightMode.FlyAwayAndBack;
}

/// <summary>
/// 生命周期效果和派生限制
/// </summary>
[Serializable]
public sealed class ProjectileLifecycleSettings
{
    [LabelText("命中效果")] public List<SkillEffect> onHit = new List<SkillEffect>();
    [LabelText("消亡效果")] public List<SkillEffect> onDeath = new List<SkillEffect>();
    [LabelText("飞行节点效果")] public List<SkillEffect> onNode = new List<SkillEffect>();
    [LabelText("寿命均分段数"), MinValue(1)] public int segments = 1;
    [LabelText("节点方向偏移（度）")] public float nodeAngle;
    [LabelText("命中派生寻敌半径"), MinValue(0)] public float searchRadius = 10f;
    [LabelText("最大派生深度"), MinValue(0)] public int maxDepth = 4;
    [LabelText("根施放累计生成上限"), MinValue(1)] public int maxSpawnCount = 512;

    /// <summary>
    /// 复制列表以隔离飞行中的弹体与配置编辑
    /// </summary>
    internal ProjectileLifecycleSettings Snapshot()
    {
        var copy = (ProjectileLifecycleSettings)MemberwiseClone();
        copy.onHit = new List<SkillEffect>(onHit);
        copy.onDeath = new List<SkillEffect>(onDeath);
        copy.onNode = new List<SkillEffect>(onNode);
        return copy;
    }
}

/// <summary>
/// 单次根施放共享的派生预算
/// </summary>
internal sealed class ProjectileChainBudget
{
    internal int remaining;
    internal int maxDepth;
    /// <summary>
    /// 消耗一个生成名额
    /// </summary>
    internal bool Consume(int depth)
    {
        if (depth > maxDepth || remaining <= 0) return false;
        remaining--;
        return true;
    }
}

/// <summary>
/// 每发子弹独立的运行数据
/// </summary>
internal sealed class ProjectileRuntime
{
    internal SkillContext context;
    internal float speed, damage, scale, lifetime;
    internal SkillPowerType power;
    internal ProjectileFlightSettings flight;
    internal ProjectileLifecycleSettings lifecycle;
    internal LayerMask targetMask = ~0, blockingMask;
    internal float sweepRadius = 0.1f, stopDistance;
    internal int piercing, depth;
    internal Vector3 center, direction;
    internal ProjectileChainBudget budget;
    internal HashSet<GameObject> history = new HashSet<GameObject>();

    /// <summary>
    /// 复制事件上下文并保留技能会话
    /// </summary>
    internal SkillContext ContextAt(Vector3 point, Vector3 forward, GameObject target)
    {
        return new SkillContext { Caster = context.Caster, Skill = context.Skill, SourceWeapon = context.SourceWeapon,
            SourceInstanceId = context.SourceInstanceId, Session = context.Session, Target = target,
            OriginPoint = point, Point = point, Direction = forward };
    }
}
