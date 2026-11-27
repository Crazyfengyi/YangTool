using DataStruct;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 将 ThroughWorld 角色属性与受击入口适配到 Battle 技能系统
/// </summary>
public sealed class ThroughWorldBattleAdapter : MonoBehaviour, ISkillStatProvider, IDamageable
{
    [SerializeField, LabelText("角色")] private RoleBase role; // 显式指定的角色对象
    [SerializeField, LabelText("Battle施法器")] private SkillCaster caster; // 统一施法会话入口

    /// <summary>
    /// 按角色属性读取 Battle 公式支持的攻击和防御
    /// </summary>
    public float GetSkillStat(StatType type)
    {
        if (role == null) return 0;
        CharacterStats battleStats = GetComponent<CharacterStats>();
        if (type == StatType.Attack || type == StatType.Defense)
        {
            float value = role.roleAttributeControl != null && role.roleAttributeControl.attributesDic != null
                ? role.GetRoleAttributeValue(type == StatType.Attack ? RoleAttribute.Atk : RoleAttribute.Def) : 0;
            if (battleStats == null) return value;
            battleStats.SetBaseValue(type, value);
            return battleStats.GetValue(type);
        }
        return battleStats != null ? battleStats.GetValue(type) : 0;
    }

    /// <summary>
    /// 接收 Battle 已完成防御和暴击计算的伤害 不再次调用旧减防
    /// </summary>
    public bool TakeDamage(float amount, SkillContext context)
    {
        if (role == null || !role.IsCanBeHit() || amount <= 0) return false;
        var info = new DamageInfo
        {
            damage = amount,
            atkPos = context != null ? context.OriginPoint : transform.position,
            behitPos = role.FirstCollider != null ? role.ClosestColliderPos(context != null ? context.OriginPoint : transform.position) : role.transform.position
        };
        role.BeHit(ref info);
        role.ShowBeHitEffect(info.beHitEffectInfo);
        return true;
    }

    /// <summary>
    /// 根据技能表与映射提交一次独立 Battle 施法会话
    /// </summary>
    public bool TryCast(ThroughWorldBattleSkill mapping, GameObject target, out SkillCastSession session)
    {
        session = null;
        if (role == null || caster == null || mapping == null || mapping.skill == null || mapping.timeline == null ||
            GameTableManager.Instance == null || GameTableManager.Instance.Tables == null)
        {
            Debug.LogError("ThroughWorld Battle 技能映射或技能表未初始化", this);
            return false;
        }
        cfg.skill.Skill table = GameTableManager.Instance.Tables.TbSkill.GetOrDefault(mapping.skillId);
        if (table == null || table.Atk == null)
        {
            Debug.LogError($"ThroughWorld 技能表缺少伤害配置 {mapping.skillId}", this);
            return false;
        }
        return TryCast(mapping, table, target, out session);
    }

    /// <summary>
    /// 使用已加载的技能表行提交施法 测试场景可独立加载表文件
    /// </summary>
    public bool TryCast(ThroughWorldBattleSkill mapping, cfg.skill.Skill table, GameObject target, out SkillCastSession session)
    {
        session = null;
        if (role == null || caster == null || mapping == null || mapping.skill == null || mapping.timeline == null ||
            table == null || table.Atk == null || table.Id != mapping.skillId)
        {
            Debug.LogError("ThroughWorld Battle 技能映射与技能表数据不匹配", this);
            return false;
        }
        caster.AddSkill(mapping.skill);
        Vector3 origin = caster.GetOriginPosition();
        Vector3 point = target != null ? target.transform.position : origin;
        Vector3 direction = target != null ? point - origin : transform.forward;
        return caster.TryCast(new SkillCastRequest
        {
            Skill = mapping.skill,
            Timeline = mapping.timeline,
            TableDamage = new SkillTableDamage(table.Atk.Num, table.Atk.Percent),
            RangeOverride = table.AtkRang,
            CooldownOverride = table.Cd,
            Target = target,
            Point = point,
            Direction = direction.sqrMagnitude > 0 ? direction.normalized : transform.forward
        }, out session);
    }

    /// <summary>
    /// 打断当前 Battle 施法
    /// </summary>
    public void CancelCast()
    {
        if (caster != null) caster.CancelCast();
    }
}
