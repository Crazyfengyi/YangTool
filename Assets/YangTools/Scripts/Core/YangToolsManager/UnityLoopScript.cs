/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-18 
*/
using UnityEngine;
using System.Collections;

/// <summary>
/// Unity生命周期脚本
/// </summary>
public class UnityLoopScript : MonoBehaviour
{
    #region 变量
    #endregion

    #region 生命周期
    /// <summary>
    /// 游戏组件初始化。
    /// </summary>
    private void Awake()
    {
        //Application.targetFrameRate = m_FrameRate;
        //Time.timeScale = m_GameSpeed;
        //Application.runInBackground = m_RunInBackground;
        //Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
    }

    private void Start()
    {
    }
    private void Update()
    {
        YangTools.YangToolsManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
    }
    private void OnApplicationQuit()
    {
        YangTools.YangToolsManager.OnApplicationQuit();
    }

    private void OnDestroy()
    {
    }
    #endregion

    #region 方法
    #endregion
}