using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Spread Modifier")]
    public sealed class SpreadModifierSpellDefinition : ModifierSpellDefinition
    {
        [SerializeField] private float addedSpreadAngle = 10f;

        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.SpreadAngle = Mathf.Max(0f, config.SpreadAngle + addedSpreadAngle);
        }
    }
}
