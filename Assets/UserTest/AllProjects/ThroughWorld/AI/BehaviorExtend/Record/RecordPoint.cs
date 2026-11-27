using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 记录自身或目标模型挂点 旧跳跃选点和曲线选点需要专用接口
/// </summary>
[TaskIcon(BehaviorSetting.path + "绿点.png")]
[TaskCategory("RoleAI/Record")]
[TaskDescription("记录自身或目标模型挂点 旧跳跃选点和曲线选点需要专用接口")]
public class RecordPoint : Conditional
{
    public RecordPointType recordPointType; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null) return TaskStatus.Failure;
        var record = BehaviorSetting.Record(role);
        if (recordPointType == RecordPointType.记录初始旋转到公共列表)
        {
            var root = BehaviorSetting.Root(role);
            record.Direction = root != null ? root.forward : (Vector3?)null;
            return BehaviorSetting.Result(record.Direction.HasValue);
        }
        record.Position = null;
        var type = recordPointType;
        if (type == RecordPointType.记录怪物跳跃到玩家身边的选点 || type == RecordPointType.记录远程攻击点_贝塞尔目标点)
            return BehaviorSetting.Unsupported(this, "没有此专用选点接口");
        bool useTarget = type >= RecordPointType.攻击目标位置 && type <= RecordPointType.攻击目标身体位置;
        var actor = useTarget ? (role.Target != null ? role.Target.GetComponentInParent<RoleBase>() : null) : role;
        if (actor == null || type < RecordPointType.攻击目标位置 || type > RecordPointType.自身身体位置)
            return TaskStatus.Failure;
        ModelPointType pointType = type switch
        {
            RecordPointType.攻击目标左手位置 or RecordPointType.自身左手位置 => ModelPointType.LeftHand,
            RecordPointType.攻击目标右手位置 or RecordPointType.自身右手位置 => ModelPointType.RightHand,
            RecordPointType.攻击目标头部位置 or RecordPointType.自身头部位置 => ModelPointType.Head,
            RecordPointType.攻击目标身体位置 or RecordPointType.自身身体位置 => ModelPointType.Body,
            _ => ModelPointType.Foot
        };
        var point = actor.ModelInfo != null ? actor.ModelInfo.GetEffectPoint(pointType) : null;
        if (point == null) return TaskStatus.Failure;
        record.Position = point.position;
        return TaskStatus.Success;
    }
}

/// <summary>
/// 保留旧行为树的选项顺序
/// </summary>
public enum RecordPointType
{
    None,
    记录怪物跳跃到玩家身边的选点,
    攻击目标位置,
    攻击目标左手位置,
    攻击目标右手位置,
    攻击目标头部位置,
    攻击目标身体位置,
    自身位置,
    自身左手位置,
    自身右手位置,
    自身头部位置,
    自身身体位置,
    记录初始旋转到公共列表,
    记录远程攻击点_贝塞尔目标点,
}
