///** 
// *Copyright(C) 2020 by XCHANGE 
// *All rights reserved. 
// *Author:       YangWork 
// *UnityVersion：2020.3.7f1c1 
// *创建时间:         2021-06-30 
//*/
//using UnityEngine;
//using System.Collections;
//using BehaviorDesigner.Runtime.Tasks;
//using BehaviorDesigner.Runtime;

//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("开始执行行为树")]
//[TaskIcon("{SkinColor}StartBehaviorTreeIcon.png")]
//public class StartSkillBehaviorTree : Action
//{
//    public SharedInt group;
//    public SharedBool waitForCompletion = false;
//    //是否完成
//    private bool behaviorComplete;
//    private Behavior behavior;
//    private RoleBase roleBase;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }
//    public override void OnStart()
//    {
//        Behavior[] behaviorTrees = roleBase.GetComponents<Behavior>();

//        for (int i = 0; i < behaviorTrees.Length; ++i)
//        {
//            if (behaviorTrees[i].Group == group.Value)
//            {
//                behavior = behaviorTrees[i];
//                break;
//            }
//        }

//        if (behavior != null)
//        {
//            //技能树资源名称
//            string treeName = roleBase.GetCurrentSkill().SkillData.actionAssetsName;
//            ExternalBehavior tree = AAResMgr.Ins.LoadAsset<ExternalBehavior>(treeName);
//            behavior.ExternalBehavior = tree;
//            behavior.EnableBehavior();

//            if (waitForCompletion.Value)
//            {
//                behaviorComplete = false;
//                behavior.OnBehaviorEnd += BehaviorEnded;
//            }
//        }
//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (behavior == null)
//        {
//            return TaskStatus.Failure;
//        }
//        //等待行为树执行完成
//        if (waitForCompletion.Value && !behaviorComplete)
//        {
//            return TaskStatus.Running;
//        }

//        return TaskStatus.Success;
//    }

//    private void BehaviorEnded(Behavior behavior)
//    {
//        behaviorComplete = true;
//    }

//    public override void OnEnd()
//    {
//        if (behavior != null && waitForCompletion.Value)
//        {
//            behavior.OnBehaviorEnd -= BehaviorEnded;
//        }
//    }
//}