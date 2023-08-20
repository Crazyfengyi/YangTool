/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2022.3.0f1c1 
 *创建时间:         2023-08-20 
*/  
using System;
using System.Collections;  
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using DG.Tweening;
/// <summary>
/// 动画控制器
/// </summary>
public class AniControl 
{
    private Animator animator;

    public AniControl(Animator _animation)
    {
        animator = _animation;
    }
    /// <summary>
    /// 播放
    /// </summary>
    public void Play(string name)
    {
        animator.Play(name);
    }
    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="name"></param>
    /// <param name="layer"></param>
    /// <param name="normalizedTime"></param>
    public void Play(string name, int layer, float normalizedTime)
    {
        animator.Play(name, layer, normalizedTime);
    }
    /// <summary>
    /// 开关
    /// </summary>
    public void Enabled(bool isEnable)
    {
        animator.enabled = isEnable;
    }
} 