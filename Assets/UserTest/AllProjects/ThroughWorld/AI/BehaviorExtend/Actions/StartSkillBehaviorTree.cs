using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 启动配置的技能树并按配置等待同一请求结束
/// </summary>
[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("启动配置的技能树并按配置等待同一请求结束")]
public class StartSkillBehaviorTree : SkillUseAction
{
    public SharedInt group; // 技能树分组
    public SharedBool waitForCompletion = false; // 是否等待技能树执行完成
    private bool started; // 本轮已尝试启动
    private bool shouldWait; // 本轮是否等待技能结束

    /// <summary>
    /// 校验技能树分组并启动请求
    /// </summary>
    public override void OnStart()
    {
        started = false;
        var role = GetComponent<RoleBase>();
        var tree = role != null ? role.SkillControl?.SkillTree : null;
        if (tree == null || tree == Owner || role.SkillControl.IsTimeLine || (group != null && tree.Group != group.Value)) return;
        shouldWait = waitForCompletion?.Value == true;
        base.OnStart();
        started = true;
    }
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (!started) return TaskStatus.Failure;
        var status = base.OnUpdate();
        return status == TaskStatus.Running && !shouldWait ? TaskStatus.Success : status;
    }
    /// <summary>
    /// 等待被打断时结束自己启动的技能
    /// </summary>
    public override void OnEnd()
    {
        if (started && shouldWait) base.OnEnd();
        started = false;
    }
}
