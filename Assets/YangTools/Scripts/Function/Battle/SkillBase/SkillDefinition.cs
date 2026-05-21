using System.Collections.Generic;
using UnityEngine;

/**
 * 技能定义类,存储和定义技能的基本属性和行为
 */
[CreateAssetMenu(menuName = "Game/Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    public string Id; //技能的唯一标识符
    public string DisplayName; //技能的显示名称
    [TextArea] 
    public string Description; //技能的描述信息
    public Sprite Icon; //技能的图标

    [Header("Cast")]
    public SkillTargetType TargetType = SkillTargetType.Unit; //技能的目标类型
    [Min(0f)] public float CastTime = 0f; //技能的施放时间
    [Min(0f)] public float Cooldown = 1f; //技能的冷却时间
    [Min(0f)] public float Range = 5f; //技能的作用范围
    public bool ExecuteOnAnimationEvent;//在动画事件中执行
    [Min(0f)] 
    public float AnimationEventTimeout = 0f;//动画事件退出时间
    public SkillCost Cost = new SkillCost(); //技能的使用消耗，包含资源、魔法值等消耗类型

    [Header("Effects")]
    public List<SkillEffect> Effects = new List<SkillEffect>(); //技能效果列表，可以包含多个效果组合
}
