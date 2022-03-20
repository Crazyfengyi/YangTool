/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-20 
*/
using UnityEngine;
using System.Collections;
using System;
using YangTools.UGUI;

namespace YangTools.UGUI
{
    /// <summary>
    /// UI页面
    /// </summary>
    public class UIPanel : MonoBehaviour, IUIPanel
    {
        private int serialId;//序列号
        private string uiPanelAssetName;//页面资源名
        private IUIGroup uiGroup;//UI组
        private int depthInUIGroup;//在组里的深度
        private bool pauseCoveredUIPanel;//是否暂停被覆盖的页面

        private UGUIPanelLogicBase uiPanelLogic;//UGUI界面逻辑类

        #region 对外属性
        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId
        {
            get
            {
                return serialId;
            }
        }
        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName
        {
            get
            {
                return uiPanelAssetName;
            }
        }
        /// <summary>
        /// 获取界面实例
        /// </summary>
        public object Handle
        {
            get
            {
                return gameObject;
            }
        }
        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        public IUIGroup UIGroup
        {
            get
            {
                return uiGroup;
            }
        }
        /// <summary>
        /// 获取界面深度
        /// </summary>
        public int DepthInUIGroup
        {
            get
            {
                return depthInUIGroup;
            }
        }
        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        public bool PauseCoveredUIPanel
        {
            get
            {
                return pauseCoveredUIPanel;
            }
        }
        /// <summary>
        /// 获取界面逻辑类
        /// </summary>
        public UGUIPanelLogicBase Logic
        {
            get
            {
                return uiPanelLogic;
            }
        }
        #endregion

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

            uiPanelLogic = GetComponent<UGUIPanelLogicBase>();
            if (uiPanelLogic == null)
            {
                Debug.LogError(string.Format("UI form '{0}' can not get UI form logic.", uiPanelAssetName));
                return;
            }

            try
            {
                uiPanelLogic.OnInit(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnInit with exception '{2}'.", this.serialId.ToString(), this.uiPanelAssetName, exception.ToString()));
            }
        }
        /// <summary>
        /// 界面回收
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                uiPanelLogic.OnRecycle();
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnRecycle with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
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
                uiPanelLogic.OnOpen(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnOpen with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
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
                uiPanelLogic.OnClose(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnClose with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
        }
        /// <summary>
        /// 界面暂停
        /// </summary>
        public void OnPause()
        {
            try
            {
                uiPanelLogic.OnPause();
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnPause with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
        }
        /// <summary>
        /// 界面暂停恢复
        /// </summary>
        public void OnResume()
        {
            try
            {
                uiPanelLogic.OnResume();
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnResume with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
        }
        /// <summary>
        /// 界面遮挡
        /// </summary>
        public void OnCover()
        {
            try
            {
                uiPanelLogic.OnCover();
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnCover with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
        }
        /// <summary>
        /// 界面遮挡恢复
        /// </summary>
        public void OnReveal()
        {
            try
            {
                uiPanelLogic.OnReveal();
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnReveal with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
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
                uiPanelLogic.OnRefocus(userData);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnRefocus with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
        }
        /// <summary>
        /// 界面轮询
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="realElapseSeconds">真实流逝时间,以秒为单位</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                uiPanelLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnUpdate with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
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
            try
            {
                uiPanelLogic.OnDepthChanged(uiGroupDepth, depthInUIGroup);
            }
            catch (Exception exception)
            {
                Debug.LogError(string.Format("UI form '[{0}]{1}' OnDepthChanged with exception '{2}'.", serialId.ToString(), uiPanelAssetName, exception.ToString()));
            }
        }
        #endregion
    }
}