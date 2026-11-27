using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;
using Pathfinding;

/// <summary>
/// 移动到攻击距离 记录点 初始点或随机点
/// </summary>
[TaskIcon(BehaviorSetting.path + "Move.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("移动到攻击距离 记录点 初始点或随机点")]
public class MoveTargetPos : Action
{
    public MoveType moveType; // 目的地类型
    public bool isLookTargetPoint = true; // 是否转向运动方向
    public float floatValue = 1; // 随机移动半径
    public float rotaSpeed = 10f; // 转向速度

    private RoleBase role; // 本轮移动角色
    private AIPath path; // 本轮使用的寻路组件
    private float deadline; // 寻路超时时刻
    private bool started; // 本节点是否已启动寻路

    /// <summary>
    /// 计算本轮目的地并启动现有寻路组件
    /// </summary>
    public override void OnStart()
    {
        started = false;
        role = GetComponent<RoleBase>();
        path = (role as Monster)?.AIPath;
        if (path == null && role != null) path = GetComponent<AIPath>();
        if (role == null || path == null || !path.isActiveAndEnabled || AstarPath.active == null) return;
        Vector3 destination;
        var record = BehaviorSetting.Record(role);
        switch (moveType)
        {
            case MoveType.记录点:
                if (!record.Position.HasValue) return;
                destination = record.Position.Value;
                break;
            case MoveType.记录的攻击点:
                if (!record.Attack.HasValue) return;
                destination = record.Attack.Value;
                break;
            case MoveType.移动到初始点:
                destination = role.StartPos;
                break;
            case MoveType.球形范围随机移动:
                var offset = Random.insideUnitCircle * Mathf.Max(0, floatValue);
                destination = role.transform.position + new Vector3(offset.x, 0, offset.y);
                break;
            case MoveType.根据攻击距离远离或靠近目标:
                if (role.Target == null) return;
                float? attackRange = role.SkillControl?.CurrentRunTimeSkillData?.skill?.AtkRang;
                if (!attackRange.HasValue && role is Monster monster) attackRange = monster.AIFindTargetDistance;
                if (!attackRange.HasValue || attackRange.Value < 0) return;
                var target = role.Target.transform.position;
                var direction = role.transform.position - target;
                direction.y = 0;
                if (direction.sqrMagnitude < 0.0001f) direction = role.transform.forward;
                destination = target + direction.normalized * attackRange.Value;
                break;
            default: return;
        }
        deadline = Time.time + 5f;
        path.destination = destination;
        path.isStopped = false;
        path.SearchPath();
        started = true;
    }
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (!started || path == null || !path.isActiveAndEnabled || AstarPath.active == null || Time.time >= deadline)
            return TaskStatus.Failure;
        if (moveType == MoveType.根据攻击距离远离或靠近目标 && role.Target == null) return TaskStatus.Failure;
        if (isLookTargetPoint) BehaviorSetting.Face(BehaviorSetting.Root(role), path.velocity, rotaSpeed);
        return !path.pathPending && path.reachedDestination ? TaskStatus.Success : TaskStatus.Running;
    }
    /// <summary>
    /// 停止本节点启动的寻路
    /// </summary>
    public override void OnEnd()
    {
        if (started && path != null) path.isStopped = true;
        started = false;
    }
}

/// <summary>
/// 保留旧行为树的选项顺序
/// </summary>
public enum MoveType
{
    None,
    根据攻击距离远离或靠近目标,
    记录的攻击点,
    记录点,
    球形范围随机移动,
    移动到初始点,
}
