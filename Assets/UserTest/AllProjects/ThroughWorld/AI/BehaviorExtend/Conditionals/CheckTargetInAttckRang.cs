//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon("Assets/0.Game/_Assets/CoreScript/Behavior_Extend/Gizmos/IsOkIcon.png")]
//[TaskCategory("RoleAI/CheckCondition")]
///// <summary>
///// 检测是否有目标
///// </summary>
//public class CheckTargetInAttckRang : Conditional
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        //Debuger.ToError($"是否在攻击范围内:{roleBase.CheckTargetInAttackRange()}");

//        if (roleBase.CheckTargetInAttackRange())
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Failure;
//    }

//    public override void OnEnd()
//    {

//    }
//}
