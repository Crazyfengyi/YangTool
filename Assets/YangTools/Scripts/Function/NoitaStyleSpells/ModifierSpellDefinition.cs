using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 将自身加入施法上下文的待处理修饰器列表
    /// </summary>
    public abstract class ModifierSpellDefinition : SpellDefinition, IProjectileModifier
    {
        /// <summary>
        /// 缓存当前修饰器并继续处理下一个法术节点
        /// </summary>
        /// <param name="context">本次效果使用的施法上下文</param>
        /// <param name="node">当前执行的法术链表节点</param>
        public override SpellNode Apply(CastContext context, SpellNode node)
        {
            context?.AddModifier(this);
            return node.Next;
        }

        /// <summary>
        /// 由具体修饰法术实现运行时弹体参数变更
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public abstract void Modify(ProjectileRuntimeConfig config, CastContext context);
    }
}
