//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "music.jpeg")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("播放背景音乐Action")]
///// <summary>
///// 使用当前技能
///// </summary>
//public class PlayBGMusic : Action
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
//    ///  是添加还是移除
//    /// </summary>
//    public bool isAdd = true;

//    private static Dictionary<string,int>  bgMgr = new Dictionary<string,int>(); //背景音效管理

//    public override void OnAwake()
//    {
//        roleBase = gameObject.GetComponent<RoleBase>();
//    }

//    public override void OnStart()
//    {

//    }

//    public override TaskStatus OnUpdate()
//    {
//        //if (name == null || name == "") return TaskStatus.Failure;
//        //MusicAudioPlayer map = AudioMgr.Ins.GetAudioPlayer<MusicAudioPlayer>();
//        //if (isAdd) //添加
//        //{
//        //    if(bgMgr.ContainsKey(name))
//        //    {
//        //        bgMgr[name] += 1;
//        //    }
//        //    else
//        //    {
//        //        bgMgr[name] = 1;
//        //    }

//        //    if(name == "sfx_bgm_forest_battle_loop" && map.audioState.Name == "sfx_bgm_forest_boss_loop") //小怪背景,如果当前是大怪，则不覆盖
//        //    {
//        //        return TaskStatus.Success;
//        //    }

//        //    AudioMgr.Ins.Play<MusicAudioPlayer>(name, true, null);
//        //}
//        //else //移除
//        //{
//        //    if (bgMgr.ContainsKey(name))
//        //    {
//        //        bgMgr[name] -= 1;
//        //    }
//        //    else
//        //    {
//        //        bgMgr[name] = 0;
//        //    }

//        //    if (name == "sfx_bgm_forest_battle_loop") //小怪
//        //    {
//        //        if (bgMgr[name] == 0 && map.audioState.Name == name) //播放普通场景音效
//        //        {
//        //            AudioMgr.Ins.Play<MusicAudioPlayer>("sfx_bgm_forest_idle_loop", true, null);
//        //        }
//        //    }
//        //    else if(name == "sfx_bgm_forest_boss_loop") //Boss音效
//        //    {
//        //        if(bgMgr.ContainsKey("sfx_bgm_forest_battle_loop") && bgMgr["sfx_bgm_forest_battle_loop"]>0) //播放小怪背景音乐
//        //        {
//        //            AudioMgr.Ins.Play<MusicAudioPlayer>("sfx_bgm_forest_battle_loop", true, null);
//        //        }
//        //        else
//        //        {
//        //            AudioMgr.Ins.Play<MusicAudioPlayer>("sfx_bgm_forest_idle_loop", true, null);//播放普通背景音乐
//        //        }
//        //    }
//        //}
        
//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
