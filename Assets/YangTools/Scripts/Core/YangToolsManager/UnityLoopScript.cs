/*
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-18 
*/

using UnityEngine;
using YangTools.Scripts.Core.YangCoroutine;

namespace YangTools.Scripts.Core.YangToolsManager
{
    /// <summary>
    /// Unity生命周期脚本
    /// </summary>
    public class UnityLoopScript : MonoBehaviour
    {
        #region 生命周期
        private void Update()
        {
            Scripts.Core.YangToolsManager.YangToolsManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
            YangCoroutineManager.Instance.UpdateCoroutine();
        }
        private void OnApplicationQuit()
        {
            Scripts.Core.YangToolsManager.YangToolsManager.OnApplicationQuit();
        }
        #endregion
    }
}
