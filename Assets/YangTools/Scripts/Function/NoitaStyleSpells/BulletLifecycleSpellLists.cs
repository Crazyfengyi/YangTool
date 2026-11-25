using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 分别保存出生 周期 命中与死亡时执行的法术列表
    /// </summary>
    [System.Serializable]
    public sealed class BulletLifecycleSpellLists
    {
        // 弹体出生时执行的法术列表
        [SerializeField] private List<SpellDefinition> onSpawn = new List<SpellDefinition>();
        // 弹体周期触发时执行的法术列表
        [SerializeField] private List<SpellDefinition> onTick = new List<SpellDefinition>();
        // 弹体命中时执行的法术列表
        [SerializeField] private List<SpellDefinition> onHit = new List<SpellDefinition>();
        // 弹体结束时执行的法术列表
        [SerializeField] private List<SpellDefinition> onDeath = new List<SpellDefinition>();

        public IReadOnlyList<SpellDefinition> OnSpawn => onSpawn;
        public IReadOnlyList<SpellDefinition> OnTick => onTick;
        public IReadOnlyList<SpellDefinition> OnHit => onHit;
        public IReadOnlyList<SpellDefinition> OnDeath => onDeath;
    }
}
