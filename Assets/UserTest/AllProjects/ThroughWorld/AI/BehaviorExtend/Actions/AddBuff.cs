//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "攻击.jpeg")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("加Buff Action")]
///// <summary>
///// 使用当前技能
///// </summary>
//public class AddBuff : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// 资源名字
//    /// </summary>
//    public BuffID buffId;

//    /// <summary>
//    /// 触发几率
//    /// </summary>
//    public int percent = 50;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        if(Random.Range(1, 101) <= percent)
//        {
//            foreach (RoleBase rb in this.roleBase.beAttackedList)
//            {
//                rb.AddBuff(buffId);
//            }
//        }

//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
