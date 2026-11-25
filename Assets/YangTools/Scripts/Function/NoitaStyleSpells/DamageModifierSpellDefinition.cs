using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 定义伤害倍率与固定伤害增量的法杖修饰法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Damage Modifier")]
    public sealed class DamageModifierSpellDefinition : ModifierSpellDefinition
    {
        // 伤害倍率
        [SerializeField] private float damageMultiplier = 1f;
        // 倍率计算后叠加的固定伤害
        [SerializeField] private float bonusDamage = 0f;

        /// <summary>
        /// 先应用伤害倍率再叠加固定伤害值
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.Damage = config.Damage * damageMultiplier + bonusDamage;
        }
    }
}
