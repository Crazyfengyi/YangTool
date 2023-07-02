/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

using System;
using UnityEngine;

namespace YangTools.UGUI
{
    /// <summary>
    /// UI页面管理类--框架自动生成挂载到UI
    /// </summary>
    public class UIPanel : MonoBehaviour, IUIPanel
    {
        private int serialId;//序列号
        private string uiPanelAssetName;//页面资源名
        private IUIGroup uiGroup;//UI组
        private int depthInUIGroup;//在组里的深度
        private bool isOpening;//是否打开中
        private bool pauseCoveredUIPanel;//是否暂停被覆盖的页面
        private object handle;

        private IUGUIPanel uguiPanelBase;//UGUI界面逻辑类

        #region 对外属性

        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId => serialId;

        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName => uiPanelAssetName;

        /// <summary>
        /// 获取界面实例
        /// </summary>
        public object Handle
        {
            get => handle;
            set => handle = value;
        }

        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        public IUIGroup UIGroup => uiGroup;

        /// <summary>
        /// 获取界面深度
        /// </summary>
        public int DepthInUIGroup => depthInUIGroup;

        /// <summary>
        /// 是否打开中
        /// </summary>
        public bool IsOpening => isOpening;

        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        public bool PauseCoveredUIPanel => pauseCoveredUIPanel;

        /// <summary>
        /// 获取界面逻辑类
        /// </summary>
        public IUGUIPanel UGUIPanel => uguiPanelBase;

        #endregion 对外属性

        #region 生命周期

        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroup">界面所处的界面组</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="isNewInstance">是否是新实例</param>
        /// <param name="userData">用户自定义数据</param>
        public void OnInit(int serialId, string uiPanelAssetName, IUIGroup uiGroup, bool pauseCovereduiPanel, bool isNewInstance, object userData)
        {
            this.serialId = serialId;
            this.uiPanelAssetName = uiPanelAssetName;
            this.uiGroup = uiGroup;
            this.depthInUIGroup = 0;
            this.pauseCoveredUIPanel = pauseCovereduiPanel;

            if (!isNewInstance)
            {
                return;
            }

            uguiPanelBase = GetComponent<IUGUIPanel>();
            if (uguiPanelBase == null)
            {
                Debug.LogError(string.Format("UI form '{0}' can not get UI form logic.", uiPanelAssetName));
                return;
            }

            try
            {
                uguiPanelBase.OnInit(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面初始化异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面回收
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                uguiPanelBase.OnRecycle();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面回收异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }

            serialId = 0;
            depthInUIGroup = 0;
            pauseCoveredUIPanel = true;
        }

        /// <summary>
        /// 界面打开
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public void OnOpen(object userData)
        {
            try
            {
                uguiPanelBase.OnOpen(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面打开异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
            isOpening = true;
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
                uguiPanelBase.OnClose(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面关闭异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
            isOpening = false;
        }

        /// <summary>
        /// 界面暂停
        /// </summary>
        public void OnPause()
        {
            try
            {
                uguiPanelBase.OnPause();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面暂停异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面暂停恢复
        /// </summary>
        public void OnResume()
        {
            try
            {
                uguiPanelBase.OnResume();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面暂停恢复异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面遮挡
        /// </summary>
        public void OnCover()
        {
            try
            {
                uguiPanelBase.OnCover();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面遮挡异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面遮挡恢复
        /// </summary>
        public void OnReveal()
        {
            try
            {
                uguiPanelBase.OnReveal();
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面遮挡恢复异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面激活
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public void OnRefocus(object userData)
        {
            try
            {
                uguiPanelBase.OnRefocus(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面激活异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
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
                uguiPanelBase.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面轮询异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        /// <summary>
        /// 界面深度改变
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            this.depthInUIGroup = depthInUIGroup;

            //uguiPanelBase.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            try
            {
                uguiPanelBase.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception exception)
            {
                Debug.LogError($"UI界面深度改变异常---ID:{serialId},名称:{uiPanelAssetName},异常:{exception}");
            }
        }

        #endregion 生命周期
    }
}