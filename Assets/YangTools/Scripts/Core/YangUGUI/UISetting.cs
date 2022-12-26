/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-19 
*/
using System;
using UnityEngine;

namespace YangTools.UGUI
{
    #region 设置类定义
    /// <summary>
    /// UI静态设置
    /// </summary>
    public static class UISetting
    {
        public const int DefaultPriority = 0;//默认优先级
    }
    /// <summary>
    /// UI组设置
    /// </summary>
    [Serializable]
    internal sealed class UIGroupSetting
    {
        [SerializeField]
        private string m_Name = null;
        [Range(0, 160)]
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
    #endregion

    #region 接口定义
    /// <summary>
    /// UI界面管理器接口
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 获取界面组数量
        /// </summary>
        int UIGroupCount
        {
            get;
        }

        #region 事件
        /// <summary>
        /// 打开界面成功事件
        /// </summary>
        event EventHandler<OpenUIPanelSuccessEventArgs> OpenUIPanelSuccess;
        /// <summary>
        /// 打开界面失败事件
        /// </summary>
        event EventHandler<OpenUIPanelFailureEventArgs> OpenUIPanelFailure;
        /// <summary>
        /// 打开界面更新事件
        /// </summary>
        event EventHandler<OpenUIPanelUpdateEventArgs> OpenUIPanelUpdate;
        /// <summary>
        /// 关闭界面完成事件
        /// </summary>
        event EventHandler<CloseUIPanelCompleteEventArgs> CloseUIPanelComplete;
        #endregion

        #region 辅助接口
        ///// <summary>
        ///// 设置对象池管理器
        ///// </summary>
        ///// <param name="objectPoolManager">对象池管理器</param>
        //void SetObjectPoolManager(IObjectPoolManager objectPoolManager);
        ///// <summary>
        ///// 设置资源管理器
        ///// </summary>
        ///// <param name="resourceManager">资源管理器</param>
        //void SetResourceManager(IResourceManager resourceManager);
        /// <summary>
        /// 设置界面辅助器
        /// </summary>
        /// <param name="uiPanelHelper">界面辅助器</param>
        void SetuiPanelHelper(IUIPanelHelper uiPanelHelper);
        #endregion

        #region UI组操作
        /// <summary>
        /// 是否存在UI界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <returns>是否存在界面组</returns>
        bool HasUIGroup(string uiGroupName);
        /// <summary>
        /// 获取UI界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <returns>要获取的界面组</returns>
        IUIGroup GetUIGroup(string uiGroupName);
        /// <summary>
        /// 获取所有UI界面组
        /// </summary>
        /// <returns>所有界面组</returns>
        IUIGroup[] GetAllUIGroups();
        /// <summary>
        /// 增加UI界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper);
        /// <summary>
        /// 增加UI界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper);
        #endregion

        #region UI界面操作
        /// <summary>
        /// 是否存在UI界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>是否存在界面</returns>
        bool HasUIPanel(int serialId);
        /// <summary>
        /// 是否存在UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>是否存在界面</returns>
        bool HasUIPanel(string uiPanelAssetName);
        /// <summary>
        /// 获取UI界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>要获取的界面</returns>
        IUIPanel GetUIPanel(int serialId);
        /// <summary>
        /// 获取UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        IUIPanel GetUIPanel(string uiPanelAssetName);
        /// <summary>
        /// 获取UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        IUIPanel[] GetUIPanels(string uiPanelAssetName);
        /// <summary>
        /// 获取所有已加载的UI界面
        /// </summary>
        /// <returns>所有已加载的界面</returns>
        IUIPanel[] GetAllLoadedUIPanels();
        /// <summary>
        /// 是否是合法的UI界面
        /// </summary>
        /// <param name="uiPanel">界面</param>
        /// <returns>界面是否合法</returns>
        bool IsValidUIPanel(IUIPanel uiPanel);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, bool pauseCovereduiPanel);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, object userData);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCoveredUIPanel">是否暂停被覆盖的界面</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority, bool pauseCoveredUIPanel);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority, object userData);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCoveredUIPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, bool pauseCoveredUIPanel, object userData);
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCoveredUIPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority, bool pauseCoveredUIPanel, object userData);
        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        void CloseUIPanel(int serialId);
        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        /// <param name="userData">用户自定义数据</param>
        void CloseUIPanel(int serialId, object userData);
        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        void CloseUIPanel(IUIPanel uiPanel);
        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        /// <param name="userData">用户自定义数据</param>
        void CloseUIPanel(IUIPanel uiPanel, object userData);
        /// <summary>
        /// 关闭所有已加载的UI界面
        /// </summary>
        void CloseAllLoadedUIPanels();
        /// <summary>
        /// 关闭所有已加载的UI界面
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        void CloseAllLoadedUIPanels(object userData);
        /// <summary>
        /// 激活UI界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        void RefocusUIPanel(IUIPanel uiPanel);
        /// <summary>
        /// 激活UI界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        /// <param name="userData">用户自定义数据</param>
        void RefocusUIPanel(IUIPanel uiPanel, object userData);
        #endregion
    }
    /// <summary>
    /// UI界面组接口
    /// </summary>
    public interface IUIGroup
    {
        #region 属性
        /// <summary>
        /// 获取界面组名称
        /// </summary>
        string Name
        {
            get;
        }
        /// <summary>
        /// 获取或设置界面组深度
        /// </summary>
        int Depth
        {
            get;
            set;
        }
        /// <summary>
        /// 获取或设置界面组是否暂停
        /// </summary>
        bool Pause
        {
            get;
            set;
        }
        /// <summary>
        /// 获取界面组中界面数量
        /// </summary>
        int uiPanelCount
        {
            get;
        }
        /// <summary>
        /// 获取当前界面
        /// </summary>
        IUIPanel CurrentUIPanel
        {
            get;
        }
        /// <summary>
        /// 获取界面组辅助器
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
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>界面组中是否存在界面</returns>
        bool HasUIPanel(string uiPanelAssetName);
        /// <summary>
        /// 从界面组中获取界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>要获取的界面</returns>
        IUIPanel GetUIPanel(int serialId);
        /// <summary>
        /// 从界面组中获取界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        IUIPanel GetUIPanel(string uiPanelAssetName);
        /// <summary>
        /// 从界面组中获取界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        IUIPanel[] GetUIPanels(string uiPanelAssetName);
        /// <summary>
        /// 从界面组中获取所有界面
        /// </summary>
        /// <returns>界面组中的所有界面</returns>
        IUIPanel[] GetAllUIPanels();
        #endregion
    }
    /// <summary>
    /// UI界面接口
    /// </summary>
    public interface IUIPanel
    {
        #region 属性
        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        int SerialId
        {
            get;
        }
        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        string UIPanelAssetName
        {
            get;
        }
        /// <summary>
        /// 获取界面实例
        /// </summary>
        object Handle
        {
            get;
            set;
        }
        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        IUIGroup UIGroup
        {
            get;
        }
        /// <summary>
        /// 获取界面在界面组中的深度
        /// </summary>
        int DepthInUIGroup
        {
            get;
        }
        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        bool PauseCoveredUIPanel
        {
            get;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 初始化UI界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="isNewInstance">是否是新实例</param>
        /// <param name="userData">用户自定义数据</param>
        void OnInit(int serialId, string uiPanelAssetName, IUIGroup uiGroup, bool pauseCovereduiPanel, bool isNewInstance, object userData);
        /// <summary>
        /// UI界面打开
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        void OnOpen(object userData);
        /// <summary>
        /// UI界面暂停
        /// </summary>
        void OnPause();
        /// <summary>
        /// UI界面暂停恢复
        /// </summary>
        void OnResume();
        /// <summary>
        /// UI界面遮挡
        /// </summary>
        void OnCover();
        /// <summary>
        /// UI界面遮挡恢复
        /// </summary>
        void OnReveal();
        /// <summary>
        /// UI界面激活
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        void OnRefocus(object userData);
        /// <summary>
        /// UI界面轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds);
        /// <summary>
        /// UI界面深度改变
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        void OnDepthChanged(int uiGroupDepth, int depthInUIGroup);
        /// <summary>
        /// UI界面关闭
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        void OnClose(bool isShutdown, object userData);
        /// <summary>
        /// UI界面回收
        /// </summary>
        void OnRecycle();
        #endregion
    }
    /// <summary>
    /// UI界面组辅助器接口
    /// </summary>
    public interface IUIGroupHelper
    {
        /// <summary>
        /// 设置界面组深度
        /// </summary>
        /// <param name="depth">界面组深度</param>
        void SetDepth(int depth);
    }
    /// <summary>
    /// UI界面辅助器接口
    /// </summary>
    public interface IUIPanelHelper
    {
        /// <summary>
        /// 实例化界面
        /// </summary>
        /// <param name="uiPanelAsset">要实例化的界面资源</param>
        /// <returns>实例化后的界面</returns>
        object InstantiateUIPanel(object uiPanelAsset);
        /// <summary>
        /// 创建界面
        /// </summary>
        /// <param name="uiPanelInstance">界面实例</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面</returns>
        IUIPanel CreateUIPanel(object uiPanelInstance, IUIGroup uiGroup, object userData);
        /// <summary>
        /// 释放界面
        /// </summary>
        /// <param name="uiPanelAsset">要释放的界面资源</param>
        /// <param name="uiPanelInstance">要释放的界面实例</param>
        void ReleaseUIPanel(object uiPanelAsset, object uiPanelInstance);
    }
    #endregion

    #region 事件参数定义
    /// <summary>
    /// 打开界面成功事件
    /// </summary>
    public sealed class OpenUIPanelSuccessEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面成功事件的新实例
        /// </summary>
        public OpenUIPanelSuccessEventArgs()
        {
            UIPanel = null;
            Duration = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取打开成功的界面
        /// </summary>
        public IUIPanel UIPanel
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取加载持续时间
        /// </summary>
        public float Duration
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面成功事件
        /// </summary>
        /// <param name="uiPanel">加载成功的界面</param>
        /// <param name="duration">加载持续时间</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的打开界面成功事件</returns>
        public static OpenUIPanelSuccessEventArgs Create(IUIPanel uiPanel, float duration, object userData)
        {
            OpenUIPanelSuccessEventArgs openuiPanelSuccessEventArgs = new OpenUIPanelSuccessEventArgs();
            openuiPanelSuccessEventArgs.UIPanel = uiPanel;
            openuiPanelSuccessEventArgs.Duration = duration;
            openuiPanelSuccessEventArgs.UserData = userData;
            return openuiPanelSuccessEventArgs;
        }
        /// <summary>
        /// 清理打开界面成功事件
        /// </summary>
        public void Clear()
        {
            UIPanel = null;
            Duration = 0f;
            UserData = null;
        }
    }
    /// <summary>
    /// 打开界面失败事件
    /// </summary>
    public sealed class OpenUIPanelFailureEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面失败事件的新实例
        /// </summary>
        public OpenUIPanelFailureEventArgs()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroupName = null;
            PauseCovereduiPanel = false;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取界面组名称
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        public bool PauseCovereduiPanel
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取错误信息
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面失败事件
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的打开界面失败事件</returns>
        public static OpenUIPanelFailureEventArgs Create(int serialId, string uiPanelAssetName, string uiGroupName, bool pauseCovereduiPanel, string errorMessage, object userData)
        {
            OpenUIPanelFailureEventArgs openuiPanelFailureEventArgs = new OpenUIPanelFailureEventArgs();
            openuiPanelFailureEventArgs.SerialId = serialId;
            openuiPanelFailureEventArgs.UIPanelAssetName = uiPanelAssetName;
            openuiPanelFailureEventArgs.UIGroupName = uiGroupName;
            openuiPanelFailureEventArgs.PauseCovereduiPanel = pauseCovereduiPanel;
            openuiPanelFailureEventArgs.ErrorMessage = errorMessage;
            openuiPanelFailureEventArgs.UserData = userData;
            return openuiPanelFailureEventArgs;
        }
        /// <summary>
        /// 清理打开界面失败事件
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroupName = null;
            PauseCovereduiPanel = false;
            ErrorMessage = null;
            UserData = null;
        }
    }
    /// <summary>
    /// 打开界面更新事件
    /// </summary>
    public sealed class OpenUIPanelUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化打开界面更新事件的新实例
        /// </summary>
        public OpenUIPanelUpdateEventArgs()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroupName = null;
            PauseCovereduiPanel = false;
            Progress = 0f;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取界面组名称
        /// </summary>
        public string UIGroupName
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        public bool PauseCovereduiPanel
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取打开界面进度
        /// </summary>
        public float Progress
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建打开界面更新事件
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="progress">打开界面进度</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的打开界面更新事件</returns>
        public static OpenUIPanelUpdateEventArgs Create(int serialId, string uiPanelAssetName, string uiGroupName, bool pauseCovereduiPanel, float progress, object userData)
        {
            OpenUIPanelUpdateEventArgs openuiPanelUpdateEventArgs = new OpenUIPanelUpdateEventArgs();
            openuiPanelUpdateEventArgs.SerialId = serialId;
            openuiPanelUpdateEventArgs.UIPanelAssetName = uiPanelAssetName;
            openuiPanelUpdateEventArgs.UIGroupName = uiGroupName;
            openuiPanelUpdateEventArgs.PauseCovereduiPanel = pauseCovereduiPanel;
            openuiPanelUpdateEventArgs.Progress = progress;
            openuiPanelUpdateEventArgs.UserData = userData;
            return openuiPanelUpdateEventArgs;
        }
        /// <summary>
        /// 清理打开界面更新事件
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroupName = null;
            PauseCovereduiPanel = false;
            Progress = 0f;
            UserData = null;
        }
    }
    /// <summary>
    /// 关闭界面完成事件
    /// </summary>
    public sealed class CloseUIPanelCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化关闭界面完成事件的新实例
        /// </summary>
        public CloseUIPanelCompleteEventArgs()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroup = null;
            UserData = null;
        }

        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        public IUIGroup UIGroup
        {
            get;
            private set;
        }
        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建关闭界面完成事件
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的关闭界面完成事件</returns>
        public static CloseUIPanelCompleteEventArgs Create(int serialId, string uiPanelAssetName, IUIGroup uiGroup, object userData)
        {
            CloseUIPanelCompleteEventArgs closeuiPanelCompleteEventArgs = new CloseUIPanelCompleteEventArgs();
            closeuiPanelCompleteEventArgs.SerialId = serialId;
            closeuiPanelCompleteEventArgs.UIPanelAssetName = uiPanelAssetName;
            closeuiPanelCompleteEventArgs.UIGroup = uiGroup;
            closeuiPanelCompleteEventArgs.UserData = userData;
            return closeuiPanelCompleteEventArgs;
        }
        /// <summary>
        /// 清理关闭界面完成事件
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroup = null;
            UserData = null;
        }
    }
    #endregion
}