using DataStruct;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 复用 ThroughWorld 伤害检测的测试角色 不创建正式 HUD 或本地存档
/// </summary>
public class SkillFlowDemoRole : RoleBase
{
    [SerializeField, LabelText("模型信息")] private ModelInfo demoModel; // 场景显式绑定的模型
    [SerializeField, Min(1), LabelText("初始生命")] private float initialHealth = 200; // 靶子的重置生命
    private float currentHealth; // 当前演示生命
    private int receivedHits; // 实际收到的伤害次数

    public float CurrentHealth => currentHealth;
    public int ReceivedHits => receivedHits;

    /// <summary>
    /// 只初始化演示需要的技能控制和目标生命
    /// </summary>
    public override void IInit()
    {
        ModelInfo = demoModel;
        InitializeSkillControl();
        InitRecord();
        ResetHealth();
    }

    /// <summary>
    /// 从本次技能取得固定伤害 不依赖角色属性表
    /// </summary>
    public override DamageInfo GetDamageInfo()
    {
        return new DamageInfo { damage = SkillControl?.CurrentRunTimeSkillData?.skill.Atk.Num ?? 0 };
    }

    /// <summary>
    /// 靶子只记录血量与次数 保持物体存活以便重复测试
    /// </summary>
    public override void BeHit(ref DamageInfo damageInfo)
    {
        currentHealth = Mathf.Max(0, currentHealth - Mathf.Abs(damageInfo.damage));
        receivedHits++;
        Debug.Log($"[技能流程测试] 命中 {name} 伤害 {damageInfo.damage} 剩余生命 {currentHealth}", this);
        if (currentHealth <= 0) IDie();
    }

    /// <summary>
    /// 测试目标不使用正式属性表进行防御计算
    /// </summary>
    public override DamageInfo GetHitCompute(DamageInfo damageInfo)
    {
        return damageInfo;
    }

    /// <summary>
    /// 恢复演示生命与命中次数
    /// </summary>
    internal void ResetHealth()
    {
        currentHealth = initialHealth;
        receivedHits = 0;
    }

    /// <summary>
    /// 测试施法者使用演示血量 避免读取正式 HUD 的生命对象
    /// </summary>
    public override bool IsCanAtk()
    {
        return isActiveAndEnabled && currentHealth > 0;
    }

    /// <summary>
    /// 测试靶子以当前血量参与技能目标校验
    /// </summary>
    public override bool IsCanBeHit()
    {
        return isActiveAndEnabled && currentHealth > 0;
    }
}
