using UnityEngine;

namespace NoitaStyleSpells
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class Bullet : MonoBehaviour
    {
        private Rigidbody body;
        private Collider bulletCollider;
        private ProjectileRuntimeConfig config;
        private CastContext sourceContext;
        private BulletFactory factory;
        private float age;
        private float tickTimer;
        private bool isDespawning;

        public Bullet PoolPrefab { get; private set; }
        public float Damage => config?.Damage ?? 0f;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            bulletCollider = GetComponent<Collider>();
        }

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

        private void OnCollisionEnter(Collision collision)
        {
            if (config == null || isDespawning || !IsLayerInMask(collision.gameObject.layer, config.CollisionMask))
            {
                return;
            }

            RunLifecycle(config.OnHit, GetCurrentDirection());
            Despawn(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (config == null || isDespawning || !IsLayerInMask(other.gameObject.layer, config.CollisionMask))
            {
                return;
            }

            RunLifecycle(config.OnHit, GetCurrentDirection());
            Despawn(true);
        }

        public void SetPoolPrefab(Bullet prefab)
        {
            PoolPrefab = prefab;
        }

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
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.velocity = GetCurrentDirection() * runtimeConfig.Speed;
            }

            RunLifecycle(config.OnSpawn, GetCurrentDirection());
        }

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

        private void RunLifecycle(System.Collections.Generic.IReadOnlyList<SpellDefinition> spells, Vector3 direction)
        {
            if (spells == null || spells.Count == 0 || sourceContext == null)
            {
                return;
            }

            CastContext childContext = sourceContext.CreateChildContext(transform.position, direction);
            SpellRunner.Run(spells, childContext);
        }

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

        private void ResetState()
        {
            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            config = null;
            sourceContext = null;
            factory = null;
            age = 0f;
            tickTimer = 0f;
        }

        private Vector3 GetCurrentDirection()
        {
            if (body != null && body.velocity.sqrMagnitude > 0.001f)
            {
                return body.velocity.normalized;
            }

            if (config != null && config.Direction.sqrMagnitude > 0f)
            {
                return config.Direction.normalized;
            }

            return transform.forward;
        }

        private static bool IsLayerInMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
