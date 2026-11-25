using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 定义可按链表顺序执行的法杖法术资产基类
    /// </summary>
    public abstract class SpellDefinition : ScriptableObject
    {
        /// <summary>
        /// 执行当前法术并返回后续节点 具体行为由派生法术实现
        /// </summary>
        /// <param name="context">本次效果使用的施法上下文</param>
        /// <param name="node">当前执行的法术链表节点</param>
        public abstract SpellNode Apply(CastContext context, SpellNode node);
    }
}
