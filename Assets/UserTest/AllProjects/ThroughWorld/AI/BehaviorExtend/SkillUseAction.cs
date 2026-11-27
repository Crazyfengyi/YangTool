using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 启动控制器中的请求并等待该编号结束 节点不持有执行器事件
/// </summary>
[TaskDescription("启动技能并等待统一执行结果")]
[TaskCategory("RoleAI/Motion")]
[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
public class SkillUseAction : Action
{
    private SkillControl control; // 本轮调度使用的控制器
    private int executionId; // 本轮请求编号
    private bool accepted; // 是否成功开始执行

    /// <summary>
    /// 启动当前等待中的请求 同步结束也保留本轮编号
    /// </summary>
    public override void OnStart()
    {
        RoleBase role = GetComponent<RoleBase>();
        control = role != null ? role.SkillControl : null;
        executionId = control != null ? control.ExecutionId : 0;
        accepted = control != null && control.SkillTree != Owner && control.TryBeginSkill(executionId);
    }

    /// <summary>
    /// 只有本轮正常完成才返回成功 取消与打断返回失败
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (!accepted || control == null) return TaskStatus.Failure;
        if (control.ExecutionId == executionId && control.NeedUseSkill) return TaskStatus.Running;
        return control.LastResult.ExecutionId == executionId && control.LastResult.Result == SkillRunResult.Completed
            ? TaskStatus.Success : TaskStatus.Failure;
    }

    /// <summary>
    /// AI 节点被中止时只打断自己启动的请求
    /// </summary>
    public override void OnEnd()
    {
        if (accepted && control != null && control.ExecutionId == executionId && control.NeedUseSkill) control.BreakSkill();
        accepted = false;
    }
}
