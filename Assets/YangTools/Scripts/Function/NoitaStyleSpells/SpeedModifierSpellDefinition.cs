using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 定义速度倍率与固定速度增量的法杖修饰法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Speed Modifier")]
    public sealed class SpeedModifierSpellDefinition : ModifierSpellDefinition
    {
        // 速度倍率
        [SerializeField] private float speedMultiplier = 1f;
        // 倍率计算后叠加的固定速度
        [SerializeField] private float bonusSpeed = 0f;

        /// <summary>
        /// 应用速度倍率与增量并将最终速度限制为非负
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            config.Speed = Mathf.Max(0f, config.Speed * speedMultiplier + bonusSpeed);
        }
    }
}
