using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    public sealed class ProjectileRuntimeConfig
    {
        public Bullet Prefab { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Lifetime { get; set; }
        public int ProjectileCount { get; set; }
        public float SpreadAngle { get; set; }
        public LayerMask CollisionMask { get; set; }
        public float TickInterval { get; set; }
        public List<SpellDefinition> OnSpawn { get; } = new List<SpellDefinition>();
        public List<SpellDefinition> OnTick { get; } = new List<SpellDefinition>();
        public List<SpellDefinition> OnHit { get; } = new List<SpellDefinition>();
        public List<SpellDefinition> OnDeath { get; } = new List<SpellDefinition>();

        public static ProjectileRuntimeConfig FromConfig(BulletConfig config)
        {
            ProjectileRuntimeConfig runtimeConfig = new ProjectileRuntimeConfig
            {
                Prefab = config.Prefab,
                Speed = config.Speed,
                Damage = config.Damage,
                Lifetime = config.Lifetime,
                ProjectileCount = Mathf.Max(1, config.ProjectileCount),
                SpreadAngle = Mathf.Max(0f, config.SpreadAngle),
                CollisionMask = config.CollisionMask,
                TickInterval = Mathf.Max(0f, config.TickInterval)
            };

            BulletLifecycleSpellLists lifecycle = config.LifecycleSpells;
            AddRange(runtimeConfig.OnSpawn, lifecycle?.OnSpawn);
            AddRange(runtimeConfig.OnTick, lifecycle?.OnTick);
            AddRange(runtimeConfig.OnHit, lifecycle?.OnHit);
            AddRange(runtimeConfig.OnDeath, lifecycle?.OnDeath);

            return runtimeConfig;
        }

        public ProjectileRuntimeConfig CloneForSingleProjectile(Vector3 direction)
        {
            ProjectileRuntimeConfig clone = new ProjectileRuntimeConfig
            {
                Prefab = Prefab,
                Speed = Speed,
                Damage = Damage,
                Lifetime = Lifetime,
                ProjectileCount = 1,
                SpreadAngle = 0f,
                CollisionMask = CollisionMask,
                TickInterval = TickInterval,
                Direction = direction
            };

            clone.OnSpawn.AddRange(OnSpawn);
            clone.OnTick.AddRange(OnTick);
            clone.OnHit.AddRange(OnHit);
            clone.OnDeath.AddRange(OnDeath);

            return clone;
        }

        public Vector3 Direction { get; private set; }

        public void AppendLifecycleSpells(
            IReadOnlyList<SpellDefinition> onSpawn,
            IReadOnlyList<SpellDefinition> onTick,
            IReadOnlyList<SpellDefinition> onHit,
            IReadOnlyList<SpellDefinition> onDeath)
        {
            AddRange(OnSpawn, onSpawn);
            AddRange(OnTick, onTick);
            AddRange(OnHit, onHit);
            AddRange(OnDeath, onDeath);
        }

        private static void AddRange(List<SpellDefinition> target, IReadOnlyList<SpellDefinition> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    target.Add(source[i]);
                }
            }
        }
    }
}
