using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using DataStruct;
using UnityEngine;

/// <summary>
/// 使用统一控制器启动当前待执行技能
/// </summary>
[TaskCategory("RoleAI/Motion")]
[TaskDescription("使用统一控制器启动当前待执行技能")]
public class UseCurrentSkill : SkillUseAction
{
    [HideInInspector] public bool isImmediatelyCooling; // 兼容旧配置 冷却时点由控制器统一处理
}
