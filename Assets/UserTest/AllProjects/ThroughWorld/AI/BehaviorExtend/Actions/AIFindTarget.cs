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
    private static readonly int SpeedParameter = Animator.StringToHash("Speed"); // 移动动画速度参数
    /// <summary>
    /// 自身角色脚本
    /// </summary>
    private Monster monster;
    private AIPath AIPath;
    private Animator movementAnimator; // 配置了移动速度参数的 Animator
    [LabelText("使用警戒范围")]
    public bool UseGuardRang;

    /// <summary>
    /// 方向
    /// </summary>
    private float distance;
    /// <summary>
    /// 缓存怪物组件
    /// </summary>
    public override void OnAwake()
    {
        monster = gameObject.GetComponent<Monster>();
    }

    /// <summary>
    /// 在怪物初始化后读取寻路状态和动画参数
    /// </summary>
    public override void OnStart()
    {
        AIPath = monster != null ? monster.AIPath : null;
        movementAnimator = null;
        if (AIPath == null || !AIPath.isActiveAndEnabled || monster.Target == null || AstarPath.active == null) return;
        AIPath.destination = monster.Target.transform.position;
        var animator = monster.Animator;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (var parameter in animator.parameters)
            {
                if (parameter.nameHash != SpeedParameter || parameter.type != AnimatorControllerParameterType.Float) continue;
                movementAnimator = animator;
                break;
            }
        }
        if (UseGuardRang)
        {
            distance = monster.GetRoleAttributeValue(RoleAttribute.GuardRang);
        }
        else
        {
            distance = monster.AIFindTargetDistance;
        }
    }

    /// <summary>
    /// 追踪目标并在达到水平距离后结束
    /// </summary>
    public override TaskStatus OnUpdate()
    {
        if (monster != null && AstarPath.active != null && AIPath != null && AIPath.isActiveAndEnabled && monster.Target)
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
            SetAnimationSpeed(AIPath.velocity.magnitude);
        }
        else
        {
            Stop();
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

    /// <summary>
    /// 行为树关闭时收尾
    /// </summary>
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
        SetAnimationSpeed(0);
    }

    /// <summary>
    /// 只更新配置了速度参数的角色动画
    /// </summary>
    private void SetAnimationSpeed(float speed)
    {
        if (movementAnimator != null) movementAnimator.SetFloat(SpeedParameter, speed);
    }
}
