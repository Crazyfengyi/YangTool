///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-11 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using UnityEngine;
//using System.Collections;
//using YangToolDebuger;

//[TaskCategory("RoleAI/Motion")]
//public class LookAtkTarget : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 转身速度
//    /// </summary>
//    public float speed = 10f;
//    /// <summary>
//    /// 开始旋转
//    /// </summary>
//    private Vector3 startRota;
//    /// <summary>
//    /// 目标旋转
//    /// </summary>
//    private Vector3 targetRota;
//    private Vector3 Target
//    {
//        get
//        {
//            if (roleBase.GetTarget())
//            {
//                return roleBase.GetTarget().transform.position;
//            }

//            Debuger.ToError($"{gameObject.name}:LookAtkTarget找不到目标");
//            //容错找不到目标时,用自身
//            return roleBase.transform.position + new Vector3(0, 0, 1);
//        }
//    }

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {
//        startRota = gameObject.transform.eulerAngles;
//        targetRota = Target - gameObject.transform.position;

//        if (targetRota == Vector3.zero)
//        {
//            targetRota = roleBase.Model.forward;
//        }
//    }

//    public override TaskStatus OnUpdate()
//    {
//        roleBase.Model.transform.forward = Vector3.Slerp(roleBase.Model.forward, targetRota.normalized, speed * Time.deltaTime);

//        //转向到小于3度时
//        if (Vector3.Angle(roleBase.Model.forward.normalized, targetRota.normalized) < ProjectSettingInfo.rotateDeviation)
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Running;
//    }
//}