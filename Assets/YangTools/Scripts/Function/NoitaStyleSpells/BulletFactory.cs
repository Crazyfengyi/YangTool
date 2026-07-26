using System.Collections.Generic;
using UnityEngine;

namespace NoitaStyleSpells
{
    public sealed class BulletFactory : MonoBehaviour
    {
        private readonly Dictionary<Bullet, Queue<Bullet>> pools = new Dictionary<Bullet, Queue<Bullet>>();

        [SerializeField] private Transform poolRoot;
        [SerializeField] private int prewarmCount = 0;
        [SerializeField] private Bullet[] prewarmPrefabs;

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
