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
    /// <summary>
    /// 子弹上一段移动轨迹的起点
    /// </summary>
    private Vector3 previousPosition;
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
        previousPosition = transform.position;
        launched = true;

        if (lifeSeconds > 0f)
        {
            Destroy(gameObject, lifeSeconds);
        }
    }

    private void Update()
    {
        if (!launched) return;
        previousPosition = transform.position;
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
            Vector3 hitPoint = GetHitPoint(other);

            if (damageable.TakeDamage(result.Amount, context))
            {
                context.ReportHit(target, hitPoint);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 根据子弹上一段移动轨迹取得碰撞体表面的命中点
    /// </summary>
    private Vector3 GetHitPoint(Collider targetCollider)
    {
        Vector3 movement = transform.position - previousPosition;
        if (movement.sqrMagnitude > Mathf.Epsilon)
        {
            Ray ray = new Ray(previousPosition, movement.normalized);
            if (targetCollider.Raycast(ray, out RaycastHit hit, movement.magnitude))
            {
                return hit.point;
            }
        }

        return targetCollider.ClosestPoint(transform.position);
    }
}
