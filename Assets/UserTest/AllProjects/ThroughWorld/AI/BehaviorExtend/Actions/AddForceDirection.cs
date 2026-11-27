using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 沿记录点方向为现有角色移动组件施加冲刺动量
/// </summary>
[TaskIcon(BehaviorSetting.path + "推.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("沿记录点方向施加动量 旧冲刺碰撞特效需由其他节点处理")]
public class AddForceDirection : Action
{
    public bool isLookTargetPoint = true; // 保留原节点配置
    public float rotaSpeed = 10f; // 保留原节点配置
    public float durationTime = 2f; // 保留原节点配置
    public float forceSize = 10; // 保留原节点配置
    public string hitEffectName = "爪子击中"; // 保留原节点配置
    private CMF.AdvancedWalkerController mover; // 本轮使用的角色移动组件
    private RoleBase role; // 当前角色
    private Vector3 direction; // 本轮冲刺方向
    private float originalMomentum; // 冲刺方向上的原始动量
    private float endTime; // 冲刺结束时间
    private bool started; // 本轮是否已施加动量

    /// <summary>
    /// 校验配置并施加一次动量
    /// </summary>
    public override void OnStart()
    {
        started = false;
        if (!string.IsNullOrWhiteSpace(hitEffectName)) return;
        role = GetComponent<RoleBase>();
        mover = GetComponent<CMF.AdvancedWalkerController>();
        if (role == null || mover == null || !mover.isActiveAndEnabled ||
            !(durationTime > 0) || float.IsInfinity(durationTime) || !(forceSize > 0) || float.IsInfinity(forceSize)) return;
        var point = BehaviorSetting.Record(role).Position;
        if (!point.HasValue) return;
        direction = point.Value - role.transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude < 0.000001f) return;
        direction.Normalize();
        originalMomentum = Vector3.Dot(mover.GetMomentum(), direction);
        mover.AddMomentum(direction * forceSize);
        endTime = Time.time + durationTime;
        started = true;
    }

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (!string.IsNullOrWhiteSpace(hitEffectName))
            return BehaviorSetting.Unsupported(this, "旧冲刺碰撞特效未接入 仅移动时请清空命中特效字段");
        if (!started || mover == null || !mover.isActiveAndEnabled) return TaskStatus.Failure;
        if (isLookTargetPoint) BehaviorSetting.Face(BehaviorSetting.Root(role), direction, rotaSpeed);
        return Time.time >= endTime ? TaskStatus.Success : TaskStatus.Running;
    }

    /// <summary>
    /// 移除冲刺方向上仍有的附加动量
    /// </summary>
    public override void OnEnd()
    {
        if (started && mover != null)
        {
            float remaining = Mathf.Clamp(Vector3.Dot(mover.GetMomentum(), direction) - Mathf.Max(0, originalMomentum), 0, forceSize);
            if (remaining > 0) mover.AddMomentum(-direction * remaining);
        }
        started = false;
    }
}
