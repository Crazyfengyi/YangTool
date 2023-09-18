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
//[TaskDescription("需要先记录攻击点")]
//public class LookRigidBody : Action
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

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }
//    public override void OnStart()
//    {
//        startRota = gameObject.transform.eulerAngles;
//        if (roleBase.rigbody.velocity != Vector3.zero)
//        {
//            targetRota = roleBase.rigbody.velocity;
//        }
//        else
//        {
//            targetRota = roleBase.Model.forward;
//        }
//    }
//    public override TaskStatus OnUpdate()
//    {
//        //方向
//        roleBase.Model.forward = Vector3.Slerp(roleBase.Model.forward, targetRota.normalized, speed * Time.deltaTime);

//        //转向到小于3度时
//        if (Vector3.Angle(roleBase.Model.forward.normalized, targetRota.normalized) < 3)
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Running;
//    }
//}