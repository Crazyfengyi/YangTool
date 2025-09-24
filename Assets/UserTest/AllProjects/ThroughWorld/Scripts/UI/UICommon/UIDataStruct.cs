/* 
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
using YangTools.Scripts.Core.YangUGUI;

/// <summary>
/// 通用二级弹窗参数
/// </summary>
public class ConfirmData : UGUIDataBase
{
    /// <summary>
    /// 显示的文字
    /// </summary>
    public string showMsg;
    /// <summary>
    /// ok按钮文字
    /// </summary>
    public string okBtnText;
    /// <summary>
    /// 关闭按钮文字
    /// </summary>
    public string cancelBtnText;

    /// <summary>
    /// 是否倒计时自动选择
    /// </summary>
    public bool isCountDownSelect = false;
    /// <summary>
    /// 倒计时
    /// </summary>
    public float countDownTime;
    /// <summary>
    /// 自动选择OK按钮
    /// </summary>
    public bool autoSelectOk;

    /// <summary>
    /// OK按钮回调
    /// </summary>
    public Action okCallBack;
    /// <summary>
    /// Cancel按钮回调
    /// </summary>
    public Action cancelCallBack;
}