using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Prefab Modifier")]
    public sealed class PrefabModifierSpellDefinition : ModifierSpellDefinition
    {
        [SerializeField] private Bullet prefab;

        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            if (prefab != null)
            {
                config.Prefab = prefab;
            }
        }
    }
}
