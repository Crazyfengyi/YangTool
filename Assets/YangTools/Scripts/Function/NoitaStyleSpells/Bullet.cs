using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 以刚体驱动独立法杖弹体并调度生命周期法术与对象池回收
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class Bullet : MonoBehaviour
    {
        // 弹体用于设置速度的刚体
        private Rigidbody body;
        // 弹体碰撞或触发检测组件
        private Collider bulletCollider;
        // 当前弹体独立的运行时配置
        private ProjectileRuntimeConfig config;
        // 创建当前弹体的施法上下文
        private CastContext sourceContext;
        // 负责生成与回收当前弹体的工厂
        private BulletFactory factory;
        // 当前弹体存活的游戏时间秒数
        private float age;
        // 周期法术的累计计时
        private float tickTimer;
        // 防止生命周期重入导致重复回收
        private bool isDespawning;

        public Bullet PoolPrefab { get; private set; }
        public float Damage => config?.Damage ?? 0f;

        /// <summary>
        /// 缓存弹体的刚体和碰撞体
        /// </summary>
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            bulletCollider = GetComponent<Collider>();
        }

        /// <summary>
        /// 推进弹体年龄与周期法术并在寿命结束后回收
        /// </summary>
        private void Update()
        {
            if (config == null || isDespawning)
            {
                return;
            }

            age += Time.deltaTime;
            RunTickLifecycle();

            if (age >= config.Lifetime)
            {
                Despawn(true);
            }
        }

        /// <summary>
        /// 在碰撞层有效时执行命中法术并结束弹体生命周期
        /// </summary>
        /// <param name="collision">物理碰撞信息</param>
        private void OnCollisionEnter(Collision collision)
        {
            if (config == null || isDespawning || !IsLayerInMask(collision.gameObject.layer, config.CollisionMask))
            {
                return;
            }

            RunLifecycle(config.OnHit, GetCurrentDirection());
            Despawn(true);
        }

        /// <summary>
        /// 在触发层有效时执行命中法术并结束弹体生命周期
        /// </summary>
        /// <param name="other">进入触发器的碰撞体</param>
        private void OnTriggerEnter(Collider other)
        {
            if (config == null || isDespawning || !IsLayerInMask(other.gameObject.layer, config.CollisionMask))
            {
                return;
            }

            RunLifecycle(config.OnHit, GetCurrentDirection());
            Despawn(true);
        }

        /// <summary>
        /// 记录弹体所属的对象池预制体键
        /// </summary>
        /// <param name="prefab">弹体预制体与对象池索引</param>
        public void SetPoolPrefab(Bullet prefab)
        {
            PoolPrefab = prefab;
        }

        /// <summary>
        /// 重置弹体状态 设置刚体速度并执行出生法术
        /// </summary>
        /// <param name="runtimeConfig">当前弹体独立使用的运行时配置</param>
        /// <param name="context">本次效果使用的施法上下文</param>
        /// <param name="bulletFactory">负责弹体生成与回收的工厂</param>
        public void Initialize(ProjectileRuntimeConfig runtimeConfig, CastContext context, BulletFactory bulletFactory)
        {
            config = runtimeConfig;
            sourceContext = context;
            factory = bulletFactory;
            age = 0f;
            tickTimer = 0f;
            isDespawning = false;

            transform.position = context.Origin;
            transform.rotation = Quaternion.LookRotation(runtimeConfig.Direction.sqrMagnitude > 0f ? runtimeConfig.Direction : context.Direction);

            if (bulletCollider != null)
            {
                bulletCollider.enabled = true;
            }

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.linearVelocity = GetCurrentDirection() * runtimeConfig.Speed;
            }

            RunLifecycle(config.OnSpawn, GetCurrentDirection());
        }

        /// <summary>
        /// 按配置间隔触发周期法术 非正间隔时跳过
        /// </summary>
        private void RunTickLifecycle()
        {
            if (config.TickInterval <= 0f || config.OnTick.Count == 0)
            {
                return;
            }

            tickTimer += Time.deltaTime;
            if (tickTimer < config.TickInterval)
            {
                return;
            }

            tickTimer = 0f;
            RunLifecycle(config.OnTick, GetCurrentDirection());
        }

        /// <summary>
        /// 以当前弹体位置与方向创建子上下文执行生命周期法术
        /// </summary>
        /// <param name="spells">按执行顺序排列的法术列表</param>
        /// <param name="direction">世界空间方向</param>
        private void RunLifecycle(System.Collections.Generic.IReadOnlyList<SpellDefinition> spells, Vector3 direction)
        {
            if (spells == null || spells.Count == 0 || sourceContext == null)
            {
                return;
            }

            CastContext childContext = sourceContext.CreateChildContext(transform.position, direction);
            SpellRunner.Run(spells, childContext);
        }

        /// <summary>
        /// 防止重复回收并按参数执行死亡法术后交回工厂
        /// </summary>
        /// <param name="runDeathLifecycle">是否在回收前执行死亡法术</param>
        private void Despawn(bool runDeathLifecycle)
        {
            if (isDespawning)
            {
                return;
            }

            isDespawning = true;

            if (runDeathLifecycle)
            {
                RunLifecycle(config?.OnDeath, GetCurrentDirection());
            }

            BulletFactory currentFactory = factory;
            ResetState();

            if (currentFactory != null)
            {
                currentFactory.Despawn(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 清除运行时配置 计时器与刚体速度
        /// </summary>
        private void ResetState()
        {
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            config = null;
            sourceContext = null;
            factory = null;
            age = 0f;
            tickTimer = 0f;
        }

        /// <summary>
        /// 优先使用刚体速度方向 其次使用配置方向与物体前方
        /// </summary>
        private Vector3 GetCurrentDirection()
        {
            if (body != null && body.linearVelocity.sqrMagnitude > 0.001f)
            {
                return body.linearVelocity.normalized;
            }

            if (config != null && config.Direction.sqrMagnitude > 0f)
            {
                return config.Direction.normalized;
            }

            return transform.forward;
        }

        /// <summary>
        /// 判断物体层是否包含于指定层掩码
        /// </summary>
        /// <param name="layer">待检查的物体层索引</param>
        /// <param name="mask">允许的层掩码</param>
        /// <returns>层包含于掩码时返回 true</returns>
        private static bool IsLayerInMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
