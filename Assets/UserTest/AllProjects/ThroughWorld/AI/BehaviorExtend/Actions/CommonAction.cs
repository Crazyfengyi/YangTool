using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 执行当前角色支持的通用动作 旧职业武器和宠物分支缺少接口
/// </summary>
[TaskIcon(BehaviorSetting.path + "Common.png")]
[TaskCategory("RoleAI/Motion")]
[TaskDescription("执行当前角色支持的通用动作 旧职业武器和宠物分支缺少接口")]
public class CommonAction : Action
{
    public CommonActionType commonActionType; // 保留原节点配置
    public bool boolValue; // 保留原节点配置
    public float floatVaule; // 保留原节点配置
    public string strValue; // 保留原节点配置

    /// <summary>
    /// 执行当前节点并返回处理结果
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        var role = GetComponent<RoleBase>();
        if (role == null) return TaskStatus.Failure;
        switch (commonActionType)
        {
            case CommonActionType.设置血条显隐:
                if (role.HealthControl?.healthBar == null) return TaskStatus.Failure;
                role.HealthControl.ToogleHpBarShow(boolValue);
                return TaskStatus.Success;
            case CommonActionType.播放特效_跟随:
            case CommonActionType.播放特效_不跟随:
                return BehaviorSetting.PlayRoleEffect(this, role, strValue, (ModelPointType)floatVaule,
                    commonActionType == CommonActionType.播放特效_跟随);
            case CommonActionType.设置技能进入冷却:
                if (role.SkillControl == null || role.SkillControl.NeedUseSkill) return TaskStatus.Failure;
                role.SkillControl.StartCommonCooldown();
                return TaskStatus.Success;
            case CommonActionType.死亡关闭碰撞体:
                if (role.HealthControl == null || role.HealthControl.GetCurrentHp() > 0 || role.FirstCollider == null) return TaskStatus.Failure;
                role.FirstCollider.enabled = false;
                return TaskStatus.Success;
            case CommonActionType.NULL_占位:
                return TaskStatus.Success;
            default:
                return BehaviorSetting.Unsupported(this, $"旧通用动作未映射 {commonActionType}");
        }
    }
}

/// <summary>
/// 保留旧行为树的选项顺序
/// </summary>
public enum CommonActionType
{
    None,
    设置血条显隐,
    设置是否激活,
    设置是否攻击中,
    设置普攻冷却时间,
    设置手上武器显隐,
    设置背上武器显隐,
    设置技能进入冷却,
    玩家专用_设置特殊动作结束,
    设置切换职业完成,
    玩家专用_切换职业,
    玩家专用_拿出武器结束,
    玩家专用_需要拿出武器,
    播放特效_跟随,
    播放特效_不跟随,
    清理循环特效,
    设置负面状态,
    清理循环音效,
    死亡关闭碰撞体,
    设置滑步打断值,
    宠物专用_跟随主人,
    设置是否强制跟随中,
    激活点,
    NULL_占位,
    死亡动画前,
    清空临时特效,
}
