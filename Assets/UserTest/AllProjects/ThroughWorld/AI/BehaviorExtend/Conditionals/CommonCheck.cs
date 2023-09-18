///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-26 
//*/
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
//[TaskDescription("通过条件检测")]
///// <summary>
///// 通用检测
///// </summary>
//public class CommonCheck : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 检测条件类型
//    /// </summary>
//    public CheckConditionType checkConditionType;
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        if (!roleBase)
//        {
//            Debuger.ToError($"{gameObject.name}的CommonCheck中RoleBase未找到");
//        }
//    }

//    public override TaskStatus OnUpdate()
//    {
//        switch (checkConditionType)
//        {
//            case CheckConditionType.是否在攻击中:
//                return roleBase.GetBoolMark(DataStruct.BoolUseEnum.IsInAttackTag) ? TaskStatus.Success : TaskStatus.Failure;
//            case CheckConditionType.是否在滑步中:
//                return ((Player)roleBase).isInDodgeSlide ? TaskStatus.Success : TaskStatus.Failure;
//            case CheckConditionType.是否在使用技能中:
//                return roleBase.GetSkillNeedUse() ? TaskStatus.Success : TaskStatus.Failure;
//            case CheckConditionType.是否已经是默认位置和旋转:
//                {
//                    float ratate = Vector3.Angle(roleBase.Model.forward, roleBase.initialDirection);
//                    float distance = Vector3.Distance(roleBase.transform.position, roleBase.initialPoint);
//                    if (distance < ProjectSettingInfo.navMeshAgentDeviation && ratate < ProjectSettingInfo.rotateDeviation)
//                    {
//                        return TaskStatus.Success;
//                    }
//                    return TaskStatus.Failure;
//                }
//            case CheckConditionType.技能是否为TimeLine:
//                {
//                    RoleSkill currentSkill = roleBase.GetCurrentSkill();
//                    if (currentSkill != null && currentSkill.SkillData.isTimeline)
//                    {
//                        return TaskStatus.Success;
//                    }
//                    return TaskStatus.Failure;
//                }
//            case CheckConditionType.技能是否为BehaverTree:
//                {
//                    RoleSkill currentSkill = roleBase.GetCurrentSkill();
//                    if (currentSkill != null && currentSkill.SkillData.isTimeline == false)
//                    {
//                        return TaskStatus.Success;
//                    }
//                    return TaskStatus.Failure;
//                }
//            case CheckConditionType.是否角色切换职业中:
//                {
//                    return roleBase.GetBoolMark(DataStruct.BoolUseEnum.IsTakeBackWeapon) ? TaskStatus.Success : TaskStatus.Failure;
//                }
//            case CheckConditionType.是否拿出武器中:
//                return roleBase.GetBoolMark(DataStruct.BoolUseEnum.IsTakeWeapon) ? TaskStatus.Success : TaskStatus.Failure;
//            case CheckConditionType.是否和看下点同方向:
//                {
//                    var targetRota = roleBase.atkPoint - gameObject.transform.position;
//                    return Vector3.Angle(roleBase.Model.forward.normalized, targetRota.normalized) < ProjectSettingInfo.rotateDeviation ? TaskStatus.Success : TaskStatus.Failure;
//                }
//            case CheckConditionType.滑步打断值是否为true和有输入:
//                return roleBase.GetBoolMark(DataStruct.BoolUseEnum.滑步打断值) && RoleManager.playerHaveInput ? TaskStatus.Success : TaskStatus.Failure;
//            case CheckConditionType.目标是否在当前技能范围内:

//                if (roleBase.GetCurrentSkill() != null)
//                {
//                    //技能使用距离
//                    float skilluseDistance = roleBase.GetCurrentSkill().UseDistance;
//                    //目标
//                    RoleBase target = roleBase.GetTarget();
//                    //角色与目标的距离
//                    float distance = RoleBase.Distance(roleBase, target);

//                    return distance <= skilluseDistance ? TaskStatus.Success : TaskStatus.Failure;
//                }
//                return TaskStatus.Failure;
//            case CheckConditionType.宠物专用_和主人的距离是否大于跟随距离:
//                {
//                    PetBase pet = (PetBase)roleBase;
//                    RoleBase master = ((PetBase)roleBase).master;
//                    if (master == null)
//                    {
//                        return TaskStatus.Failure;
//                    }

//                    if (pet.Distance(master) >= 2f)
//                    {
//                        return TaskStatus.Success;
//                    }
//                    return TaskStatus.Failure;
//                }
//            case CheckConditionType.宠物专用_和主人的距离是否大于强制跟随距离:
//                {
//                    PetBase pet = (PetBase)roleBase;
//                    RoleBase master = ((PetBase)roleBase).master;
//                    if (master == null)
//                    {
//                        return TaskStatus.Failure;
//                    }

//                    if (pet.Distance(master) >= ProjectSettingInfo.petMaxDistance)
//                    {
//                        return TaskStatus.Success;
//                    }

//                    return TaskStatus.Failure;
//                }
//            case CheckConditionType.检测是否强制跟随主人中:
//                return roleBase.GetBoolMark(DataStruct.BoolUseEnum.强制跟随主人中) ? TaskStatus.Success : TaskStatus.Failure;
//            case CheckConditionType.宠物是否在房间寻路网格上:
//                return ((PetBase)roleBase).IsOnNavMesh() ? TaskStatus.Success : TaskStatus.Failure;

//            case CheckConditionType.是否需要激活:
//                return roleBase.GetBoolMark(DataStruct.BoolUseEnum.是否需要激活) ? TaskStatus.Success : TaskStatus.Failure;
//            default:
//                Debuger.ToError("没识别到通用CommonCheck类型");
//                break;
//        }

//        return TaskStatus.Failure;
//    }
//}

///// <summary>
///// 检测条件类型
///// </summary>
//public enum CheckConditionType
//{
//    None,
//    是否在攻击中,
//    是否在滑步中,
//    是否在使用技能中,
//    是否已经是默认位置和旋转,
//    技能是否为TimeLine,
//    技能是否为BehaverTree,
//    是否角色切换职业中,
//    是否拿出武器中,
//    是否和看下点同方向,
//    滑步打断值是否为true和有输入,
//    目标是否在当前技能范围内,
//    宠物专用_和主人的距离是否大于跟随距离,
//    宠物专用_和主人的距离是否大于强制跟随距离,
//    检测是否强制跟随主人中,
//    宠物是否在房间寻路网格上,
//    是否需要激活,
//}