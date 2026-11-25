using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 按预制体分别维护独立法杖系统的弹体对象池
    /// </summary>
    public sealed class BulletFactory : MonoBehaviour
    {
        // 按弹体预制体维护的可复用队列
        private readonly Dictionary<Bullet, Queue<Bullet>> pools = new Dictionary<Bullet, Queue<Bullet>>();

        // 停用弹体的收纳节点
        [SerializeField] private Transform poolRoot;
        // 每个指定预制体的预热数量
        [SerializeField] private int prewarmCount = 0;
        // 启动时需要预热的弹体预制体列表
        [SerializeField] private Bullet[] prewarmPrefabs;

        /// <summary>
        /// 准备对象池根节点并预热指定的弹体预制体
        /// </summary>
        private void Awake()
        {
            if (poolRoot == null)
            {
                GameObject root = new GameObject("Bullet Pool");
                root.transform.SetParent(transform);
                poolRoot = root.transform;
            }

            Prewarm();
        }

        /// <summary>
        /// 按数量与水平散射角生成弹体并为每枚弹体复制运行时配置
        /// </summary>
        /// <param name="config">本次弹体使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        public void Spawn(ProjectileRuntimeConfig config, CastContext context)
        {
            if (config == null || config.Prefab == null || context == null)
            {
                return;
            }

            int count = Mathf.Max(1, config.ProjectileCount);
            for (int i = 0; i < count; i++)
            {
                Vector3 direction = GetSpreadDirection(context.Direction, config.SpreadAngle, count, i);
                ProjectileRuntimeConfig singleConfig = config.CloneForSingleProjectile(direction);
                Bullet bullet = Get(singleConfig.Prefab);
                bullet.Initialize(singleConfig, context, this);
            }
        }

        /// <summary>
        /// 停用弹体并归还所属预制体队列 无预制体键时销毁
        /// </summary>
        /// <param name="bullet">待回收的弹体实例</param>
        public void Despawn(Bullet bullet)
        {
            if (bullet == null)
            {
                return;
            }

            Bullet prefab = bullet.PoolPrefab;
            bullet.gameObject.SetActive(false);
            bullet.transform.SetParent(poolRoot);

            if (prefab == null)
            {
                Destroy(bullet.gameObject);
                return;
            }

            if (!pools.TryGetValue(prefab, out Queue<Bullet> pool))
            {
                pool = new Queue<Bullet>();
                pools.Add(prefab, pool);
            }

            pool.Enqueue(bullet);
        }

        /// <summary>
        /// 从指定预制体队列取得弹体 或创建新实例
        /// </summary>
        /// <param name="prefab">弹体预制体与对象池索引</param>
        private Bullet Get(Bullet prefab)
        {
            if (!pools.TryGetValue(prefab, out Queue<Bullet> pool))
            {
                pool = new Queue<Bullet>();
                pools.Add(prefab, pool);
            }

            while (pool.Count > 0)
            {
                Bullet pooledBullet = pool.Dequeue();
                if (pooledBullet != null)
                {
                    pooledBullet.gameObject.SetActive(true);
                    pooledBullet.transform.SetParent(null);
                    return pooledBullet;
                }
            }

            Bullet bullet = Instantiate(prefab);
            bullet.SetPoolPrefab(prefab);
            return bullet;
        }

        /// <summary>
        /// 为指定预制体预创建配置数量的停用弹体
        /// </summary>
        private void Prewarm()
        {
            if (prewarmPrefabs == null || prewarmCount <= 0)
            {
                return;
            }

            for (int i = 0; i < prewarmPrefabs.Length; i++)
            {
                Bullet prefab = prewarmPrefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                for (int j = 0; j < prewarmCount; j++)
                {
                    Bullet bullet = Instantiate(prefab, poolRoot);
                    bullet.SetPoolPrefab(prefab);
                    bullet.gameObject.SetActive(false);

                    if (!pools.TryGetValue(prefab, out Queue<Bullet> pool))
                    {
                        pool = new Queue<Bullet>();
                        pools.Add(prefab, pool);
                    }

                    pool.Enqueue(bullet);
                }
            }
        }

        /// <summary>
        /// 在水平面按索引均匀计算散射方向
        /// </summary>
        /// <param name="baseDirection">散射中心的基准方向</param>
        /// <param name="spreadAngle">全部弹体覆盖的总角度</param>
        /// <param name="count">同次发射的弹体总数</param>
        /// <param name="index">从零开始的元素索引</param>
        private static Vector3 GetSpreadDirection(Vector3 baseDirection, float spreadAngle, int count, int index)
        {
            Vector3 direction = baseDirection.sqrMagnitude > 0f ? baseDirection.normalized : Vector3.forward;
            if (count <= 1 || spreadAngle <= 0f)
            {
                return direction;
            }

            float step = count > 1 ? spreadAngle / (count - 1) : 0f;
            float angle = -spreadAngle * 0.5f + step * index;
            return Quaternion.AngleAxis(angle, Vector3.up) * direction;
        }
    }
}
