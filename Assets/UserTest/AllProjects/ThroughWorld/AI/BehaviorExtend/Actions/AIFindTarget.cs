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

[TaskCategory("RoleAI/Motion")]
public class AIFindTarget : Action
{
    /// <summary>
    /// 自身角色脚本
    /// </summary>
    private Monster monster;
    private AIPath AIPath;

    /// <summary>
    /// 方向
    /// </summary>
    private Vector3 direction;

    public override void OnAwake()
    {
        monster = gameObject.GetComponent<Monster>();
        AIPath = monster.AIPath;
    }

    public override void OnStart()
    {
        AIPath.isStopped = false;
    }

    // 寻找目标。代理到达目的地后返回success
    public override TaskStatus OnUpdate()
    {
        if (AstarPath.active != null && AIPath != null && monster.Target)
        {
            AIPath.destination = monster.Target.transform.position;

            if (Vector3.Distance(transform.position, monster.Target.transform.position) < monster.GetRoleAttribute(RoleAttribute.AtkRang))
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