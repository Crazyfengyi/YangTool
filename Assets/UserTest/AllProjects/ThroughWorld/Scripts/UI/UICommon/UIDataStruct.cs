/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-02-04 
*/
using System;
using System.Collections;
using UnityEngine;
using YangTools;
using YangTools.UGUI;

/// <summary>
/// 通用二级弹窗参数
/// </summary>
public class ConfirmData : UGUIDataBase
{
    /// <summary>
    /// 显示的文字
    /// </summary>
    public string showStr;
    /// <summary>
    /// OK按钮回调
    /// </summary>
    public Action okCallBack;
    /// <summary>
    /// Cancel按钮回调
    /// </summary>
    public Action cancelCallBack;
}