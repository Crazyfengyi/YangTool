///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-20 
//*/
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;
//using DataStruct;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// 检测玩家在使用技能
///// </summary>
//public class CheckIsUseSkill : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        if (!roleBase)
//        {
//            Debuger.ToError($"{gameObject.name}的CheakIsInActive中RoleBase未找到");
//        }
//    }

//    public override TaskStatus OnUpdate()
//    {
//        return roleBase.GetSkillNeedUse() ? TaskStatus.Success : TaskStatus.Failure;
//    }
//}