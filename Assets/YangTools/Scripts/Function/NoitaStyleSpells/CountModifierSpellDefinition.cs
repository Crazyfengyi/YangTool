using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 定义增加弹体数量的法杖修饰法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Count Modifier")]
    public sealed class CountModifierSpellDefinition : ModifierSpellDefinition
    {
        // 增加的弹体数量
        [SerializeField] private int addedProjectileCount = 1;

        /// <summary>
        /// 增加弹体数量并保证至少生成一枚
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.ProjectileCount = Mathf.Max(1, config.ProjectileCount + addedProjectileCount);
        }
    }
}
