using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 从记录点发起当前技能的范围伤害检测
/// </summary>
[TaskIcon(BehaviorSetting.path + "攻击.png")]
[TaskCategory("RoleAI/Atk")]
[TaskDescription("从记录点发起当前技能的范围伤害检测")]
public class AreaDamage : Action
{
    public HitCheckType areaDamageType; // 保留原节点配置
    public float rang = 10f; // 保留原节点配置
    public float angle = 0f; // 保留原节点配置
    public Vector3 OffsetVector; // 保留原节点配置
    public Vector3 Rotate; // 保留原节点配置
    public string hitEffectName; // 保留原节点配置
    public string hitSoundName; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        var root = BehaviorSetting.Root(role);
        if (root == null || role.SkillControl?.IsExecuting != true || !role.IsCanAtk() || !(rang > 0)) return TaskStatus.Failure;
        var point = BehaviorSetting.Record(role).Position;
        if (!point.HasValue) return TaskStatus.Failure;
        if (areaDamageType != HitCheckType.圆形 && areaDamageType != HitCheckType.矩形 && areaDamageType != HitCheckType.扇形)
            return TaskStatus.Failure;
        if (Rotate != Vector3.zero || !string.IsNullOrWhiteSpace(hitEffectName) || !string.IsNullOrWhiteSpace(hitSoundName))
            return BehaviorSetting.Unsupported(this, "当前伤害检测入口不支持独立旋转或命中特效音效配置");
        var delta = point.Value - role.transform.position;
        var config = new CheckConfig { checkType = areaDamageType, radius = rang, diameter = rang * 2,
            Angle = angle, Long = rang, Width = angle > 0 ? angle : rang, Hight = rang,
            offsetVector = OffsetVector + new Vector3(Vector3.Dot(root.right, delta), Vector3.Dot(root.up, delta), Vector3.Dot(root.forward, delta)) };
        return BehaviorSetting.Result(role.DamageCheck(config));
    }
}
