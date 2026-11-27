using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;
using Pathfinding;

/// <summary>
/// 在角色初始位置周边选择巡逻点
/// </summary>
[TaskCategory("RoleAI/Motion")]
[TaskDescription("在角色初始位置周边选择巡逻点")]
public class PatrolAction : Action
{
    public bool isLookTargetPoint = true; // 是否转向运动方向
    public float rotaSpeed = 10f; // 转向速度
    [SerializeField] private float radius = 1f; // 巡逻半径

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
        if (!(radius > 0)) return;
        var offset = Random.insideUnitCircle * radius;
        Vector3 destination = role.StartPos + new Vector3(offset.x, 0, offset.y);
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
