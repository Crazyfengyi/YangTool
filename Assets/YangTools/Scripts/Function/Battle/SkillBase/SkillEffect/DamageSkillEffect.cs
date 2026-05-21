using UnityEngine;

/// <summary>
/// 伤害技能效果类.用于处理造成伤害的技能逻辑
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Effects/Damage")]
public class DamageSkillEffect : SkillEffect
{
    //基础伤害值
    [SerializeField] private float baseDamage = 10f;
    //伤害系数
    [SerializeField] private float powerScale = 1f;
    //伤害类型
    [SerializeField] private SkillPowerType powerType = SkillPowerType.Attack;

    /// <summary>
    /// 执行技能效果的方法
    /// </summary>
    public override void Execute(SkillContext context)
    {
        // 检查目标是否存在，不存在则直接返回
        if (context.Target == null) return;

        // 获取目标可伤害接口
        var damageable = context.Target.GetComponentInParent<IDamageable>();
        // 如果目标不可被伤害，则直接返回
        if (damageable == null) return;

        // 获取可伤害组件的引用
        var damageableComponent = damageable as Component;
        // 确定目标对象，优先使用可伤害组件所在的游戏对象
        var target = damageableComponent != null ? damageableComponent.gameObject : context.Target;

        // 计算技能伤害结果
        var result = SkillDamageTool.Calculate(
            context,
            target,
            baseDamage,
            powerScale,
            powerType);

        // 让目标受到计算出的伤害
        damageable.TakeDamage(result.Amount, context);
    }
}
