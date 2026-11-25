using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 近战打击技能效果类
/// </summary>

[CreateAssetMenu(menuName = "Game/Skills/Effects/Melee Hit")]
public class MeleeHitSkillEffect : SkillEffect
{
    /// <summary>
    /// 近战打击形状枚举 定义了近战攻击的范围形状 球形或长方体
    /// </summary>

    public enum MeleeHitShape
    {
        [LabelText("球形")]
        Sphere,  // 球形范围
        [LabelText("长方体")]
        Box      // 长方体范围
    }

    [LabelText("判定形状")]
    [SerializeField] private MeleeHitShape shape = MeleeHitShape.Sphere;  // 近战打击形状，默认为球形
    [LabelText("攻击距离")]
    [SerializeField] private float range = 2f;  // 近战攻击的射程
    [LabelText("球形半径")]
    [SerializeField] private float radius = 1f;  // 球形打击范围的半径
    [LabelText("长方体尺寸")]
    [SerializeField] private Vector3 boxSize = new Vector3(1f, 1f, 2f);  // 长方体打击范围的尺寸
    [LabelText("目标层级")]
    [SerializeField] private LayerMask targetMask = ~0;  // 可攻击目标的层级掩码，默认为所有层级
    [LabelText("基础伤害")]
    [SerializeField] private float baseDamage = 10f;  // 基础伤害值
    [LabelText("属性加成系数")]
    [PropertyTooltip("基础伤害加上所选属性乘以此系数后再参与后续伤害计算")]
    [SerializeField] private float powerScale = 1f;  // 伤害系数
    [LabelText("加成属性")]
    [SerializeField] private SkillPowerType powerType = SkillPowerType.Attack;  // 技能伤害类型

    /// <summary>
    /// 执行技能效果
    /// </summary>
    /// <param name="context">技能上下文 包含技能执行所需的各种信息</param>

    public override void Execute(SkillContext context)
    {
        if (context == null)  // 检查上下文是否有效
        {
            return;
        }

        // 计算打击中心点
        Vector3 direction = context.Direction.sqrMagnitude > 0f ? context.Direction.normalized : Vector3.forward;
        Vector3 center = context.OriginPoint + direction * Mathf.Max(0f, range);
        // 根据形状选择不同的碰撞检测方法
        Collider[] hits = shape == MeleeHitShape.Box
            ? Physics.OverlapBox(center, boxSize * 0.5f, Quaternion.LookRotation(direction, Vector3.up), targetMask)
            : Physics.OverlapSphere(center, radius, targetMask);

        var damagedTargets = new HashSet<GameObject>();  // 用于记录已造成伤害的目标，避免重复伤害
        for (int i = 0; i < hits.Length; i++)
        {
            // 查找可伤害组件
            IDamageable damageable = hits[i].GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                continue;
            }

            // 获取目标游戏对象
            var damageableComponent = damageable as Component;
            GameObject target = damageableComponent != null ? damageableComponent.gameObject : hits[i].gameObject;
            // 跳过施法者自身
            if (context.Caster != null && target == context.Caster.gameObject)
            {
                continue;
            }

            // 避免对同一目标重复造成伤害
            if (!damagedTargets.Add(target))
            {
                continue;
            }

            // 计算伤害并应用
            SkillDamageResult result = SkillDamageTool.Calculate(context, target, baseDamage, powerScale, powerType);
            if (damageable.TakeDamage(result.Amount, context))
            {
                context.ReportHit(target);
            }
        }
    }
}
