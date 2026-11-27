using BehaviorDesigner.Runtime.Tasks;
using Sirenix.OdinInspector;
using BehaviorDesigner.Runtime;

/// <summary>
/// 由 AI 行为树提交桥接技能并等待同一 Battle 会话结束
/// </summary>
[TaskCategory("ThroughWorld/技能")]
public sealed class ThroughWorldBattleSkillAction : Action
{
    [LabelText("技能映射")] public ThroughWorldBattleSkill mapping; // 对应技能表与时间线
    public SharedGameObject target; // 可选显式目标 默认使用角色当前目标
    public UnityEngine.TextAsset demoTable; // 独立演示可指定 Luban JSON 正式使用已加载的表
    private ThroughWorldBattleAdapter adapter; // 当前角色适配器
    private SkillCastSession session; // 当前节点提交的施法会话

    /// <summary>
    /// 取得角色目标并提交技能
    /// </summary>
    public override void OnStart()
    {
        adapter = GetComponent<ThroughWorldBattleAdapter>();
        RoleBase role = GetComponent<RoleBase>();
        session = null;
        if (adapter == null || role == null || mapping == null) return;
        var selectedTarget = target != null && target.Value != null ? target.Value : role.Target;
        if (demoTable == null) adapter.TryCast(mapping, selectedTarget, out session);
        else
        {
            try
            {
                var table = new cfg.skill.TbSkill(SimpleJSON.JSON.Parse(demoTable.text));
                adapter.TryCast(mapping, table.GetOrDefault(mapping.skillId), selectedTarget, out session);
            }
            catch (System.Exception exception)
            {
                UnityEngine.Debug.LogException(exception, gameObject);
            }
        }
    }

    /// <summary>
    /// 成功完成返回成功 被拒绝或打断返回失败
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (session == null) return TaskStatus.Failure;
        if (!session.IsCompleted) return TaskStatus.Running;
        return session.WasInterrupted ? TaskStatus.Failure : TaskStatus.Success;
    }

    /// <summary>
    /// 节点中止时只取消仍在执行的会话
    /// </summary>
    public override void OnEnd()
    {
        if (adapter != null && session != null && !session.IsCompleted) adapter.CancelCast();
    }
}
