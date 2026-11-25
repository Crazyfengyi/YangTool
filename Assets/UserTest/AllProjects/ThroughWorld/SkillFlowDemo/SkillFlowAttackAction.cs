using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 播放现有史莱姆模型的攻击动画 配合后续等待和伤害节点
/// </summary>
[TaskCategory("ThroughWorld/技能流程测试")]
public class SkillFlowAttackAction : Action
{
    /// <summary>
    /// 动画只负责表现 伤害仍由独立检测节点决定
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        RoleBase role = GetComponent<RoleBase>();
        if (role == null || role.Animator == null) return TaskStatus.Failure;
        role.Animator.CrossFade("Attack", 0.05f);
        return TaskStatus.Success;
    }
}
