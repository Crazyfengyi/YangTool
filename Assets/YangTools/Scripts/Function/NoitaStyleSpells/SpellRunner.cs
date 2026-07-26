using System.Collections.Generic;

namespace NoitaStyleSpells
{
    public static class SpellRunner
    {
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
