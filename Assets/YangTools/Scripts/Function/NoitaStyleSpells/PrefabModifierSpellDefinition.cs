using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 定义替换弹体预制体的法杖修饰法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Modifiers/Prefab Modifier")]
    public sealed class PrefabModifierSpellDefinition : ModifierSpellDefinition
    {
        // 弹体使用的预制体
        [SerializeField] private Bullet prefab;

        /// <summary>
        /// 使用有效的覆盖预制体替换本次弹体模板
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public override void Modify(ProjectileRuntimeConfig config, CastContext context)
        {
            if (prefab != null)
            {
                config.Prefab = prefab;
            }
        }
    }
}
