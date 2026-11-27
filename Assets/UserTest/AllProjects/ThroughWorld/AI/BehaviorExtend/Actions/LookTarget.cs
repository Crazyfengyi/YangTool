using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

/// <summary>
/// 按配置转向当前目标
/// </summary>
[TaskCategory("RoleAI/Motion")]
[TaskDescription("沿水平方向转向当前目标")]
public class LookTarget : Action
{
    public LookType lookType; // 转向方式
    public float speed = 10f; // 转向速度
    private RoleBase roleBase; // 当前角色

    /// <summary>
    /// 缓存角色引用
    /// </summary>
    public override void OnAwake()
    {
        roleBase = GetComponent<RoleBase>();
    }

    /// <summary>
    /// 目标丢失时结束 转向完成后返回成功
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var root = BehaviorSetting.Root(roleBase);
        if (root == null || roleBase.Target == null) return TaskStatus.Failure;
        if (lookType != LookType.Lerp && lookType != LookType.Immediate) return TaskStatus.Failure;
        return BehaviorSetting.Face(root, roleBase.Target.transform.position - root.position, speed, lookType == LookType.Immediate);
    }
}

/// <summary>
/// 转向执行方式
/// </summary>
public enum LookType
{
    Lerp,
    Immediate,
}
