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
using YangTools.Scripts.Core.YangExtend;

[TaskCategory("RoleAI/Motion")]
public class LookTarget : Action
{
    /// <summary>
    /// 自身角色脚本
    /// </summary>
    private RoleBase roleBase;
    /// <summary>
    /// 转向类型
    /// </summary>
    public LookType lookType;
    /// <summary>
    /// 转身速度
    /// </summary>
    public float speed = 10f;
    /// <summary>
    /// 开始方向
    /// </summary>
    private Vector3 startDirection;
    /// <summary>
    /// 目标方向
    /// </summary>
    private Vector3 targetDirection;

    public override void OnAwake()
    {
        roleBase = gameObject.GetComponent<RoleBase>();
    }

    public override void OnStart()
    {
        startDirection = roleBase.ModelInfo.Root.forward;

        if (roleBase.Target)
        {
            targetDirection = (roleBase.Target.transform.position - gameObject.transform.position).SetYValue(gameObject.transform.position.y).normalized;
        }
        else
        {
            targetDirection = roleBase.ModelInfo.Root.forward;
        }
    }

    public override TaskStatus OnUpdate()
    {
        switch (lookType)
        {
            case LookType.Lerp:
                {
                    roleBase.ModelInfo.Root.forward = Vector3.RotateTowards(roleBase.ModelInfo.Root.forward, targetDirection, speed * Time.deltaTime, 0f);
                    //roleBase.ModelInfo.Root.rotation = Quaternion.LookRotation(newDirection);
                    //转向到小于1度时
                    if (Vector3.Angle(roleBase.ModelInfo.Root.forward, targetDirection) < 1F)
                    {
                        return TaskStatus.Success;
                    }
                }
                break;
            case LookType.Immediate:
                roleBase.ModelInfo.Root.LookAt(targetDirection);
                return TaskStatus.Success;
            default:
                break;
        }

        return TaskStatus.Running;
    }
}

public enum LookType
{
    /// <summary>
    /// 缓动
    /// </summary>
    Lerp,
    /// <summary>
    /// 立刻
    /// </summary>
    Immediate,
}