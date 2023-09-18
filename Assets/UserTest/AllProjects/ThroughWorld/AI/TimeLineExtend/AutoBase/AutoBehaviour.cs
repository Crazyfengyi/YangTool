/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-06-04 
*/
using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;

/// <summary>
/// 不要直接使用基类
/// </summary>
[Serializable]
public class AutoBehaviour : PlayableBehaviour
{
    //是否触发
    protected bool isTrigger;
    public virtual void Init()
    {
        
    }
    //TimeLine开始时执行
    public override void OnGraphStart(Playable playable)
    {
    }
    //TimeLine停止时执行
    public override void OnGraphStop(Playable playable)
    {
    }
    //进入区域时(或者编辑器下在区域内恢复(暂停))
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        isTrigger = false;
    }
    //离开区域时(或者编辑器下在区域内暂停)
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        isTrigger = false;
    }
}