using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Projectile Spell")]
    public sealed class ProjectileSpellDefinition : SpellDefinition
    {
        [SerializeField] private BulletConfig bulletConfig;

        public override SpellNode Apply(CastContext context, SpellNode node)
        {
            if (context?.BulletFactory == null || bulletConfig == null)
            {
                context?.ClearPendingModifiers();
                return node.Next;
            }

            ProjectileRuntimeConfig runtimeConfig = ProjectileRuntimeConfig.FromConfig(bulletConfig);
            context.ApplyAndConsumeModifiers(runtimeConfig);
            context.BulletFactory.Spawn(runtimeConfig, context);
            return node.Next;
        }
    }
}
