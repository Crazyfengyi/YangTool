/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-05-11 
*/
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Pathfinding;
using cfg.monster;
using DataStruct;
using Sirenix.OdinInspector;
using YangTools.Scripts.Core.YangExtend;

[TaskCategory("RoleAI/Motion")]
public class AIFindTarget : Action
{
    /// <summary>
    /// 自身角色脚本
    /// </summary>
    private Monster monster;
    private AIPath AIPath;
    [LabelText("使用警戒范围")]
    public bool UseGuardRang;

    /// <summary>
    /// 方向
    /// </summary>
    private Vector3 direction;
    private float distance;
    public override void OnAwake()
    {
        monster = gameObject.GetComponent<Monster>();
        AIPath = monster != null ? monster.AIPath : null;
    }

    public override void OnStart()
    {
        if (AIPath == null) return;
        AIPath.isStopped = false;
        if (UseGuardRang)
        {
            distance = monster.GetRoleAttributeValue(RoleAttribute.GuardRang);
        }
        else
        {
            distance = monster.AIFindTargetDistance;
        }
    }

    // 寻找目标。代理到达目的地后返回success
    public override TaskStatus OnUpdate()
    {
        if (monster != null && AstarPath.active != null && AIPath != null && monster.Target)
        {
            distance = UseGuardRang ? monster.GetRoleAttributeValue(RoleAttribute.GuardRang) : monster.AIFindTargetDistance;
            AIPath.destination = monster.Target.transform.position;

            if (Vector3.Distance(transform.position.SetYValue(), monster.Target.transform.position.SetYValue()) <= distance)
            {
                Stop();
                return TaskStatus.Success;
            }
            else
            {
                AIPath.isStopped = false;
            }
            if (monster.Animator != null) monster.Animator.SetFloat("Speed", AIPath.velocity.magnitude);
        }
        else
        {
            return TaskStatus.Failure;
        }

        return TaskStatus.Running;
    }

    /// <summary>
    /// 任务结束,停止运动
    /// </summary>
    public override void OnEnd()
    {
        Stop();
    }

    public override void OnBehaviorComplete()
    {
        Stop();
    }

    /// <summary>
    /// 停止寻路
    /// </summary>
    protected void Stop()
    {
        if (AIPath != null) AIPath.isStopped = true;
    }
}