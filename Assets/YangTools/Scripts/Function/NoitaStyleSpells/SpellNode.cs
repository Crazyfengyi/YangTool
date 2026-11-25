namespace NoitaStyleSpells
{
    /// <summary>
    /// 保存一个法术定义及其后继节点
    /// </summary>
    public sealed class SpellNode
    {
        /// <summary>
        /// 使用指定法术定义创建链表节点
        /// </summary>
        /// <param name="spell">节点持有的法术定义</param>
        public SpellNode(SpellDefinition spell)
        {
            Spell = spell;
        }

        public SpellDefinition Spell { get; }
        public SpellNode Next { get; set; }
    }
}
