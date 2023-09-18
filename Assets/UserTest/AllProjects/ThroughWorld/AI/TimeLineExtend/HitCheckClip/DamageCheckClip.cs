/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-05-28 
*/
using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using Sirenix.OdinInspector;

[LabelText("伤害检测")]
[Serializable]
public class DamageCheckClip : AutoClip, ITimelineClipAsset
{
    public DamageCheckBehaviour data = new DamageCheckBehaviour();
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        ScriptPlayable<DamageCheckBehaviour> playable = ScriptPlayable<DamageCheckBehaviour>.Create(graph, data);
        DamageCheckBehaviour clone = playable.GetBehaviour();
        clone.bindRole = owner.GetComponent<RoleBase>();
        clone.Init();

        return playable;
    }
    ///// <summary>
    ///// 创建clip在轨道上--反射调用
    ///// </summary>
    //public static TimelineClip CreateClip(TrackAsset trackAsset)
    //{
    //    TimelineClip info = trackAsset.CreateClip<DamageCheckClip>();
    //    return info;
    //}
}