using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    [System.Serializable]
    public sealed class BulletLifecycleSpellLists
    {
        [SerializeField] private List<SpellDefinition> onSpawn = new List<SpellDefinition>();
        [SerializeField] private List<SpellDefinition> onTick = new List<SpellDefinition>();
        [SerializeField] private List<SpellDefinition> onHit = new List<SpellDefinition>();
        [SerializeField] private List<SpellDefinition> onDeath = new List<SpellDefinition>();

        public IReadOnlyList<SpellDefinition> OnSpawn => onSpawn;
        public IReadOnlyList<SpellDefinition> OnTick => onTick;
        public IReadOnlyList<SpellDefinition> OnHit => onHit;
        public IReadOnlyList<SpellDefinition> OnDeath => onDeath;
    }
}
