/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

using System;
using UnityEngine;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// UI页面管理类--框架自动生成挂载到UI
    /// </summary>
    public class UIPanel : MonoBehaviour, IUIPanel 
    {
        #region 对外属性

        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId { get; private set; }

        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName { get; private set; }

        /// <summary>
        /// 获取界面实例
        /// </summary>
        public object Handle { get; set; }

        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        public IUIGroup UIGroup { get; private set; }

        /// <summary>
        /// 获取界面深度
        /// </summary>
        public int DepthInUIGroup { get; private set; }

        /// <summary>
        /// 是否打开中
        /// </summary>
        public bool IsOpening { get; private set; }

        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        public bool PauseCoveredUIPanel { get; private set; }

        /// <summary>
        /// 获取界面逻辑类
        /// </summary>
        public IUGUIPanel UGUIPanel { get; private set; }

        #endregion 对外属性

        #region 生命周期

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="panelAssetName">界面资源名称</param>
        /// <param name="group">界面所处的界面组</param>
        /// <param name="pauseCoveredPanel">是否暂停被覆盖的界面</param>
        /// <param name="isNewInstance">是否是新实例</param>
        /// <param name="userData">用户自定义数据</param>
        public void OnInit(int serialId, string panelAssetName, IUIGroup group, bool pauseCoveredPanel, bool isNewInstance, object userData)
        {
            this.SerialId = serialId;
            this.UIPanelAssetName = panelAssetName;
            this.UIGroup = group;
            this.DepthInUIGroup = 0;
            this.PauseCoveredUIPanel = pauseCoveredPanel;

            if (!isNewInstance)
            {
                return;
            }

            UGUIPanel = GetComponent<IUGUIPanel>();
            if (UGUIPanel == null)
            {
                Debug.LogError(string.Format("UI form '{0}' can not get UI form logic.", panelAssetName));
                return;
            }

            try
            {
                UGUIPanel.OnInit(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面初始化异常---ID:{serialId},名称:{panelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面打开
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public void OnOpen(object userData)
        {
            try
            {
                UGUIPanel.OnOpen(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面打开异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
            IsOpening = true;
        }

        /// <summary>
        /// 界面关闭
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        public void OnClose(bool isShutdown, object userData)
        {
            try
            {
                UGUIPanel.OnClose(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面关闭异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
            IsOpening = false;
        }

        /// <summary>
        /// 界面暂停
        /// </summary>
        public void OnPause()
        {
            try
            {
                UGUIPanel.OnPause();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面暂停异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面暂停恢复
        /// </summary>
        public void OnResume()
        {
            try
            {
                UGUIPanel.OnResume();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面暂停恢复异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面遮挡
        /// </summary>
        public void OnLostFocus()
        {
            try
            {
                UGUIPanel.OnLostFocus(); 
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面遮挡异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
        }
        /// <summary>
        /// 界面遮挡恢复
        /// </summary>
        public void OnReFocus()
        {
            try
            {
                UGUIPanel.OnReFocus();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面遮挡恢复异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        public void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            try
            {
                UGUIPanel.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面轮询异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面深度改变
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            this.DepthInUIGroup = depthInUIGroup;

            //uguiPanelBase.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            try
            {
                UGUIPanel.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面深度改变异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }
        }
        
        /// <summary>
        /// 界面回收
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                UGUIPanel.OnRecycle();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面回收异常---ID:{SerialId},名称:{UIPanelAssetName},异常:{exception}");
            }

            SerialId = 0;
            DepthInUIGroup = 0;
            PauseCoveredUIPanel = true;
        }

        #endregion 生命周期
    }
}