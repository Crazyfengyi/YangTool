///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-05-18 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime.Tasks.Movement;
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.AI;
//using DataStruct;
//using YangTools;

//[TaskIcon(BehaviorSetting.path + "跳跃.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("需要至少记录一个点")]
//public class JumpToPos : Action
//{
//    /// <summary>
//    /// 是否看向目标点
//    /// </summary>
//    public bool isLookTargetPoint = true;
//    /// <summary>
//    /// 转向速度
//    /// </summary>
//    public float rotaSpeed = 10f;
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    private Vector3 startPos;
//    /// <summary>
//    /// 目标点
//    /// </summary>
//    private Vector3 targetPos;
//    /// <summary>
//    /// 记时
//    /// </summary>
//    private float time = 0f;
//    /// <summary>
//    /// 速度
//    /// </summary>
//    public float speed = 2f;
//    /// <summary>
//    /// 移动时间
//    /// </summary>
//    public float moveTime = 1f;
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
//        time = 0;
//        startRota = gameObject.transform.eulerAngles;
//        startPos = roleBase.transform.position;
//        targetPos = roleBase.recordPointList[0];
//        targetRota = targetPos - gameObject.transform.position;

//        if (targetRota == Vector3.zero)
//        {
//            targetRota = roleBase.Model.forward;
//        }

//        //TODO 需要阶段回调?播特效之类的
//        GameObject effect = roleBase.PlayEffectAtSelf("猩猩跳跃拖尾", EffectPointType.Body);
//        roleBase.loopEffectList.Add(effect);
//    }

//    // 寻找目标。代理到达目的地后返回success
//    public override TaskStatus OnUpdate()
//    {
//        if (isLookTargetPoint)
//        {
//            //转向到于3度时
//            if (Vector3.Angle(roleBase.Model.forward.normalized, targetRota.normalized) > 3)
//            {
//                roleBase.Model.forward = Vector3.Slerp(roleBase.Model.forward, targetRota.normalized, rotaSpeed * Time.deltaTime);
//            }
//        }

//        Vector3 tempPos = startPos;
//        //时间内移动到目标点
//        if (Mathf.Abs(time - moveTime) > 0.03f)
//        {
//            tempPos = Vector3.Lerp(startPos, targetPos, time / moveTime);
//        }
//        else
//        {
//            tempPos = targetPos;
//        }
//        tempPos.y = Mathf.Lerp(startPos.y, targetPos.y, time / moveTime);

//        gameObject.transform.position = tempPos;
//        time += Time.deltaTime * speed;

//        if (time > moveTime)
//        {
//            roleBase.ClearAllTempEffect();
//            gameObject.transform.position = targetPos;
//            roleBase.rigbody.velocity = Vector3.zero;
//            return TaskStatus.Success;
//        }
//        return TaskStatus.Running;
//    }
//    public override void OnEnd()
//    {
//    }
//}