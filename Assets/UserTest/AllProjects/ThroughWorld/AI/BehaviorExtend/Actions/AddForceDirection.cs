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

//[TaskIcon(BehaviorSetting.path + "推.png")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("需要至少记录一个点")]
//public class AddForceDirection : Action
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
//    /// <summary>
//    /// 开始点
//    /// </summary>
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
//    /// 持续时间
//    /// </summary>
//    public float durationTime = 2f;
//    /// <summary>
//    /// 力的大小
//    /// </summary>
//    public float forceSize = 10;
//    /// <summary>
//    /// 开始旋转
//    /// </summary>
//    private Vector3 startRota;
//    /// <summary>
//    /// 目标方向
//    /// </summary>
//    private Vector3 targetDirection;
//    /// <summary>
//    /// 击中特效名称
//    /// </summary>
//    public string hitEffectName = "爪子击中";
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
//        targetDirection = targetPos - gameObject.transform.position;

//        roleBase.OnCollisionEnterCallBack += OtherEnter;
//        roleBase.OnCollisionExitCallBack += OtherStay;
//        roleBase.OnCollisionStayCallBack += OtherExit;
//        roleBase.roleMove.AddVelocity(forceSize * targetDirection.normalized * 1.6f, Physics.gravity, durationTime);
//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (time >= durationTime)
//        {
//            return TaskStatus.Success;
//        }

//        time += Time.deltaTime;
//        return TaskStatus.Running;
//    }

//    public override void OnEnd()
//    {
//        roleBase.rigbody.velocity = Vector3.zero;
//        roleBase.OnCollisionEnterCallBack -= OtherEnter;
//        roleBase.OnCollisionExitCallBack -= OtherStay;
//        roleBase.OnCollisionStayCallBack -= OtherExit;
//        //roleBase.ClearAllTempEffect();
//    }

//    /// <summary>
//    /// 其他角色碰撞开始
//    /// </summary>
//    public void OtherEnter(RoleBase otherRole)
//    {
//        DamageInfo damageInfo = roleBase.GetDamageInfo(DamageType.物理伤害);
//        DamageInfo resultDamageInfo = otherRole.GetHitCompute(damageInfo);

//        if (!string.IsNullOrEmpty(hitEffectName))
//        {
//            HitEffectInfo hitEffectInfo = new HitEffectInfo();
//            hitEffectInfo.effectName = hitEffectName;
//            hitEffectInfo.isFollowRole = true;
//            hitEffectInfo.effectPointType = EffectPointType.Body;
//            otherRole.ShowBeHitEffect(hitEffectInfo);
//        }

//        otherRole.BeAttackProcess(resultDamageInfo);

//        //TODO:宠物猪临时写死
//        if (roleBase.name.Contains("Pet_100001"))
//        {
//            otherRole.AddBuff(BuffID.眩晕);
//        }
//    }

//    /// <summary>
//    /// 其他角色碰撞中
//    /// </summary>
//    public void OtherStay(RoleBase roleBase)
//    {

//    }

//    /// <summary>
//    /// 其他角色碰撞结束
//    /// </summary>
//    public void OtherExit(RoleBase roleBase)
//    {

//    }

//}