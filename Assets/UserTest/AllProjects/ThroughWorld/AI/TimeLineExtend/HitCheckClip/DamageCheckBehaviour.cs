/** 
 *Copyright(C) 2020 by XCHANGE 
 *All rights reserved. 
 *Author:       YangWork 
 *UnityVersion：2020.3.7f1c1 
 *创建时间:         2021-05-28 
*/
using UnityEngine;
using UnityEngine.Playables;
using System;
using Sirenix.OdinInspector;
using DataStruct;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using UnityEngine.Tilemaps;

/// <summary>
/// 伤害检测
/// </summary>
[Serializable]
public class DamageCheckBehaviour : AutoBehaviour
{
    [HideInInspector]
    public RoleBase bindRole;
    [LabelText("检测模式")]
    public TimeLineCheckType timeLineCheckType;
    [HideIf("@(timeLineCheckType != TimeLineCheckType.间隔伤害)")]
    [LabelText("间隔时间")]
    public float intervalTime;
    [LabelText("伤害类型")]
    public DamageType damageType;
    [LabelText("检测信息")]
    public CheckConfig checkConfig;
    //[LabelText("命中后自身特效")]
    //public HitEffectInfo selfShowEffect;
    //[LabelText("命中后目标特效")]
    //public HitEffectInfo targetShowEffect;
    //[LabelText("BUFF触发信息")]
    //public BuffTriggerInfo buffTriggerInfo;

    private float time;//记时
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        time = 0;
    }
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (isTrigger == false && info.weight == 1)
        {
            isTrigger = true;
            HitCheck();
        }

        if (info.weight == 1 && timeLineCheckType == TimeLineCheckType.间隔伤害)
        {
            time += info.deltaTime;
            if (time >= intervalTime)
            {
                time = 0;
                HitCheck();
            }
        }
    }
    /// <summary>
    /// 伤害检查
    /// </summary>
    public void HitCheck()
    {
        if (bindRole == null) return;

        bindRole.DamageCheck(checkConfig);
    }
}
/// <summary>
/// 伤害检测类型
/// </summary>
public enum TimeLineCheckType
{
    单次伤害,
    间隔伤害
}