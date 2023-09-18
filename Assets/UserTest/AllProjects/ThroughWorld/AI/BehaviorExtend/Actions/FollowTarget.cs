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
//using UnityEngine.AI;

//[TaskCategory("RoleAI/Motion")]
//public class FollowTarget : Action
//{
//    protected NavMeshAgent navMeshAgent;
//    /// <summary>
//    /// 是否看向目标点
//    /// </summary>
//    public bool isLookTargetPoint = true;
//    /// <summary>
//    /// 转向速度
//    /// </summary>
//    public float rotaSpeed = 30f;
//    /// <summary>
//    /// 是否时间退出
//    /// </summary>
//    public bool isTimeOut;
//    /// <summary>
//    /// 目标时间
//    /// </summary>
//    public float targetTime = 2f;
//    /// <summary>
//    /// 记时
//    /// </summary>
//    private float time;
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 方向
//    /// </summary>
//    private Vector3 direction;
//    private Vector3 TargetPos
//    {
//        get
//        {
//            if (roleBase.Target)
//            {
//               // navMeshAgent.stoppingDistance = roleBase.rolePropertyInfo.GetProperty(RolePropertyType.AttackRange).CurrentValue;
//                return roleBase.Target.transform.position;
//            }

//            //容错找不到寻路目标--就原地不动
//            return roleBase.transform.position;
//        }
//    }
//    public override void OnAwake()
//    {
//        //roleBase = gameObject.GetComponent<RoleBase>();

//        //if (roleBase.GetComponent<MonsterBase>() != null)
//        //{
//        //    navMeshAgent = roleBase.GetComponent<MonsterBase>().navMeshAgent;
//        //}
//        //else
//        //{
//        //    navMeshAgent = roleBase.GetComponent<PetBase>().navMeshAgent;
//        //}
//    }

//    public override void OnStart()
//    {
//        time = 0;
//        UpdateRotation(false);
//        SetDestination(TargetPos);
//    }

//    // 寻找目标。代理到达目的地后返回success
//    public override TaskStatus OnUpdate()
//    {
//        if (Vector3.Distance(TargetPos, roleBase.transform.position) < 0.01f)
//        {
//            direction = (TargetPos - roleBase.transform.position).normalized;

//            if (direction != Vector3.zero)
//            {
//                //roleBase.modelInfo.forward = direction;
//            }

//            return TaskStatus.Success;
//        }

//        if (Time.frameCount % 5 == 0)
//        {
//            SetDestination(TargetPos);
//        }

//        if (HasPath() && isLookTargetPoint)
//        {
//            Vector3 targetRota = roleBase.modelInfo.Root.forward;

//            if (navMeshAgent.velocity != Vector3.zero)
//            {
//                targetRota = navMeshAgent.velocity;
//            }

//            //转向
//            roleBase.modelInfo.Root.forward = Vector3.Slerp(roleBase.Model.forward, targetRota, rotaSpeed * Time.deltaTime);
//        }

//        if (HasArrived() || (isTimeOut && time >= targetTime))
//        {
//            direction = (TargetPos - roleBase.transform.position).normalized;

//            if (direction == Vector3.zero)
//            {
//                direction = roleBase.modelInfo.Root.forward;
//            }

//            roleBase.modelInfo.Root.forward = direction;
//            return TaskStatus.Success;
//        }

//        time += Time.deltaTime;
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

//        if (navMeshAgent.isOnNavMesh == false)
//        {
//            //Debuger.ToError($"角色:{roleBase.transform.name}的寻路出现问题了");
//        }

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
//        return navMeshAgent.hasPath;
//    }

//    /// <summary>
//    /// 寻路速度
//    /// </summary>
//    protected Vector3 Velocity()
//    {
//        return navMeshAgent.velocity;
//    }

//    /// <summary>
//    /// 如果该位置是有效的寻径位置，则返回true。
//    /// </summary>
//    protected bool SamplePosition(Vector3 position)
//    {
//        NavMeshHit hit;
//        return NavMesh.SamplePosition(position, out hit, float.MaxValue, NavMesh.AllAreas);
//    }
//    /// <summary>
//    /// 是否到达目标点
//    /// </summary>
//    protected bool HasArrived()
//    {
//        // 如果路径处于挂起状态，则还没有计算该路径。
//        float remainingDistance;
//        if (navMeshAgent.pathPending)
//        {
//            remainingDistance = float.PositiveInfinity;
//        }
//        else
//        {
//            remainingDistance = navMeshAgent.remainingDistance;
//        }

//        return remainingDistance <= navMeshAgent.stoppingDistance;
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