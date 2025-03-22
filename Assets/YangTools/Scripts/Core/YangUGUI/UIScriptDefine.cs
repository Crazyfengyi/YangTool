/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:2022-02-19
 */

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YangTools.Scripts.Core.YangUGUI
{
    #region 类定义

    /// <summary>
    /// UI常量定义
    /// </summary>
    public static class UIConstDefine
    {
        public const int DefaultPriority = 0; //默认优先级
    }

    /// <summary>
    /// UI组设置
    /// </summary>
    [Serializable]
    public sealed class UIGroupSetting
    {
        [SerializeField] private string mName = null;
        [Range(0, 160)] [SerializeField] private int mDepth = 0;
        public string Name => mName;
        public int Depth => mDepth;
    }

    #endregion 类定义

    #region 接口定义

    /// <summary>
    /// UI管理器
    /// </summary>
    public interface IUIManager
    {
        /// <summary>
        /// 获取界面组数量
        /// </summary>
        int UIGroupCount { get; }

        #region 事件

        /// <summary>
        /// 打开界面成功事件
        /// </summary>
        event EventHandler<UIPanelOpenSucceedEventArgs> OpenUIPanelSuccess;

        /// <summary>
        /// 打开界面失败事件
        /// </summary>
        event EventHandler<UIPanelOpenFailedEventArgs> OpenUIPanelFailure;

        /// <summary>
        /// 关闭界面完成事件
        /// </summary>
        event EventHandler<UIPanelClosedEventArgs> CloseUIPanelComplete;

        #endregion 事件

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
        /// <param name="uiCreateHelper">辅助器</param>
        void SetUIPanelHelper(IUICreateHelper uiCreateHelper);

        #endregion 辅助接口

        #region UI组操作

        /// <summary>
        /// 是否存在UI界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <returns>是否存在界面组</returns>
        bool HasGroup(string groupName);

        /// <summary>
        /// 获取UI界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <returns>要获取的界面组</returns>
        IUIGroup GetGroup(string groupName);

        /// <summary>
        /// 获取所有UI界面组
        /// </summary>
        /// <returns>所有界面组</returns>
        IUIGroup[] GetAllGroups();

        /// <summary>
        /// 增加UI界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        bool AddGroup(string groupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper);

        #endregion UI组操作

        #region UI界面操作

        /// <summary>
        /// 界面是否打开
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        (bool have, UIPanelInfo panelInfo) PanelIsOpen(string assetName);

        /// <summary>
        /// 是否存在UI界面
        /// </summary>
        /// <param name="serialId">序列编号</param>
        bool HasPanel(int serialId);

        /// <summary>
        /// 是否存在UI界面
        /// </summary>
        /// <param name="assetName">资源名称</param>
        bool HasPanel(string assetName);

        /// <summary>
        /// 获取UI界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        IUIPanel GetPanel(int serialId);

        /// <summary>
        /// 获取UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        IUIPanel GetPanel(string uiPanelAssetName);

        /// <summary>
        /// 获取UI界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        IUIPanel[] GetPanels(string uiPanelAssetName);

        /// <summary>
        /// 获取所有已加载的UI界面
        /// </summary>
        /// <returns>所有已加载的界面</returns>
        IUIPanel[] GetAllLoadedPanels();

        /// <summary>
        /// UI界面是否合法
        /// </summary>
        /// <param name="uiPanel">界面</param>
        bool IsValidPanel(IUIPanel uiPanel);

        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        /// <param name="groupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCoveredUIPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        async UniTask<(int id,IUGUIPanel panel)> OpenPanel(string assetName, string groupName, int priority, bool pauseCoveredUIPanel,
            object userData)
        {
            return (-1,null);
        }

        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="serialId">界面的序列编号</param>
        /// <param name="userData">用户自定义数据</param>
        void ClosePanel(int serialId, object userData);

        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="uiPanel">界面</param>
        /// <param name="userData">用户自定义数据</param>
        void ClosePanel(IUIPanel uiPanel, object userData);

        /// <summary>
        /// 关闭所有已加载的UI界面
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        void CloseAllLoadedPanels(object userData);
        
        #endregion UI界面操作
    }

    /// <summary>
    /// UI组
    /// </summary>
    public interface IUIGroup
    {
        #region 属性

        /// <summary>
        /// 组名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 组深度
        /// </summary>
        int Depth { get; set; }

        /// <summary>
        /// 组是否暂停
        /// </summary>
        bool Pause { get; set; }

        /// <summary>
        /// 组里界面数量
        /// </summary>
        int PanelCount { get; }

        /// <summary>
        /// 当前组选中界面
        /// </summary>
        IUIPanel CurrentPanel { get; }

        /// <summary>
        /// 获取组辅助器
        /// </summary>
        IUIGroupHelper Helper { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 组中是否存在界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>界面组中是否存在界面</returns>
        (bool have, UIPanelInfo panelInfo) HasPanel(int serialId);

        /// <summary>
        /// 组中是否存在界面
        /// </summary>
        /// <param name="panelAssetName">界面资源名称</param>
        /// <returns>界面组中是否存在界面</returns>
        (bool have, UIPanelInfo panelInfo) HasPanel(string panelAssetName);

        /// <summary>
        /// 从组中获取界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>要获取的界面</returns>
        IUIPanel GetPanel(int serialId);

        /// <summary>
        /// 从组中获取界面
        /// </summary>
        /// <param name="panelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        IUIPanel GetPanel(string panelAssetName);

        /// <summary>
        /// 从组中获取界面
        /// </summary>
        /// <param name="panelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        IUIPanel[] GetPanels(string panelAssetName);

        /// <summary>
        /// 从组中获取所有界面
        /// </summary>
        /// <returns>组中的所有界面</returns>
        IUIPanel[] GetAllPanels();

        #endregion 方法
    }

    /// <summary>
    /// UI界面信息类
    /// </summary>
    public interface IUIPanel
    {
        #region 属性

        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        int SerialId { get; }

        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        string UIPanelAssetName { get; }

        /// <summary>
        /// 获取界面实例
        /// </summary>
        object Handle { get; set; }

        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        IUIGroup UIGroup { get; }

        /// <summary>
        /// 获取界面在界面组中的深度
        /// </summary>
        int DepthInUIGroup { get; }

        /// <summary>
        /// 是否打开中
        /// </summary>
        bool IsOpening { get; }

        /// <summary>
        /// 获取是否暂停被覆盖的界面
        /// </summary>
        bool PauseCoveredUIPanel { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 初始化UI界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="panelAssetName">界面资源名称</param>
        /// <param name="group">界面所属的界面组</param>
        /// <param name="pauseCoveredPanel">是否暂停被覆盖的界面</param>
        /// <param name="isNewInstance">是否是新实例</param>
        /// <param name="userData">用户自定义数据</param>
        void OnInit(int serialId, string panelAssetName, IUIGroup group, bool pauseCoveredPanel, bool isNewInstance, object userData);

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
        /// 界面被遮挡时(失去聚焦)
        /// </summary>
        void OnLostFocus();

        /// <summary>
        /// 界面重新聚焦(恢复聚焦)
        /// </summary>
        void OnReFocus();

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

        #endregion 方法
    }

    /// <summary>
    /// UGUI界面
    /// </summary>
    public interface IUGUIPanel
    {
        #region 方法

        /// <summary>
        /// 初始化界面
        /// </summary>
        void OnInit(object userData);

        /// <summary>
        /// UI界面打开时
        /// </summary>
        void OnOpen(object userData);

        /// <summary>
        /// UI界面暂停时
        /// </summary>
        void OnPause();

        /// <summary>
        /// UI界面暂停恢复时
        /// </summary>
        void OnResume();

        /// <summary>
        /// 界面被遮挡时(失去聚焦)
        /// </summary>
        void OnLostFocus();

        /// <summary>
        /// 界面重新聚焦(恢复聚焦)
        /// </summary>
        void OnReFocus();
        
        /// <summary>
        /// UI界面轮询时
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds);

        /// <summary>
        /// UI界面深度改变时
        /// </summary>
        /// <param name="groupDepth">界面组深度</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度</param>
        void OnDepthChanged(int groupDepth, int depthInUIGroup); 

        /// <summary>
        /// UI界面关闭时
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发</param>
        /// <param name="userData">用户自定义数据</param>
        void OnClose(bool isShutdown, object userData);

        /// <summary>
        /// UI界面回收时
        /// </summary>
        void OnRecycle();

        #endregion 方法
    }

    /// <summary>
    /// UI组辅助器
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
    /// UI创建辅助器
    /// </summary>
    public interface IUICreateHelper
    {
        /// <summary>
        /// 实例化界面
        /// </summary>
        /// <param name="panelAsset">要实例化的界面资源</param>
        /// <returns>实例化后的界面</returns>
        object InstantiatePanel(object panelAsset);

        /// <summary>
        /// 创建界面
        /// </summary>
        /// <param name="panelInstance">界面实例</param>
        /// <param name="group">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面</returns>
        IUIPanel CreatePanel(object panelInstance, IUIGroup group, object userData);

        /// <summary>
        /// 释放界面
        /// </summary>
        /// <param name="panelAsset">要释放的界面资源</param>
        /// <param name="panelInstance">要释放的界面实例</param>
        void ReleasePanel(object panelAsset, object panelInstance); 
    }

    #endregion 接口定义

    #region 事件参数定义

    /// <summary>
    /// 打开界面成功事件
    /// </summary>
    public sealed class UIPanelOpenSucceedEventArgs : EventArgs
    {
        /// <summary>
        /// 获取打开成功的界面
        /// </summary>
        public IUIPanel UIPanel { get; private set; }

        /// <summary>
        /// 获取加载持续时间
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public object UserData { get; private set; }

        private UIPanelOpenSucceedEventArgs()
        {
            Clear();
        }

        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            UIPanel = null;
            Duration = 0f;
            UserData = null;
        }
        
        /// <summary>
        /// 创建打开界面成功事件
        /// </summary>
        /// <param name="uiPanel">加载成功的界面</param>
        /// <param name="duration">加载持续时间</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的打开界面成功事件</returns>
        public static UIPanelOpenSucceedEventArgs Create(IUIPanel uiPanel, float duration, object userData)
        {
            UIPanelOpenSucceedEventArgs eventArgs = new UIPanelOpenSucceedEventArgs
            {
                UIPanel = uiPanel,
                Duration = duration,
                UserData = userData
            };
            return eventArgs;
        }
    }

    /// <summary>
    /// 打开界面失败事件
    /// </summary>
    public sealed class UIPanelOpenFailedEventArgs : EventArgs
    {
        /// <summary>
        /// 界面序列编号
        /// </summary>
        public int SerialId { get; private set; }

        /// <summary>
        /// 界面资源名称
        /// </summary>
        public string UIPanelAssetName { get; private set; }

        /// <summary>
        /// 界面组名称
        /// </summary>
        public string UIGroupName { get; private set; }

        /// <summary>
        /// 是否暂停被覆盖的界面
        /// </summary>
        public bool PauseCoveredUIPanel { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// 初始化打开界面失败事件的新实例
        /// </summary>
        private UIPanelOpenFailedEventArgs()
        {
            Clear();
        }

        /// <summary>
        /// 清理打开界面失败事件
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroupName = null;
            PauseCoveredUIPanel = false;
            ErrorMessage = null;
            UserData = null;
        }
        /// <summary>
        /// 创建打开界面失败事件
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCoveredPanel">是否暂停被覆盖的界面</param>
        /// <param name="errorMessage">错误信息</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的打开界面失败事件</returns>
        public static UIPanelOpenFailedEventArgs Create(int serialId, string uiPanelAssetName, string uiGroupName,
            bool pauseCoveredPanel, string errorMessage, object userData)
        {
            UIPanelOpenFailedEventArgs eventArgs = new UIPanelOpenFailedEventArgs
            {
                SerialId = serialId,
                UIPanelAssetName = uiPanelAssetName,
                UIGroupName = uiGroupName,
                PauseCoveredUIPanel = pauseCoveredPanel,
                ErrorMessage = errorMessage,
                UserData = userData
            };
            return eventArgs;
        }
    }

    /// <summary>
    /// 关闭界面完成事件
    /// </summary>
    public sealed class UIPanelClosedEventArgs : EventArgs
    {
        /// <summary>
        /// 获取界面序列编号
        /// </summary>
        public int SerialId { get; private set; }

        /// <summary>
        /// 获取界面资源名称
        /// </summary>
        public string UIPanelAssetName { get; private set; }

        /// <summary>
        /// 获取界面所属的界面组
        /// </summary>
        public IUIGroup UIGroup { get; private set; }

        /// <summary>
        /// 获取用户自定义数据
        /// </summary>
        public object UserData { get; private set; }

        private UIPanelClosedEventArgs()
        {
            Clear();
        }
        /// <summary>
        /// 清理
        /// </summary>
        public void Clear()
        {
            SerialId = 0;
            UIPanelAssetName = null;
            UIGroup = null;
            UserData = null;
        }
        /// <summary>
        /// 创建关闭界面完成事件
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroup">界面所属的界面组</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>创建的关闭界面完成事件</returns>
        public static UIPanelClosedEventArgs Create(int serialId, string uiPanelAssetName, IUIGroup uiGroup,
            object userData)
        {
            UIPanelClosedEventArgs eventArgs = new UIPanelClosedEventArgs
            {
                SerialId = serialId,
                UIPanelAssetName = uiPanelAssetName,
                UIGroup = uiGroup,
                UserData = userData
            };
            return eventArgs;
        }
    }

    #endregion 事件参数定义

    #region 页面打开传参

    /// <summary>
    /// 参数基类
    /// </summary>
    public abstract class UGUIDataBase
    {
    }

    /// <summary>
    /// 默认参数
    /// </summary>
    public class DefaultUGUIDataBase : UGUIDataBase
    {
    }

    #endregion 页面打开参数
}