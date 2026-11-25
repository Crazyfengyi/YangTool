using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 保存从基础资产复制并经修饰后的单次弹体生成参数
    /// </summary>
    public sealed class ProjectileRuntimeConfig
    {
        public Bullet Prefab { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Lifetime { get; set; }
        public int ProjectileCount { get; set; }
        public float SpreadAngle { get; set; }
        public LayerMask CollisionMask { get; set; }
        public float TickInterval { get; set; }
        public List<SpellDefinition> OnSpawn { get; } = new List<SpellDefinition>();
        public List<SpellDefinition> OnTick { get; } = new List<SpellDefinition>();
        public List<SpellDefinition> OnHit { get; } = new List<SpellDefinition>();
        public List<SpellDefinition> OnDeath { get; } = new List<SpellDefinition>();

        /// <summary>
        /// 复制基础配置并限制数量 散射角和周期间隔的有效范围
        /// </summary>
        /// <param name="config">作为复制来源的基础弹体配置资产</param>
        public static ProjectileRuntimeConfig FromConfig(BulletConfig config)
        {
            ProjectileRuntimeConfig runtimeConfig = new ProjectileRuntimeConfig
            {
                Prefab = config.Prefab,
                Speed = config.Speed,
                Damage = config.Damage,
                Lifetime = config.Lifetime,
                ProjectileCount = Mathf.Max(1, config.ProjectileCount),
                SpreadAngle = Mathf.Max(0f, config.SpreadAngle),
                CollisionMask = config.CollisionMask,
                TickInterval = Mathf.Max(0f, config.TickInterval)
            };

            BulletLifecycleSpellLists lifecycle = config.LifecycleSpells;
            AddRange(runtimeConfig.OnSpawn, lifecycle?.OnSpawn);
            AddRange(runtimeConfig.OnTick, lifecycle?.OnTick);
            AddRange(runtimeConfig.OnHit, lifecycle?.OnHit);
            AddRange(runtimeConfig.OnDeath, lifecycle?.OnDeath);

            return runtimeConfig;
        }

        /// <summary>
        /// 复制单枚弹体配置并写入方向 将数量与散射还原为单发
        /// </summary>
        /// <param name="direction">世界空间方向</param>
        public ProjectileRuntimeConfig CloneForSingleProjectile(Vector3 direction)
        {
            ProjectileRuntimeConfig clone = new ProjectileRuntimeConfig
            {
                Prefab = Prefab,
                Speed = Speed,
                Damage = Damage,
                Lifetime = Lifetime,
                ProjectileCount = 1,
                SpreadAngle = 0f,
                CollisionMask = CollisionMask,
                TickInterval = TickInterval,
                Direction = direction
            };

            clone.OnSpawn.AddRange(OnSpawn);
            clone.OnTick.AddRange(OnTick);
            clone.OnHit.AddRange(OnHit);
            clone.OnDeath.AddRange(OnDeath);

            return clone;
        }

        public Vector3 Direction { get; private set; }

        /// <summary>
        /// 将四类有效生命周期法术追加到运行时列表
        /// </summary>
        /// <param name="onSpawn">出生时执行的法术列表</param>
        /// <param name="onTick">周期触发时执行的法术列表</param>
        /// <param name="onHit">命中时执行的法术列表</param>
        /// <param name="onDeath">结束时执行的法术列表</param>
        public void AppendLifecycleSpells(
            IReadOnlyList<SpellDefinition> onSpawn,
            IReadOnlyList<SpellDefinition> onTick,
            IReadOnlyList<SpellDefinition> onHit,
            IReadOnlyList<SpellDefinition> onDeath)
        {
            AddRange(OnSpawn, onSpawn);
            AddRange(OnTick, onTick);
            AddRange(OnHit, onHit);
            AddRange(OnDeath, onDeath);
        }

        /// <summary>
        /// 将源列表中的非空法术按原顺序加入目标列表
        /// </summary>
        /// <param name="target">接收追加法术的目标列表</param>
        /// <param name="source">待复制的源配置或列表</param>
        private static void AddRange(List<SpellDefinition> target, IReadOnlyList<SpellDefinition> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    target.Add(source[i]);
                }
            }
        }
    }
}
