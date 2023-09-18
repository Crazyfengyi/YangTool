///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-17 
//*/
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;
//using DataStruct;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]

//public class CheckIsDeState : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 负面状态
//    /// </summary>
//    public DeControlStateType deControlStateType;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        if (!roleBase)
//        {
//            Debuger.ToError($"{gameObject.name}的CheckIsDeState中RoleBase未找到");
//        }
//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (roleBase.roleState.IsHaveDeState(deControlStateType))
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }
//}