using UnityEngine;

namespace NoitaStyleSpells
{
    /// <summary>
    /// 保存独立法杖系统的基础弹体参数和生命周期法术
    /// </summary>
    [CreateAssetMenu(menuName = "Noita Style Spells/Bullet Config")]
    public sealed class BulletConfig : ScriptableObject
    {
        // 弹体使用的预制体
        [SerializeField] private Bullet prefab;
        // 弹体飞行速度
        [SerializeField] private float speed = 20f;
        // 弹体携带的伤害参数
        [SerializeField] private float damage = 1f;
        // 弹体最大存活秒数
        [SerializeField] private float lifetime = 3f;
        // 一次生成的弹体数量
        [SerializeField] private int projectileCount = 1;
        // 水平散射总角度
        [SerializeField] private float spreadAngle = 0f;
        // 会触发生命周期命中的层掩码
        [SerializeField] private LayerMask collisionMask = ~0;
        // 周期法术间隔秒数 非正值不执行周期法术
        [SerializeField] private float tickInterval = 0f;
        // 四类弹体生命周期法术配置
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
