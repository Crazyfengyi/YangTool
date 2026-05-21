using UnityEngine;

/// <summary>
/// 技能子弹
/// </summary>
[RequireComponent(typeof(Collider))]
public class SkillProjectile : MonoBehaviour
{
    private SkillContext context;
    private Vector3 direction;
    private float speed;
    private float baseDamage;
    private float powerScale;
    private SkillPowerType powerType;
    private bool launched;

    public void Launch(
        SkillContext context,
        Vector3 direction,
        float speed,
        float baseDamage,
        float powerScale,
        SkillPowerType powerType,
        float lifeSeconds)
    {
        this.context = context;
        this.direction = direction.normalized;
        this.speed = speed;
        this.baseDamage = baseDamage;
        this.powerScale = powerScale;
        this.powerType = powerType;
        launched = true;

        if (lifeSeconds > 0f)
        {
            Destroy(gameObject, lifeSeconds);
        }
    }

    private void Update()
    {
        if (!launched) return;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (context != null && context.Caster != null && other.gameObject == context.Caster.gameObject)
        {
            return;
        }

        var damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            var damageableComponent = damageable as Component;
            var target = damageableComponent != null ? damageableComponent.gameObject : other.gameObject;
            var result = SkillDamageTool.Calculate(
                context,
                target,
                baseDamage,
                powerScale,
                powerType);

            damageable.TakeDamage(result.Amount, context);
            Destroy(gameObject);
        }
    }
}
