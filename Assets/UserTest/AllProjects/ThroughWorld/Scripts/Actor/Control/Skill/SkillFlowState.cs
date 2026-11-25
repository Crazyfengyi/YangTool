using DataStruct;

/// <summary>
/// 角色技能请求的当前阶段
/// </summary>
public enum SkillRunState
{
    Idle,
    Pending,
    Executing
}

/// <summary>
/// 一次技能请求的最终结果
/// </summary>
public enum SkillRunResult
{
    None,
    Completed,
    Cancelled,
    Interrupted,
    Failed
}

/// <summary>
/// 技能请求未能继续的原因
/// </summary>
public enum SkillUseFailReason
{
    None,
    Busy,
    InvalidRole,
    InvalidSkill,
    InvalidTarget,
    OutOfRange,
    OnCooldown,
    OnCommonCooldown,
    MissingResource,
    InvalidExecutor,
    MissingBinding,
    RequestTimeout,
    ExecutionFailed
}

/// <summary>
/// 结束通知的不可变快照 用于区分请求编号和是否已实际开始
/// </summary>
public readonly struct SkillExecutionResult
{
    public readonly int ExecutionId; // 本次请求编号
    public readonly RunTimeSkillData Skill; // 本次运行时技能
    public readonly SkillRunResult Result; // 最终结果
    public readonly SkillUseFailReason Reason; // 失败或取消原因
    public readonly bool Started; // 是否实际启动过执行器

    /// <summary>
    /// 保存一次技能结束时的状态
    /// </summary>
    public SkillExecutionResult(int executionId, RunTimeSkillData skill, SkillRunResult result,
        SkillUseFailReason reason, bool started)
    {
        ExecutionId = executionId;
        Skill = skill;
        Result = result;
        Reason = reason;
        Started = started;
    }
}
