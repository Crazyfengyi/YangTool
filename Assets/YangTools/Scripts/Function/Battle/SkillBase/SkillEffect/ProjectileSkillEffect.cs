using UnityEngine;

/// <summary>
/// 投射物技能效果类
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Effects/Projectile")]
public class ProjectileSkillEffect : SkillEffect
{
    //投射物预制体
    [SerializeField] private SkillProjectile projectilePrefab;
    //投射物的移动速度
    [SerializeField] private float speed = 12f;
    //投射物的基础伤害值
    [SerializeField] private float baseDamage = 10f;
    //伤害的系数缩放
    [SerializeField] private float powerScale = 1f;
    //技能伤害类型
    [SerializeField] private SkillPowerType powerType = SkillPowerType.Attack;
    //投射物的生命周期（秒）
    [SerializeField] private float lifeSeconds = 5f;

    /**
     * 执行技能效果
     * @param context 技能上下文，包含技能施放者、目标点等信息
     */
    public override void Execute(SkillContext context)
    {
        // 检查投射物预制体是否已配置，若未配置则直接返回
        if (projectilePrefab == null) return;

        // 确定投射物的起始位置：若存在施法者则使用其位置，否则使用技能目标点
        var origin = context.Caster != null ? context.Caster.transform.position : context.Point;
        // 确定投射物的移动方向：若方向向量有效则归一化，否则使用默认前方向量
        var direction = context.Direction.sqrMagnitude > 0f ? context.Direction.normalized : Vector3.forward;
        // 根据方向计算投射物的旋转角度
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        // 实例化投射物预制体
        var projectile = Instantiate(projectilePrefab, origin, rotation);

        // 发射投射物，传入所有必要的参数
        projectile.Launch(context, direction, speed, baseDamage, powerScale, powerType, lifeSeconds);
    }
}
