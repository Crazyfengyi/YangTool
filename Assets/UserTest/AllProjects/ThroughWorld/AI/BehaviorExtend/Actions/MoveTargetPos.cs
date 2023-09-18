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
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "Move.png")]
//[TaskCategory("RoleAI/Motion")]
//public class MoveTargetPos : Action
//{
//    //寻路组件
//    protected NavMeshAgent navMeshAgent;
//    /// <summary>
//    /// 移动类型
//    /// </summary>
//    public MoveType moveType;
//    /// <summary>
//    /// 是否看向目标点
//    /// </summary>
//    public bool isLookTargetPoint = true;
//    /// <summary>
//    /// float值
//    /// </summary>
//    public float floatValue = 1;
//    /// <summary>
//    /// 转向速度
//    /// </summary>
//    public float rotaSpeed = 10f;
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    private Vector3 Target
//    {
//        get
//        {
//            navMeshAgent.stoppingDistance = 0;
//            switch (moveType)
//            {
//                case MoveType.根据攻击距离远离或靠近目标:
//                    if (roleBase.GetTarget())
//                    {
//                        //默认点为自身点
//                        Vector3 resultPos = roleBase.transform.position;
//                        Vector3 selfPos = roleBase.transform.position;
//                        Vector3 enumPos = roleBase.GetTarget().transform.position;

//                        float distance = Vector3.Distance(selfPos, enumPos);
//                        if (distance < roleBase.rolePropertyInfo.GetProperty(RolePropertyType.AttackRange).CurrentValue)
//                        {
//                            ////方向
//                            //var direction = selfPos - enumPos;
//                            ////距离
//                            //float distance = Random.Range(2, 4);
//                            //resultPos = enumPos + direction + direction.normalized * distance + new Vector3(Random.Range(2, 4), 0, Random.Range(2, 4));
//                            //return resultPos;

//                            //TODO 测试 目标圆环的点
//                            //Dictionary<bool, List<Gridinfo>> dic = RoomsMgr.Ins.GetCustomCircleGridinfo(roleBase.GetTarget().transform.position, 10, 5);
//                            //if (dic[true].Count > 0)
//                            //{
//                            //    Gridinfo target = dic[true][Random.Range(0, dic[true].Count)];
//                            //    return new Vector3(target.Pos.x, 0, target.Pos.z);
//                            //}
//                            //else if (dic[false].Count > 0)
//                            //{
//                            //    Gridinfo target = dic[false][Random.Range(0, dic[true].Count)];
//                            //    return new Vector3(target.Pos.x, 0, target.Pos.z);
//                            //}
//                            //TODO:临时的
//                            Vector2 center = new Vector2(selfPos.x, selfPos.z);
//                            Vector2 result = Extends.GetRandomPointInCircle(center, 6);
//                            return new Vector3(result.x, 0, result.y);
//                        }
//                        else
//                        {
//                            Vector2 center = new Vector2(selfPos.x, selfPos.z);
//                            Vector2 result = Extends.GetRandomPointInCircle(center, 6);
//                            return new Vector3(result.x, 0, result.y);
//                        }
//                    }
//                    break;
//                case MoveType.记录的攻击点:
//                    return roleBase.atkPoint;
//                case MoveType.记录点:
//                    return roleBase.recordPointList[0];
//                case MoveType.球形范围随机移动:
//                    //起点
//                    Vector3 startPos = roleBase.transform.position;
//                    Vector2 rang = Random.insideUnitCircle * floatValue;
//                    startPos.x += rang.x;
//                    startPos.z += rang.y;
//                    return startPos;
//                case MoveType.移动到初始点:
//                    return roleBase.initialPoint;
//                default:
//                    break;
//            }
//            //容错找不到寻路目标--就原地不动
//            return roleBase.transform.position;
//        }
//    }

//    #region 行为树生命周期
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//        navMeshAgent = roleBase.GetComponent<MonsterBase>().navMeshAgent;
//    }
//    public override void OnStart()
//    {
//        UpdateRotation(false);
//        SetDestination(Target);
//    }
//    public override TaskStatus OnUpdate()
//    {
//        if (navMeshAgent.hasPath && isLookTargetPoint)
//        {
//            Vector3 targetRota = roleBase.Model.forward;
//            if (navMeshAgent.velocity != Vector3.zero)
//            {
//                targetRota = navMeshAgent.velocity;
//            }

//            //转向
//            roleBase.Model.forward = Vector3.Slerp(roleBase.Model.forward, targetRota, rotaSpeed * Time.deltaTime);
//        }

//        //是否到达目标点
//        if (NavMeshHelp.CheckIsGotoTargetPos(navMeshAgent))
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Running;
//    }
//    public override void OnEnd()
//    {
//        Stop();
//        UpdateRotation(false);
//    }
//    public override void OnBehaviorComplete()
//    {
//        Stop();
//    }
//    #endregion

//    /// <summary>
//    /// 设置目标点
//    /// </summary>
//    /// <param name="destination"></param>
//    /// <returns></returns>
//    protected bool SetDestination(Vector3 destination)
//    {
//        if (navMeshAgent.isOnNavMesh == false)
//        {
//            Debuger.ToError($"角色:{roleBase.transform.name}的寻路组件没有在NavMesh上");
//        }

//        navMeshAgent.isStopped = false;
//        return navMeshAgent.SetDestination(destination);
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
//    /// <summary>
//    /// 是否更新旋转
//    /// </summary>
//    /// <param name="update"></param>
//    protected void UpdateRotation(bool update)
//    {
//        navMeshAgent.updateRotation = update;
//    }
//}

//public enum MoveType
//{
//    None,
//    根据攻击距离远离或靠近目标,
//    记录的攻击点,
//    记录点,
//    球形范围随机移动,
//    移动到初始点,
//}