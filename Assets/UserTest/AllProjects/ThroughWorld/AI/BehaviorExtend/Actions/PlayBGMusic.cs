//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;
//using DataStruct;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using YangToolDebuger;

//[TaskIcon(BehaviorSetting.path + "music.jpeg")]
//[TaskCategory("RoleAI/Motion")]
//[TaskDescription("���ű�������Action")]
///// <summary>
///// ʹ�õ�ǰ����
///// </summary>
//public class PlayBGMusic : Action
//{
//    /// <summary>
//    /// �����ɫ�ű�
//    /// </summary>
//    private RoleBase roleBase;

//    /// <summary>
//    /// ��Դ����
//    /// </summary>
//    public string name;

//    /// <summary>
//    ///  ����ӻ����Ƴ�
//    /// </summary>
//    public bool isAdd = true;

//    private static Dictionary<string,int>  bgMgr = new Dictionary<string,int>(); //������Ч����

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
//        //if (isAdd) //���
//        //{
//        //    if(bgMgr.ContainsKey(name))
//        //    {
//        //        bgMgr[name] += 1;
//        //    }
//        //    else
//        //    {
//        //        bgMgr[name] = 1;
//        //    }

//        //    if(name == "sfx_bgm_forest_battle_loop" && map.audioState.Name == "sfx_bgm_forest_boss_loop") //С�ֱ���,�����ǰ�Ǵ�֣��򲻸���
//        //    {
//        //        return TaskStatus.Success;
//        //    }

//        //    AudioMgr.Ins.Play<MusicAudioPlayer>(name, true, null);
//        //}
//        //else //�Ƴ�
//        //{
//        //    if (bgMgr.ContainsKey(name))
//        //    {
//        //        bgMgr[name] -= 1;
//        //    }
//        //    else
//        //    {
//        //        bgMgr[name] = 0;
//        //    }

//        //    if (name == "sfx_bgm_forest_battle_loop") //С��
//        //    {
//        //        if (bgMgr[name] == 0 && map.audioState.Name == name) //������ͨ������Ч
//        //        {
//        //            AudioMgr.Ins.Play<MusicAudioPlayer>("sfx_bgm_forest_idle_loop", true, null);
//        //        }
//        //    }
//        //    else if(name == "sfx_bgm_forest_boss_loop") //Boss��Ч
//        //    {
//        //        if(bgMgr.ContainsKey("sfx_bgm_forest_battle_loop") && bgMgr["sfx_bgm_forest_battle_loop"]>0) //����С�ֱ�������
//        //        {
//        //            AudioMgr.Ins.Play<MusicAudioPlayer>("sfx_bgm_forest_battle_loop", true, null);
//        //        }
//        //        else
//        //        {
//        //            AudioMgr.Ins.Play<MusicAudioPlayer>("sfx_bgm_forest_idle_loop", true, null);//������ͨ��������
//        //        }
//        //    }
//        //}
        
//        return TaskStatus.Success;
//    }

//    public override void OnEnd()
//    {

//    }
//}
