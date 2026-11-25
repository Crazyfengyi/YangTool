using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 为弹体追加出生 周期 命中和死亡法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Lifecycle Modifier")]
    public sealed class LifecycleModifierSpellDefinition : ModifierSpellDefinition
    {
        // 弹体出生时执行的法术列表
        [SerializeField] private List<SpellDefinition> onSpawn = new List<SpellDefinition>();
        // 弹体周期触发时执行的法术列表
        [SerializeField] private List<SpellDefinition> onTick = new List<SpellDefinition>();
        // 弹体命中时执行的法术列表
        [SerializeField] private List<SpellDefinition> onHit = new List<SpellDefinition>();
        // 弹体结束时执行的法术列表
        [SerializeField] private List<SpellDefinition> onDeath = new List<SpellDefinition>();

        /// <summary>
        /// 将四类生命周期法术追加到本次弹体配置
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.AppendLifecycleSpells(onSpawn, onTick, onHit, onDeath);
        }
    }
}
