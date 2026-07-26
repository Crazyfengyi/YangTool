using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Lifecycle Modifier")]
    public sealed class LifecycleModifierSpellDefinition : ModifierSpellDefinition
    {
        [SerializeField] private List<SpellDefinition> onSpawn = new List<SpellDefinition>();
        [SerializeField] private List<SpellDefinition> onTick = new List<SpellDefinition>();
        [SerializeField] private List<SpellDefinition> onHit = new List<SpellDefinition>();
        [SerializeField] private List<SpellDefinition> onDeath = new List<SpellDefinition>();

        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.AppendLifecycleSpells(onSpawn, onTick, onHit, onDeath);
        }
    }
}
