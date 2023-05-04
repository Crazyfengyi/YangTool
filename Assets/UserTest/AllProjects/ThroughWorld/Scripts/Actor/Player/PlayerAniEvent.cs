/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-04-24 
*/  
using System;
using System.Collections;  
using UnityEngine;  
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
  
public class PlayerAniEvent : MonoBehaviour 
{
	#region 方法
    /// <summary>
    /// 右腿落地
    /// </summary>
	public void FootR()
	{
        GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
    }
    /// <summary>
    /// 左腿落地
    /// </summary>
    public void FootL()
    {
        GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
    }
    /// <summary>
    /// 跳跃落地
    /// </summary>
    public void Land()
    {
        GameSoundManager.Instance.PlaySound("Audio_BulletAtk");
    }
    #endregion
} 