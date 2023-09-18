///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-12 
//*/
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// 是否激活的
///// </summary>
//public class CheckIsInActive : Conditional
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
//        if (roleBase.GetBoolMark(DataStruct.BoolUseEnum.IsInactive))
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }
//}