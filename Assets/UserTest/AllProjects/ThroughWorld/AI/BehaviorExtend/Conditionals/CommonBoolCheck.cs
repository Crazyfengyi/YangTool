//    /** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       John
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-07-13 
//*/
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;

///// <summary>
/////  获取通用bool变量值
///// </summary>
//[TaskIcon(BehaviorSetting.path + "IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
//public class CommonBoolCheck : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    public DataStruct.BoolUseEnum boolUseEnum; //bool值

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        if (!roleBase)
//        {
//            Debuger.ToError($"{gameObject.name}RoleBase未找到");
//        }
//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (roleBase.GetBoolMark(boolUseEnum))
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }
//} 