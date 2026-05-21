using System;
using UnityEngine;

/**
 * SimpleHealth类 - 实现IDamageable接口，用于管理游戏对象的健康状态
 * 包含最大生命值、当前生命值和死亡后是否销毁等属性
 * 提供受伤和治疗的功能，并通过事件通知生命值变化和死亡状态
 */
public class SimpleHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f; // 最大生命值，默认为100
    [SerializeField] private float currentHealth = 100f; // 当前生命值，默认为100
    [SerializeField] private bool destroyOnDeath = true; // 死亡后是否销毁游戏对象，默认为true

    //生命值变化事件，参数为当前生命值和最大生命值
    public event Action<float, float> OnHealthChanged;
    //死亡事件
    public event Action OnDied;

    //当前生命值的属性，只读
    public float Current => currentHealth;
    //最大生命值的属性，只读
    public float Max => maxHealth;

    /**
     * 受到伤害
     */
    public void TakeDamage(float amount, SkillContext context)
    {
        // 如果伤害值小于等于0或对象已经死亡，则直接返回
        if (amount <= 0f || currentHealth <= 0f) return;

        // 减少生命值，确保不低于0
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        // 触发生命值变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 如果生命值降至0或更低，触发死亡事件
        if (currentHealth <= 0f)
        {
            OnDied?.Invoke();
            // 如果设置了死亡后销毁，则销毁游戏对象
            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }

    /**
     * 治疗方法
     */
    public void Heal(float amount)
    {
        // 如果治疗量小于等于0或对象已经死亡，则直接返回
        if (amount <= 0f || currentHealth <= 0f) return;

        // 增加生命值，确保不超过最大生命值
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        // 触发生命值变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
