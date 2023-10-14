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
using YangTools.Extend;
using Sirenix.OdinInspector;

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
        AIPath = monster.AIPath;
    }

    public override void OnStart()
    {
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
        if (AstarPath.active != null && AIPath != null && monster.Target)
        {
            AIPath.destination = monster.Target.transform.position;

            if (Vector3.Distance(transform.position.SetYValue(), monster.Target.transform.position.SetYValue()) <= distance)
            {
                if (!AIPath.isStopped)
                {
                    Stop();
                    return TaskStatus.Success;
                }
            }
            else
            {
                AIPath.isStopped = false;
            }
            monster.Animator.SetFloat("Speed", AIPath.velocity.magnitude);
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
        AIPath.isStopped = true;
    }
}