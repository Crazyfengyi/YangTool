using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Damage Modifier")]
    public sealed class DamageModifierSpellDefinition : ModifierSpellDefinition
    {
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float bonusDamage = 0f;

        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.Damage = config.Damage * damageMultiplier + bonusDamage;
        }
    }
}
