using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    public sealed class WandCaster : MonoBehaviour
    {
        [SerializeField] private BulletFactory bulletFactory;
        [SerializeField] private Transform firePoint;
        [SerializeField] private List<SpellDefinition> spells = new List<SpellDefinition>();

        public List<SpellDefinition> Spells => spells;

        public void Cast()
        {
            if (bulletFactory == null)
            {
                return;
            }

            Transform originTransform = firePoint != null ? firePoint : transform;
            CastContext context = new CastContext(
                transform,
                originTransform.position,
                originTransform.forward,
                bulletFactory);

            SpellRunner.Run(spells, context);
        }
    }
}
