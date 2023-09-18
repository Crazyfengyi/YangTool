/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-05-28 
*/
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(255f / 255f, 255f / 255f, 100f / 255f)]
[TrackClipType(typeof(AutoClip))]
public class AutoTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<AutoMixer>.Create(graph, inputCount);
    }
}