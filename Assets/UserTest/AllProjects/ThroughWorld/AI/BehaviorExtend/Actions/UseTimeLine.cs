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
//using UnityEngine.Playables;

//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("使用技能(TimeLine)")]
//[TaskIcon(BehaviorSetting.path + "TimeLine.png")]
//public class UseTimeLine : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// 计时
//    /// </summary>
//    private float time;
//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {
//        //技能树资源名称
//        string playableName = roleBase.GetCurrentSkill().SkillData.actionAssetsName;
//        PlayableAsset playableAsset = AAResMgr.Ins.LoadAsset<PlayableAsset>(playableName);

//        time = 0;
//        roleBase.playableDirector.playableAsset = playableAsset;
//        roleBase.playableDirector.enabled = true;
//        roleBase.playableDirector.Play();
//    }

//    public override TaskStatus OnUpdate()
//    {
//        time += Time.deltaTime;
//        if (time >= roleBase.playableDirector.duration)
//        {
//            return TaskStatus.Success;
//        }

//        return TaskStatus.Running;
//    }

//    public override void OnEnd()
//    {
//        roleBase.playableDirector.playableAsset = null;
//        roleBase.playableDirector.enabled = false;
//    }
//}