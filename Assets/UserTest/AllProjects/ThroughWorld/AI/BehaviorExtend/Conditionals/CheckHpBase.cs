using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 比较角色生命值或零到一的生命比例
/// </summary>
[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
[TaskCategory("RoleAI/CheckCondition")]
[TaskDescription("比较角色生命值或零到一的生命比例")]
public class CheckHpBase : Conditional
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
    /// 按配置比较生命
    /// </summary>
    public bool Compare()
    {
        var health = GetComponent<RoleBase>()?.HealthControl;
        if (health == null) return false;
        bool percent = mathCompare is MathCompare.百分比小于 or MathCompare.百分比等于 or MathCompare.百分比大于 or MathCompare.百分比小于等于 or MathCompare.百分比大于等于;
        return BehaviorSetting.Compare(percent ? health.GetPercentHp() : health.GetCurrentHp(), targetValue, mathCompare);
    }
}

/// <summary>
/// 保留旧行为树的选项顺序
/// </summary>
public enum MathCompare
{
    None,
    小于,
    等于,
    大于,
    百分比小于,
    百分比等于,
    百分比大于,
    小于等于,
    大于等于,
    百分比小于等于,
    百分比大于等于
}
