using BehaviorDesigner.Runtime.Tasks;

/// <summary>
/// 判断角色是否持有目标
/// </summary>
[TaskCategory("RoleAI/Conditionals")]
[TaskDescription("检查角色当前目标")]
public class HaveTarget : Conditional
{
    private RoleBase role; // 当前角色

    /// <summary>
    /// 缓存角色引用
    /// </summary>
    public override void OnAwake()
    {
        role = GetComponent<RoleBase>();
    }

    /// <summary>
    /// 角色及目标均有效时通过
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        return BehaviorSetting.Result(role != null && role.Target != null);
    }
}
