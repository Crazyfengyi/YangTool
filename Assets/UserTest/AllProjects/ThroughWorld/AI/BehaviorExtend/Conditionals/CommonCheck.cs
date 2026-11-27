using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 按现有技能状态检查角色 旧武器滑步和宠物条件缺少接口
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("按现有技能状态检查角色 旧武器滑步和宠物条件缺少接口")]
public class CommonCheck : Conditional
{
    public CheckConditionType checkConditionType; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null) return TaskStatus.Failure;
        var skill = role.SkillControl;
        switch (checkConditionType)
        {
            case CheckConditionType.是否在攻击中: return BehaviorSetting.Result(skill?.IsExecuting == true);
            case CheckConditionType.是否在使用技能中: return BehaviorSetting.Result(skill?.NeedUseSkill == true);
            case CheckConditionType.技能是否为TimeLine: return BehaviorSetting.Result(skill?.NeedUseSkill == true && skill.IsTimeLine);
            case CheckConditionType.技能是否为BehaverTree: return BehaviorSetting.Result(skill?.NeedUseSkill == true && !skill.IsTimeLine);
            case CheckConditionType.目标是否在当前技能范围内: return BehaviorSetting.Result(BehaviorSetting.InSkillRange(role));
            case CheckConditionType.是否已经是默认位置和旋转:
                var root = BehaviorSetting.Root(role);
                var direction = BehaviorSetting.Record(role).Direction;
                return BehaviorSetting.Result(root != null && direction.HasValue &&
                    (role.transform.position - role.StartPos).sqrMagnitude < 0.01f &&
                    Vector3.Angle(root.forward, direction.Value) <= 1f);
            case CheckConditionType.是否和看下点同方向:
                var model = BehaviorSetting.Root(role);
                var point = BehaviorSetting.Record(role).Position;
                if (model == null || !point.HasValue) return TaskStatus.Failure;
                var offset = point.Value - model.position;
                offset.y = 0;
                return BehaviorSetting.Result(Vector3.Angle(model.forward, offset) <= 1f);
            default: return BehaviorSetting.Unsupported(this, $"旧角色条件未映射 {checkConditionType}");
        }
    }
}

/// <summary>
/// 保留旧行为树的选项顺序
/// </summary>
public enum CheckConditionType
{
    None,
    是否在攻击中,
    是否在滑步中,
    是否在使用技能中,
    是否已经是默认位置和旋转,
    技能是否为TimeLine,
    技能是否为BehaverTree,
    是否角色切换职业中,
    是否拿出武器中,
    是否和看下点同方向,
    滑步打断值是否为true和有输入,
    目标是否在当前技能范围内,
    宠物专用_和主人的距离是否大于跟随距离,
    宠物专用_和主人的距离是否大于强制跟随距离,
    检测是否强制跟随主人中,
    宠物是否在房间寻路网格上,
    是否需要激活,
}
