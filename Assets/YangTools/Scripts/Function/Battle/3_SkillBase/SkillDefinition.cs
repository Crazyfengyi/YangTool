using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 技能定义类 存储和定义技能的基本属性和行为
/// </summary>

[CreateAssetMenu(menuName = "Game/Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    [Title("基础信息")]
    [LabelText("技能标识")]
    public string Id; //技能的唯一标识符
    [LabelText("显示名称")]
    public string DisplayName; //技能的显示名称
    [MultiLineProperty]
    [LabelText("技能描述")]
    public string Description; //技能的描述信息
    [LabelText("技能图标")]
    public Sprite Icon; //技能的图标

    [Title("施放设置")]
    [LabelText("目标类型")]
    public SkillTargetType TargetType = SkillTargetType.Unit; //技能的目标类型
    [LabelText("施放时间（秒）")]
    [MinValue(0f)] public float CastTime = 0f; //技能的施放时间
    [LabelText("冷却时间（秒）")]
    [MinValue(0f)] public float Cooldown = 1f; //技能的冷却时间
    [LabelText("作用距离")]
    [MinValue(0f)] public float Range = 5f; //技能的作用范围
    [LabelText("通过动画事件执行")]
    [PropertyTooltip("勾选后通过技能动画事件执行效果 未勾选时按施放时间执行")]
    public bool ExecuteOnAnimationEvent;//在动画事件中执行
    [MinValue(0f)]
    [LabelText("动画事件等待上限（秒）")]
    [PropertyTooltip("大于零时等待动画事件直到此时间上限 零表示持续等待动画事件")]
    public float AnimationEventTimeout = 0f;//动画事件退出时间
    [LabelText("技能消耗")]
    public SkillCost Cost = new SkillCost(); //技能的使用消耗，包含资源、魔法值等消耗类型

    [Title("技能效果")]
    [LabelText("技能效果")]
    public List<SkillEffect> Effects = new List<SkillEffect>(); //技能效果列表，可以包含多个效果组合
}
