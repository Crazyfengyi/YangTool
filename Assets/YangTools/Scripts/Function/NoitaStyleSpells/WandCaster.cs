using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 在配置发射点按顺序执行法杖法术列表
    /// </summary>
    public sealed class WandCaster : MonoBehaviour
    {
        // 负责法杖弹体生成与复用的工厂
        [SerializeField] private BulletFactory bulletFactory;
        // 法杖的可选发射位置与方向
        [SerializeField] private Transform firePoint;
        // 按顺序执行的法术定义列表
        [SerializeField] private List<SpellDefinition> spells = new List<SpellDefinition>();

        public List<SpellDefinition> Spells => spells;

        /// <summary>
        /// 使用发射点或角色位置创建上下文并运行法术列表
        /// </summary>
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
