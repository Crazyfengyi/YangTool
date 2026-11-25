using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 消费待处理修饰器并请求工厂生成配置的弹体
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Projectile Spell")]
    public sealed class ProjectileSpellDefinition : SpellDefinition
    {
        // 用于创建运行时配置的基础弹体资产
        [SerializeField] private BulletConfig bulletConfig;

        /// <summary>
        /// 复制基础弹体参数并应用修饰器后生成 缺少配置时清空修饰器
        /// </summary>
        /// <param name="context">本次效果使用的施法上下文</param>
        /// <param name="node">当前执行的法术链表节点</param>
        public override SpellNode Apply(CastContext context, SpellNode node)
        {
            if (context?.BulletFactory == null || bulletConfig == null)
            {
                context?.ClearPendingModifiers();
                return node.Next;
            }

            ProjectileRuntimeConfig runtimeConfig = ProjectileRuntimeConfig.FromConfig(bulletConfig);
            context.ApplyAndConsumeModifiers(runtimeConfig);
            context.BulletFactory.Spawn(runtimeConfig, context);
            return node.Next;
        }
    }
}
