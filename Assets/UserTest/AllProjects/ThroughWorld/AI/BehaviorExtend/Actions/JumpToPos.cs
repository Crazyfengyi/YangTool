using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;
using Pathfinding;

/// <summary>
/// 按配置时长移动到记录点并暂时让出寻路控制
/// </summary>
[TaskIcon(BehaviorSetting.path + "跳跃.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("按配置时长移动到记录点并暂时让出寻路控制")]
public class JumpToPos : Action
{
    public bool isLookTargetPoint = true; // 是否转向记录点
    public float rotaSpeed = 10f; // 转向速度
    public float speed = 2f; // 时间推进倍率
    public float moveTime = 1f; // 移动时长
    private RoleBase role; // 本轮移动角色
    private AIPath path; // 临时停用的寻路组件
    private bool previousCanMove; // 寻路原有移动状态
    private Vector3 from, to; // 本轮起终点
    private float elapsed; // 已经过的移动时间
    private bool started; // 本节点是否已接管移动

    /// <summary>
    /// 保存起终点并暂时接管移动
    /// </summary>
    public override void OnStart()
    {
        started = false;
        elapsed = 0;
        role = GetComponent<RoleBase>();
        if (role == null || !(speed > 0) || !(moveTime > 0) || float.IsInfinity(speed) || float.IsInfinity(moveTime)) return;
        var point = BehaviorSetting.Record(role).Position;
        if (!point.HasValue) return;
        var body = GetComponent<Rigidbody>();
        if (body != null && !body.isKinematic) return;
        from = role.transform.position;
        to = point.Value;
        path = GetComponent<AIPath>();
        if (path != null)
        {
            previousCanMove = path.canMove;
            path.canMove = false;
        }
        started = true;
    }
    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (!started || role == null) return TaskStatus.Failure;
        if (isLookTargetPoint) BehaviorSetting.Face(BehaviorSetting.Root(role), to - from, rotaSpeed);
        elapsed += Time.deltaTime * speed;
        role.transform.position = Vector3.Lerp(from, to, elapsed / moveTime);
        return elapsed >= moveTime ? TaskStatus.Success : TaskStatus.Running;
    }
    /// <summary>
    /// 同步当前位移并恢复寻路移动控制
    /// </summary>
    public override void OnEnd()
    {
        if (started && role != null && path != null)
        {
            path.Teleport(role.transform.position, false);
            path.canMove = previousCanMove;
        }
        started = false;
    }
}
