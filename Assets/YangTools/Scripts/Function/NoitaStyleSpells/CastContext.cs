using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    public sealed class CastContext
    {
        private readonly List<IProjectileModifier> pendingModifiers = new List<IProjectileModifier>();

        public CastContext(
            Transform caster,
            Vector3 origin,
            Vector3 direction,
            BulletFactory bulletFactory,
            System.Random random = null)
        {
            Caster = caster;
            Origin = origin;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            BulletFactory = bulletFactory;
            Random = random ?? new System.Random();
        }

        public Transform Caster { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public BulletFactory BulletFactory { get; }
        public System.Random Random { get; }
        public IReadOnlyList<IProjectileModifier> PendingModifiers => pendingModifiers;

        public void AddModifier(IProjectileModifier modifier)
        {
            if (modifier != null)
            {
                pendingModifiers.Add(modifier);
            }
        }

        public void ApplyAndConsumeModifiers(ProjectileRuntimeConfig config)
        {
            if (config == null)
            {
                ClearPendingModifiers();
                return;
            }

            for (int i = 0; i < pendingModifiers.Count; i++)
            {
                pendingModifiers[i].Modify(config, this);
            }

            pendingModifiers.Clear();
        }

        public void ClearPendingModifiers()
        {
            pendingModifiers.Clear();
        }

        public CastContext CreateChildContext(Vector3 origin, Vector3 direction)
        {
            return new CastContext(Caster, origin, direction, BulletFactory, Random);
        }
    }
}
