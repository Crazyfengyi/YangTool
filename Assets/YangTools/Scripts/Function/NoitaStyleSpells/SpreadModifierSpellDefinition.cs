using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 定义增加水平散射角度的法杖修饰法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Spread Modifier")]
    public sealed class SpreadModifierSpellDefinition : ModifierSpellDefinition
    {
        // 增加的水平散射角度
        [SerializeField] private float addedSpreadAngle = 10f;

        /// <summary>
        /// 增加散射角并将结果限制为非负
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.SpreadAngle = Mathf.Max(0f, config.SpreadAngle + addedSpreadAngle);
        }
    }
}
