using System.Collections.Generic;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 将有序法术列表连接为链表并驱动执行
    /// </summary>
    public static class SpellRunner
    {
        /// <summary>
        /// 依次执行有效法术节点并按返回节点继续推进
        /// </summary>
        /// <param name="spells">按执行顺序排列的法术列表</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public static void Run(IReadOnlyList<SpellDefinition> spells, CastContext context)
        {
            if (spells == null || spells.Count == 0 || context == null)
            {
                return;
            }

            SpellNode head = BuildLinkedList(spells);
            SpellNode current = head;

            while (current != null)
            {
                if (current.Spell == null)
                {
                    current = current.Next;
                    continue;
                }

                current = current.Spell.Apply(context, current) ?? current.Next;
            }
        }

        /// <summary>
        /// 按列表顺序连接非空法术并返回链表首节点
        /// </summary>
        /// <param name="spells">按执行顺序排列的法术列表</param>
        private static SpellNode BuildLinkedList(IReadOnlyList<SpellDefinition> spells)
        {
            SpellNode head = null;
            SpellNode previous = null;

            for (int i = 0; i < spells.Count; i++)
            {
                SpellNode node = new SpellNode(spells[i]);

                if (head == null)
                {
                    head = node;
                }

                if (previous != null)
                {
                    previous.Next = node;
                }

                previous = node;
            }

            return head;
        }
    }
}
