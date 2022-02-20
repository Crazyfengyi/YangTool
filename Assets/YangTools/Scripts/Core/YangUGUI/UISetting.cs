/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-19 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YangTools.UI
{
    /// <summary>
    /// 界面管理器接口。
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        int UIGroupCount
        {
            get;
        }
        /// <summary>
        /// 打开界面成功事件。
        /// </summary>
        event EventHandler<OpenUIFormSuccessEventArgs> OpenUIPanelSuccess;
        /// <summary>
        /// 打开界面失败事件。
        /// </summary>
        event EventHandler<OpenUIFormFailureEventArgs> OpenUIPanelFailure;
        /// <summary>
        /// 打开界面更新事件。
        /// </summary>
        event EventHandler<OpenUIFormUpdateEventArgs> OpenUIPanelUpdate;
        /// <summary>
        /// 打开界面时加载依赖资源事件。
        /// </summary>
        event EventHandler<OpenUIFormDependencyAssetEventArgs> OpenUIPanelDependencyAsset;
        /// <summary>
        /// 关闭界面完成事件。
        /// </summary>
        event EventHandler<CloseUIFormCompleteEventArgs> CloseUIPanelComplete;
        ///// <summary>
        ///// 设置对象池管理器。
        ///// </summary>
        ///// <param name="objectPoolManager">对象池管理器。</param>
        //void SetObjectPoolManager(IObjectPoolManager objectPoolManager);
        ///// <summary>
        ///// 设置资源管理器。
        ///// </summary>
        ///// <param name="resourceManager">资源管理器。</param>
        //void SetResourceManager(IResourceManager resourceManager);
        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiPanelHelper">界面辅助器。</param>
        void SetUIFormHelper(IUIPanelHelper uiPanelHelper);
        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        bool HasUIGroup(string uiGroupName);
        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        IUIGroup GetUIGroup(string uiGroupName);
        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        IUIGroup[] GetAllUIGroups();
        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper);
        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper);
        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        bool HasUIPanel(int serialId);
        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        bool HasUIPanel(string uiFormAssetName);
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        IUIPanel GetUIPanel(int serialId);
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIPanel GetUIPanel(string uiFormAssetName);
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIPanel[] GetUIPanels(string uiFormAssetName);
        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        IUIPanel[] GetAllLoadedUIPanels();
        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        int[] GetAllLoadingUIPanelSerialIds();
        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        bool IsLoadingUIPanel(int serialId);
        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        bool IsLoadingUIPanel(string uiFormAssetName);
        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        bool IsValidUIPanel(IUIPanel uiForm);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, object userData);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority, object userData);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData);
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData);

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        void CloseUIPanel(int serialId);
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        void CloseUIPanel(int serialId, object userData);
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        void CloseUIPanel(IUIPanel uiForm);
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        void CloseUIPanel(IUIPanel uiForm, object userData);
        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        void CloseAllLoadedUIPanels();
        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        void CloseAllLoadedUIPanels(object userData);
        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        void CloseAllLoadingUIPanels();
        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        void ActiveUIPanel(IUIPanel uiForm);
        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        void ActiveUIPanel(IUIPanel uiForm, object userData);
    }
    /// <summary>
    /// 界面接口。
    /// </summary>
    public interface IUIPanel
    {
        #region 属性
        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        int SerialId
        {
            get;
        }
        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        string UIPanelAssetName
        {
            get;
        }
        /// <summary>
        /// 获取界面实例。
        /// </summary>
        object Handle
        {
            get;
        }
        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        IUIGroup UIGroup
        {
            get;
        }
        /// <summary>
        /// 获取界面在界面组中的深度。
        /// </summary>
        int DepthInUIGroup
        {
            get;
        }
        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        bool PauseCoveredUIPanel
        {
            get;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiFormAssetName">界面资源名称</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面</param>
        /// <param name="isNewInstance">是否是新实例</param>
        /// <param name="userData">用户自定义数据</param>
        void OnInit(int serialId, string uiFormAssetName, IUIGroup uiGroup, bool pauseCoveredUIForm, bool isNewInstance, object userData);
        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        void OnOpen(object userData);
        /// <summary>
        /// 界面暂停。
        /// </summary>
        void OnPause();
        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        void OnResume();
        /// <summary>
        /// 界面遮挡。
        /// </summary>
        void OnCover();
        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        void OnReveal();
        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        void OnRefocus(object userData);
        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void OnUpdate(float elapseSeconds, float realElapseSeconds);
        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        void OnDepthChanged(int uiGroupDepth, int depthInUIGroup);
        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        void OnClose(bool isShutdown, object userData);
        /// <summary>
        /// 界面回收。
        /// </summary>
        void OnRecycle();
        #endregion
    }
    /// <summary>
    /// 界面组接口。
    /// </summary>
    public interface IUIGroup
    {
        #region 属性
        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        string Name
        {
            get;
        }
        /// <summary>
        /// 获取或设置界面组深度。
        /// </summary>
        int Depth
        {
            get;
            set;
        }
        /// <summary>
        /// 获取或设置界面组是否暂停。
        /// </summary>
        bool Pause
        {
            get;
            set;
        }
        /// <summary>
        /// 获取界面组中界面数量。
        /// </summary>
        int UIFormCount
        {
            get;
        }

        /// <summary>
        /// 获取当前界面。
        /// </summary>
        IUIPanel CurrentUIPanel
        {
            get;
        }
        /// <summary>
        /// 获取界面组辅助器。
        /// </summary>
        IUIGroupHelper Helper
        {
            get;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 界面组中是否存在界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>界面组中是否存在界面</returns>
        bool HasUIPanel(int serialId);
        /// <summary>
        /// 界面组中是否存在界面
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称</param>
        /// <returns>界面组中是否存在界面</returns>
        bool HasUIPanel(string uiFormAssetName);
        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        IUIPanel GetUIPanel(int serialId);
        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIPanel GetUIPanel(string uiFormAssetName);
        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIPanel[] GetUIPanels(string uiFormAssetName);
        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <returns>界面组中的所有界面。</returns>
        IUIPanel[] GetAllUIPanels();
        #endregion
    }
    /// <summary>
    /// 界面组辅助器接口。
    /// </summary>
    public interface IUIGroupHelper
    {
        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        void SetDepth(int depth);
    }
    /// <summary>
    /// 界面辅助器接口。
    /// </summary>
    public interface IUIPanelHelper
    {
        /// <summary>
        /// 实例化界面。
        /// </summary>
        /// <param name="uiFormAsset">要实例化的界面资源</param>
        /// <returns>实例化后的界面</returns>
        object InstantiateUIForm(object uiFormAsset);
        /// <summary>
        /// 创建界面
        /// </summary>
        /// <param name="uiFormInstance">界面实例</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面。</returns>
        IUIPanel CreateUIPanel(object uiFormInstance, IUIGroup uiGroup, object userData);
        /// <summary>
        /// 释放界面
        /// </summary>
        /// <param name="uiFormAsset">要释放的界面资源</param>
        /// <param name="uiFormInstance">要释放的界面实例</param>
        void ReleaseUIPanel(object uiFormAsset, object uiFormInstance);
    }
    /// <summary>
    /// UI组设置
    /// </summary>
    [Serializable]
    internal sealed class UIGroupSetting
    {
        [SerializeField]
        private string m_Name = null;
        [SerializeField]
        private int m_Depth = 0;
        public string Name
        {
            get
            {
                return m_Name;
            }
        }
        public int Depth
        {
            get
            {
                return m_Depth;
            }
        }
    }

    #region 事件
    /// <summary>
    /// 打开界面成功事件。
    /// </summary>
    public sealed class OpenUIFormSuccessEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面成功事件的新实例。
        /// </summary>
        public OpenUIFormSuccessEventArgs()
        {
            UIForm = null;
            Duration = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取打开成功的界面。
        /// </summary>
        public IUIPanel UIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取加载持续时间。
        /// </summary>
        public float Duration
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面成功事件。
        /// </summary>
        /// <param name="uiForm">加载成功的界面。</param>
        /// <param name="duration">加载持续时间。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的打开界面成功事件。</returns>
        public static OpenUIFormSuccessEventArgs Create(IUIPanel uiForm, float duration, object userData)
        {
            OpenUIFormSuccessEventArgs openUIFormSuccessEventArgs = new OpenUIFormSuccessEventArgs();
            openUIFormSuccessEventArgs.UIForm = uiForm;
            openUIFormSuccessEventArgs.Duration = duration;
            openUIFormSuccessEventArgs.UserData = userData;
            return openUIFormSuccessEventArgs;
        }
        /// <summary>
        /// 清理打开界面成功事件。
        /// </summary>
        public void Clear()
        {
            UIForm = null;
            Duration = 0f;
            UserData = null;
        }
    }
    /// <summary>
    /// 打开界面失败事件。
    /// </summary>
    public sealed class OpenUIFormFailureEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面失败事件的新实例。
        /// </summary>
        public OpenUIFormFailureEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面失败事件。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的打开界面失败事件。</returns>
        public static OpenUIFormFailureEventArgs Create(int serialId, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, string errorMessage, object userData)
        {
            OpenUIFormFailureEventArgs openUIFormFailureEventArgs = new OpenUIFormFailureEventArgs();
            openUIFormFailureEventArgs.SerialId = serialId;
            openUIFormFailureEventArgs.UIFormAssetName = uiFormAssetName;
            openUIFormFailureEventArgs.UIGroupName = uiGroupName;
            openUIFormFailureEventArgs.PauseCoveredUIForm = pauseCoveredUIForm;
            openUIFormFailureEventArgs.ErrorMessage = errorMessage;
            openUIFormFailureEventArgs.UserData = userData;
            return openUIFormFailureEventArgs;
        }

        /// <summary>
        /// 清理打开界面失败事件。
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            ErrorMessage = null;
            UserData = null;
        }
    }
    /// <summary>
    /// 打开界面更新事件。
    /// </summary>
    public sealed class OpenUIFormUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面更新事件的新实例。
        /// </summary>
        public OpenUIFormUpdateEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            Progress = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取打开界面进度。
        /// </summary>
        public float Progress
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面更新事件。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="progress">打开界面进度。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的打开界面更新事件。</returns>
        public static OpenUIFormUpdateEventArgs Create(int serialId, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, float progress, object userData)
        {
            OpenUIFormUpdateEventArgs openUIFormUpdateEventArgs = new OpenUIFormUpdateEventArgs();
            openUIFormUpdateEventArgs.SerialId = serialId;
            openUIFormUpdateEventArgs.UIFormAssetName = uiFormAssetName;
            openUIFormUpdateEventArgs.UIGroupName = uiGroupName;
            openUIFormUpdateEventArgs.PauseCoveredUIForm = pauseCoveredUIForm;
            openUIFormUpdateEventArgs.Progress = progress;
            openUIFormUpdateEventArgs.UserData = userData;
            return openUIFormUpdateEventArgs;
        }

        /// <summary>
        /// 清理打开界面更新事件。
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            Progress = 0f;
            UserData = null;
        }
    }
    /// <summary>
    /// 打开界面时加载依赖资源事件。
    /// </summary>
    public sealed class OpenUIFormDependencyAssetEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面时加载依赖资源事件的新实例。
        /// </summary>
        public OpenUIFormDependencyAssetEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取被加载的依赖资源名称。
        /// </summary>
        public string DependencyAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取当前已加载依赖资源数量。
        /// </summary>
        public int LoadedCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取总共加载依赖资源数量。
        /// </summary>
        public int TotalCount
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面时加载依赖资源事件。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="dependencyAssetName">被加载的依赖资源名称。</param>
        /// <param name="loadedCount">当前已加载依赖资源数量。</param>
        /// <param name="totalCount">总共加载依赖资源数量。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的打开界面时加载依赖资源事件。</returns>
        public static OpenUIFormDependencyAssetEventArgs Create(int serialId, string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            OpenUIFormDependencyAssetEventArgs openUIFormDependencyAssetEventArgs = new OpenUIFormDependencyAssetEventArgs();
            openUIFormDependencyAssetEventArgs.SerialId = serialId;
            openUIFormDependencyAssetEventArgs.UIFormAssetName = uiFormAssetName;
            openUIFormDependencyAssetEventArgs.UIGroupName = uiGroupName;
            openUIFormDependencyAssetEventArgs.PauseCoveredUIForm = pauseCoveredUIForm;
            openUIFormDependencyAssetEventArgs.DependencyAssetName = dependencyAssetName;
            openUIFormDependencyAssetEventArgs.LoadedCount = loadedCount;
            openUIFormDependencyAssetEventArgs.TotalCount = totalCount;
            openUIFormDependencyAssetEventArgs.UserData = userData;
            return openUIFormDependencyAssetEventArgs;
        }

        /// <summary>
        /// 清理打开界面时加载依赖资源事件。
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroupName = null;
            PauseCoveredUIForm = false;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
    /// <summary>
    /// 关闭界面完成事件。
    /// </summary>
    public sealed class CloseUIFormCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化关闭界面完成事件的新实例。
        /// </summary>
        public CloseUIFormCompleteEventArgs()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroup = null;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        public IUIGroup UIGroup
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建关闭界面完成事件。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所属的界面组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的关闭界面完成事件。</returns>
        public static CloseUIFormCompleteEventArgs Create(int serialId, string uiFormAssetName, IUIGroup uiGroup, object userData)
        {
            CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = new CloseUIFormCompleteEventArgs();
            closeUIFormCompleteEventArgs.SerialId = serialId;
            closeUIFormCompleteEventArgs.UIFormAssetName = uiFormAssetName;
            closeUIFormCompleteEventArgs.UIGroup = uiGroup;
            closeUIFormCompleteEventArgs.UserData = userData;
            return closeUIFormCompleteEventArgs;
        }

        /// <summary>
        /// 清理关闭界面完成事件。
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIFormAssetName = null;
            UIGroup = null;
            UserData = null;
        }
    }
    #endregion
}