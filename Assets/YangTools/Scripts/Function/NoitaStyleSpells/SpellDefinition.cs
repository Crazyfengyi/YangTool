using UnityEngine;

namespace NoitaStyleSpells
{
    public abstract class SpellDefinition : ScriptableObject
    {
        public abstract SpellNode Apply(CastContext context, SpellNode node);
    }
}
