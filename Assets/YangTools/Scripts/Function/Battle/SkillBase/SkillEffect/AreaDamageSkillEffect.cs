using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 区域伤害技能效果类
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Effects/Area Damage")]
public class AreaDamageSkillEffect : SkillEffect
{
    [SerializeField] private float radius = 3f; //技能影响半径
    [SerializeField] private LayerMask targetMask = ~0; //目标层级掩码，~0表示所有层级
    [SerializeField] private float baseDamage = 10f; //基础伤害值
    [SerializeField] private float powerScale = 1f; //伤害系数
    [SerializeField] private SkillPowerType powerType = SkillPowerType.MagicPower; //技能伤害类型
    [SerializeField] private bool centerOnTarget = true; // 是否以目标为中心

    /// <summary>
    /// 执行技能效果的方法
    /// </summary>
    public override void Execute(SkillContext context)
    {
        //确定伤害中心点：如果有目标且centerOnTarget为true，则以目标为中心，否则使用指定点
        var center = centerOnTarget && context.Target != null
            ? context.Target.transform.position
            : context.Point;

        //检测范围内的所有碰撞体
        var hits = Physics.OverlapSphere(center, radius, targetMask);
        var damagedTargets = new HashSet<GameObject>(); // 用于记录已造成伤害的目标，避免重复伤害

        //遍历所有检测到的碰撞体
        for (int i = 0; i < hits.Length; i++)
        {
            //获取碰撞体上的可伤害组件
            var damageable = hits[i].GetComponentInParent<IDamageable>();
            if (damageable == null) continue; //如果没有可伤害组件，跳过

            //获取目标游戏对象
            var damageableComponent = damageable as Component;
            var target = damageableComponent != null ? damageableComponent.gameObject : hits[i].gameObject;
            if (!damagedTargets.Add(target)) continue; //如果已经造成过伤害，跳过

            //计算伤害值
            SkillDamageResult result = SkillDamageTool.Calculate(
                context,
                target,
                baseDamage,
                powerScale,
                powerType);

            Debug.LogError($"对目标{target.name}造成伤害：{result.Amount}");
            //对目标造成伤害
            damageable.TakeDamage(result.Amount, context);
        }
    }
}
