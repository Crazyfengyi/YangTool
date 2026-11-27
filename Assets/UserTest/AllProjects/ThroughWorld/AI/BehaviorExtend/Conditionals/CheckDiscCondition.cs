using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 比较角色与目标之间的距离
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("比较角色与目标之间的距离")]
public class CheckDiscContion : Conditional
{
    public MathCompare mathCompare = MathCompare.大于; // 保留原节点配置
    public float targetValue; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Result(Compare());
    }
    /// <summary>
    /// 有目标时比较距离
    /// </summary>
    public bool Compare()
    {
        var role = GetComponent<RoleBase>();
        return role != null && role.Target != null && BehaviorSetting.Compare(Vector3.Distance(role.transform.position, role.Target.transform.position), targetValue, mathCompare);
    }
}
