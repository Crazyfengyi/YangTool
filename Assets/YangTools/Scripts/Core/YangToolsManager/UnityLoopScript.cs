/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-18 
*/
using UnityEngine;
using System.Collections;

namespace YangTools
{
    /// <summary>
    /// Unity生命周期脚本
    /// </summary>
    public class UnityLoopScript : MonoBehaviour
    {
        #region 生命周期
        /// <summary>
        /// 游戏组件初始化。
        /// </summary>
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            YangTools.YangToolsManager.Update(Time.deltaTime, Time.unscaledDeltaTime);
            YangCoroutineManager.Instance.Updatecorountine();
        }
        private void OnDestroy()
        {
        }
        private void OnApplicationQuit()
        {
            YangTools.YangToolsManager.OnApplicationQuit();
        }
        #endregion
    }
}
