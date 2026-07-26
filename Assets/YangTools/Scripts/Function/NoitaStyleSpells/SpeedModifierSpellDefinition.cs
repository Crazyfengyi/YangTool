using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Speed Modifier")]
    public sealed class SpeedModifierSpellDefinition : ModifierSpellDefinition
    {
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float bonusSpeed = 0f;

        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.Speed = Mathf.Max(0f, config.Speed * speedMultiplier + bonusSpeed);
        }
    }
}
