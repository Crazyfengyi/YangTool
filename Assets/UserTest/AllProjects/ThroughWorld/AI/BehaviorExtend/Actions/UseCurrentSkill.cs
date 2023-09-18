//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("使用当前技能")]
///// <summary>
///// 使用当前技能
///// </summary>
//public class UseCurrentSkill : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;
//    /// <summary>
//    /// 是否立即进入能却
//    /// </summary>
//    public bool isImmediatelyCooling;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        roleBase.UseCurrentSkill(isImmediatelyCooling);

//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
