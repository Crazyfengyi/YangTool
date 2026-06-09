namespace NoitaStyleSpells
{
    public sealed class SpellNode
    {
        public SpellNode(SpellDefinition spell)
        {
            Spell = spell;
        }

        public SpellDefinition Spell { get; }
        public SpellNode Next { get; set; }
    }
}
