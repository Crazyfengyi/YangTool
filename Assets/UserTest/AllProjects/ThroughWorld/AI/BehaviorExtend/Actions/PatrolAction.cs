///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       John
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-07-12 
//*/
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime.Tasks.Movement;
//using UnityEngine;
//using System.Collections;
//using UnityEngine.AI;
//using DataStruct;
//using System.Collections.Generic;

///// <summary>
///// 巡逻行为
///// </summary>
//[TaskCategory("RoleAI/Motion")]
//public class PatrolAction : Action
//{
//    //寻路组件
//    protected NavMeshAgent navMeshAgent;

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
//    /// 巡逻半径
//    /// </summary>
//    private float radius = 1;

//    /// <summary>
//    /// 巡逻等待时间上下限
//    /// </summary>
//    private Vector2 randomTimeLimit = new Vector2(1,2);

//    /// <summary>
//    /// 卡死检测时间
//    /// </summary>
//    private float kTime = 3;

//    private Vector3 Target
//    {
//        get
//        {
//            navMeshAgent.stoppingDistance = 0;
            
//            //起点
//            Vector3  startPos = roleBase.initialPoint;
//            Vector2  rang = Random.insideUnitCircle * this.radius;
//            startPos.x += rang.x;
//            startPos.z += rang.y;
//            return startPos;
//        }
//    }

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        MonsterBase monsterBase = roleBase.GetComponent<MonsterBase>();
//        navMeshAgent = monsterBase.navMeshAgent;
//        this.radius = monsterBase.GetPatrolRadius();
//        this.randomTimeLimit = monsterBase.GetPatrolDelayTimeLimit();
//    }

//    public override void OnStart()
//    {
//        UpdateRotation(false);
//        for(int i = 0; i < 20; i++)
//        {
//            if(SetDestination(Target))
//            {
//                break;
//            }
//        }

//        kTime = 5;
//    }

//    // 寻找目标。代理到达目的地后返回success
//    public override TaskStatus OnUpdate()
//    {
//        kTime -= Time.deltaTime;

//        if (HasPath() && isLookTargetPoint)
//        {
//            //转向
//            roleBase.Model.forward = Vector3.Slerp(roleBase.Model.forward, navMeshAgent.velocity, rotaSpeed * Time.deltaTime);
//        }

//        if (HasArrived() || kTime <= 0)
//        {
//            roleBase.SetFloatMark(FloatUseEnum.巡逻待机, Random.Range(this.randomTimeLimit.x, this.randomTimeLimit.y));
//            roleBase.SetBoolMark(BoolUseEnum.IsPatrolIdle, true);

//            return TaskStatus.Success;
//        }

//        return TaskStatus.Running;
//    }

//    /// <summary>
//    /// 任务结束,停止运动。
//    /// </summary>
//    public override void OnEnd()
//    {
//        Stop();
//        UpdateRotation(false);
//    }

//    public override void OnBehaviorComplete()
//    {
//        Stop();
//    }

//    /// <summary>
//    /// 设置目标点
//    /// </summary>
//    /// <param name="destination"></param>
//    /// <returns></returns>
//    protected bool SetDestination(Vector3 destination)
//    {
//        navMeshAgent.isStopped = false;
//        return navMeshAgent.SetDestination(destination);
//    }

//    /// <summary>
//    /// 是否更新旋转
//    /// </summary>
//    /// <param name="update"></param>
//    protected void UpdateRotation(bool update)
//    {
//        navMeshAgent.updateRotation = update;
//    }

//    /// <summary>
//    /// 是否有路径
//    /// </summary>
//    protected bool HasPath()
//    {
//        return navMeshAgent.hasPath && navMeshAgent.remainingDistance > ProjectSettingInfo.navMeshAgentRemainingDistance;
//    }

//    /// <summary>
//    /// 是否到达目标点
//    /// </summary>
//    protected bool HasArrived()
//    {
//        //如果路径处于挂起状态，则还没有计算该路径。
//        float remainingDistance;
//        if (navMeshAgent.pathPending)
//        {
//            remainingDistance = float.PositiveInfinity;
//        }
//        else
//        {
//            //剩下的距离
//            remainingDistance = navMeshAgent.remainingDistance;
//        }

//        return remainingDistance <= ProjectSettingInfo.navMeshAgentRemainingDistance;
//    }

//    /// <summary>
//    /// 停止寻路。
//    /// </summary>
//    protected void Stop()
//    {
//        UpdateRotation(false);
//        if (navMeshAgent.hasPath)
//        {
//            navMeshAgent.isStopped = true;
//        }
//    }
//}