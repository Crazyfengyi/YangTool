using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 保存一次法杖施法的来源 方位 工厂与待消费修饰器
    /// </summary>
    public sealed class CastContext
    {
        // 等待下一次投射物法术消费的修饰器
        private readonly List<IProjectileModifier> pendingModifiers = new List<IProjectileModifier>();

        /// <summary>
        /// 创建共享施法来源与随机源的上下文
        /// </summary>
        /// <param name="caster">发起法杖施法的物体</param>
        /// <param name="origin">世界空间发射起点</param>
        /// <param name="direction">世界空间方向</param>
        /// <param name="bulletFactory">负责弹体生成与回收的工厂</param>
        /// <param name="random">共享的随机数生成器</param>
        public CastContext(
            Transform caster,
            Vector3 origin,
            Vector3 direction,
            BulletFactory bulletFactory,
            System.Random random = null)
        {
            Caster = caster;
            Origin = origin;
            Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.forward;
            BulletFactory = bulletFactory;
            Random = random ?? new System.Random();
        }

        public Transform Caster { get; }
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public BulletFactory BulletFactory { get; }
        public System.Random Random { get; }
        public IReadOnlyList<IProjectileModifier> PendingModifiers => pendingModifiers;

        /// <summary>
        /// 将有效修饰器加入下一次投射物生成使用的待处理列表
        /// </summary>
        /// <param name="modifier">应用到下一次投射物生成的修饰器</param>
        public void AddModifier(IProjectileModifier modifier)
        {
            if (modifier != null)
            {
                pendingModifiers.Add(modifier);
            }
        }

        /// <summary>
        /// 依次修改运行时弹体配置并清空待处理修饰器
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        public void ApplyAndConsumeModifiers(ProjectileRuntimeConfig config)
        {
            if (config == null)
            {
                ClearPendingModifiers();
                return;
            }

            for (int i = 0; i < pendingModifiers.Count; i++)
            {
                pendingModifiers[i].Modify(config, this);
            }

            pendingModifiers.Clear();
        }

        /// <summary>
        /// 丢弃尚未消费的投射物修饰器
        /// </summary>
        public void ClearPendingModifiers()
        {
            pendingModifiers.Clear();
        }

        /// <summary>
        /// 为子法术创建共享来源与工厂但不继承待处理修饰器的上下文
        /// </summary>
        /// <param name="origin">世界空间发射起点</param>
        /// <param name="direction">世界空间方向</param>
        public CastContext CreateChildContext(Vector3 origin, Vector3 direction)
        {
            return new CastContext(Caster, origin, direction, BulletFactory, Random);
        }
    }
}
