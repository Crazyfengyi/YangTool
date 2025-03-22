/*
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-04-30 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.Scripts.Core;

public class GameInputManager : MonoSingleton<GameInputManager>
{
    #region 变量
    private GameInputSet gameInput;//TODO:输入管理器
    public GameInputSet GameInput => gameInput;
    #endregion

    #region 生命周期
    protected override void Awake()
    {
        base.Awake();
        gameInput = new GameInputSet();
        EnablePlayer();
    }
    #endregion

    #region 方法
    /// <summary>
    /// 启动玩家输入
    /// </summary>
    public void EnablePlayer()
    {
        gameInput.Player.Enable();
    }
    /// <summary>
    /// 屏蔽玩家输入
    /// </summary>
    public void DisablePlayer()
    {
        gameInput.Player.Disable();
    }
    #endregion
}