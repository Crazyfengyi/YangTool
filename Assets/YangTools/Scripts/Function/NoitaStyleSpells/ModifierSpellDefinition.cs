using UnityEngine;

namespace NoitaStyleSpells
{
    public abstract class ModifierSpellDefinition : SpellDefinition, IProjectileModifier
    {
        public override SpellNode Apply(CastContext context, SpellNode node)
        {
            context?.AddModifier(this);
            return node.Next;
        }

        public abstract void Modify(ProjectileRuntimeConfig config, CastContext context);
    }
}
