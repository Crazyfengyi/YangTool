using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 线性技能连击配置,定义连续释放的技能组合
/// </summary>
[CreateAssetMenu(menuName = "Game/Skills/Skill Combo Definition")]
public class SkillComboDefinition : ScriptableObject
{
    // 连击输入缓冲时间（单位：秒）,玩家在这个时间内按下下一个技能键，会被视为有效输入
    [SerializeField, Min(0f)] private float inputBufferDuration = 0.25f;

    // 连击步骤列表，存储所有连击步骤
    [SerializeField] private List<SkillComboStep> steps = new List<SkillComboStep>();

    //获取输入缓冲时间（确保不小于0）
    public float InputBufferDuration => Mathf.Max(0f, inputBufferDuration);

    //获取只读的连击步骤列表
    public IReadOnlyList<SkillComboStep> Steps => steps;

    //获取连击总步数
    public int StepCount => steps.Count;

    /// <summary>
    /// 获取指定位置的连击步骤。
    /// </summary>
    /// <param name="index">要获取的步骤索引</param>
    /// <returns>如果索引有效则返回对应的连击步骤，否则返回null</returns>
    public SkillComboStep GetStep(int index)
    {
        return index >= 0 && index < steps.Count ? steps[index] : null;
    }
}

/// <summary>
/// 单个连击步骤配置。
/// 定义了连击中的每一个具体步骤，包括使用的技能、动画触发器等。
/// </summary>
[Serializable]
public class SkillComboStep
{
    //该步骤使用的技能定义
    [SerializeField] private SkillDefinition skill;
    //触发该步骤动画的触发器名称
    [SerializeField] private string animatorTrigger;
    //下一个步骤的有效时间窗口（单位：秒）,在这个时间内必须触发下一个步骤，否则连击中断
    [SerializeField, Min(0f)] private float nextStepWindow = 0.8f;
    //是否需要命中敌人才能进入下一步
    [SerializeField] private bool requireHit;
    //获取该步骤使用的技能
    public SkillDefinition Skill => skill;
    //获取动画触发器名称
    public string AnimatorTrigger => animatorTrigger;
    //获取下一个步骤的时间窗口（确保不小于0）
    public float NextStepWindow => Mathf.Max(0f, nextStepWindow);
    //获取是否需要命中敌人
    public bool RequireHit => requireHit;
}