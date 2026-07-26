using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Count Modifier")]
    public sealed class CountModifierSpellDefinition : ModifierSpellDefinition
    {
        [SerializeField] private int addedProjectileCount = 1;

        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.ProjectileCount = Mathf.Max(1, config.ProjectileCount + addedProjectileCount);
        }
    }
}
