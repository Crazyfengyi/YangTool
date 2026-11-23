/// <summary>
/// 可接受技能伤害的对象。
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 尝试应用伤害，返回是否实际造成伤害。
    /// </summary>
    bool TakeDamage(float amount, SkillContext context);
}
