//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "music.jpeg")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("播放音乐Action")]
///// <summary>
///// 使用当前技能
///// </summary>
//public class PlayMusic : Action
//{
//    /// <summary>
//    /// 自身角色脚本
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// 资源名字
//    /// </summary>
//    public string name;

//    /// <summary>
//    /// loop 值
//    /// </summary>
//    public bool loop = false;

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        if (name == null || name == "") return TaskStatus.Failure;
//        roleBase.PlayAudio(name,loop);
//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
