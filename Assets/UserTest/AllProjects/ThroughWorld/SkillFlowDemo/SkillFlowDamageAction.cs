using BehaviorDesigner.Runtime.Tasks;
using DataStruct;

/// <summary>
/// 技能行为树在前摇之后通过 ThroughWorld 入口执行一次伤害检测
/// </summary>
[TaskCategory("ThroughWorld/技能流程测试")]
[TaskDescription("使用 RoleBase.DamageCheck 执行一次实际范围检测")]
public class SkillFlowDamageAction : Action
{
    public CheckConfig checkConfig = new CheckConfig(); // 与时间线相同的攻击范围

    /// <summary>
    /// 无目标也正常结束该阶段 命中由物理检测和伤害流程决定
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        RoleBase role = GetComponent<RoleBase>();
        if (role == null || role.SkillControl == null || !role.SkillControl.NeedUseSkill)
            return TaskStatus.Failure;
        role.DamageCheck(checkConfig);
        return TaskStatus.Success;
    }
}
