using UnityEngine;

namespace NoitaStyleSpells
{
    [CreateAssetMenu(menuName = "Noita Style Spells/Bullet Config")]
    public sealed class BulletConfig : ScriptableObject
    {
        [SerializeField] private Bullet prefab;
        [SerializeField] private float speed = 20f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private float spreadAngle = 0f;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private float tickInterval = 0f;
        [SerializeField] private BulletLifecycleSpellLists lifecycleSpells = new BulletLifecycleSpellLists();

        public Bullet Prefab => prefab;
        public float Speed => speed;
        public float Damage => damage;
        public float Lifetime => lifetime;
        public int ProjectileCount => projectileCount;
        public float SpreadAngle => spreadAngle;
        public LayerMask CollisionMask => collisionMask;
        public float TickInterval => tickInterval;
        public BulletLifecycleSpellLists LifecycleSpells => lifecycleSpells;
    }
}
