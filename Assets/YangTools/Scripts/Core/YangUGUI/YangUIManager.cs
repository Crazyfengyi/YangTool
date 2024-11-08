/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-19
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using YangTools.ObjectPool;

namespace YangTools.UGUI
{
    public class YangUIManager : GameModuleBase, IUIManager
    {
        public static Font mainFont = null;//主字体

        private readonly Dictionary<string, UIGroup> uiGroups;//UI组
        private readonly Queue<IUIPanel> recycleQueue;//回收队列
        private IObjectPool<UIPanelInstanceObject> instancePool;//对象池

        private int serial;//序列号
        private bool isShutdown;//是否关闭
        private IUIPanelHelper uiPanelHelper;//UI界面辅助类

        public event EventHandler<UIPanelOpenSucceedEventArgs> OpenUIPanelSuccess;//打开页面成功事件

        public event EventHandler<UIPanelOpenFailedEventArgs> OpenUIPanelFailure;//打开页面失败事件

        public event EventHandler<OnUIPanelUpdateEventArgs> OpenUIPanelUpdate;//打开页面轮询事件

        public event EventHandler<OnUIPanelCloseEventArgs> CloseUIPanelComplete;//关闭界面时

        /// <summary>
        /// 获取界面组数量
        /// </summary>
        public int UIGroupCount => uiGroups.Count;

        /// <summary>
        /// 初始化
        /// </summary>
        public YangUIManager()
        {
            uiGroups = new Dictionary<string, UIGroup>(StringComparer.Ordinal);
            recycleQueue = new Queue<IUIPanel>();
            //TODO:对象池优化
            instancePool = new ObjectPool<UIPanelInstanceObject>();
            uiPanelHelper = null;
            serial = 0;
            isShutdown = false;
            OpenUIPanelSuccess = null;
            OpenUIPanelFailure = null;
            OpenUIPanelUpdate = null;
            CloseUIPanelComplete = null;
        }

        #region 生命周期

        internal override void InitModule()
        {
        }

        /// <summary>
        /// UI界面管理器轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            while (recycleQueue.Count > 0)
            {
                IUIPanel uiPanel = recycleQueue.Dequeue();
                uiPanel.OnRecycle();
                instancePool.Recycle((UIPanelInstanceObject)uiPanel.Handle);
            }
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                uiGroup.Value.Update(delaTimeSeconds, unscaledDeltaTimeSeconds);
            }
        }

        internal override void CloseModule()
        {
            isShutdown = true;
            CloseAllLoadedUIPanels();
            uiGroups.Clear();
            recycleQueue.Clear();
        }

        #endregion 生命周期

        #region 设置和查询

        /// <summary>
        /// 设置界面辅助器
        /// </summary>
        /// <param name="uiPanelHelper">界面辅助器</param>
        public void SetUIPanelHelper(IUIPanelHelper uiPanelHelper)
        {
            if (uiPanelHelper == null)
            {
                throw new Exception("UI form helper is invalid.");
            }
            this.uiPanelHelper = uiPanelHelper;
        }

        /// <summary>
        /// 是否存在界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <returns>是否存在界面组</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }
            return uiGroups.ContainsKey(uiGroupName);
        }

        /// <summary>
        /// 获取界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <returns>要获取的界面组</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }
            UIGroup uiGroup = null;
            if (uiGroups.TryGetValue(uiGroupName, out uiGroup))
            {
                return uiGroup;
            }
            return null;
        }

        /// <summary>
        /// 获取所有界面组
        /// </summary>
        /// <returns>所有界面组</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            int index = 0;
            IUIGroup[] results = new IUIGroup[uiGroups.Count];
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                results[index++] = uiGroup.Value;
            }
            return results;
        }

        /// <summary>
        /// 是否是合法的界面
        /// </summary>
        /// <param name="uiPanel">界面</param>
        /// <returns>界面是否合法</returns>
        public bool IsValidUIPanel(IUIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                return false;
            }
            return HasUIPanel(uiPanel.SerialId);
        }

        #endregion 设置和查询

        #region 增加组

        /// <summary>
        /// 增加界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        public bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper)
        {
            return AddUIGroup(uiGroupName, 0, uiGroupHelper);
        }

        /// <summary>
        /// 增加界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        public bool AddUIGroup(string uiGroupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }
            if (uiGroupHelper == null)
            {
                throw new Exception("UI group helper is invalid.");
            }
            if (HasUIGroup(uiGroupName))
            {
                return false;
            }
            uiGroups.Add(uiGroupName, new UIGroup(uiGroupName, uiGroupDepth, uiGroupHelper));
            return true;
        }

        #endregion 增加组

        #region 判断界面

        public (bool have, UIPanelInfo panelInfo) PanelIsOpen(string assetName)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                if (uiGroup.Value.UIPanelIsOpen(assetName).have)
                {
                    return uiGroup.Value.UIPanelIsOpen(assetName);
                }
            }
            return (false, null);
        }

        /// <summary>
        /// 是否存在界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>是否存在界面</returns>
        public bool HasUIPanel(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                if (uiGroup.Value.HasUIPanel(serialId).have)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 是否存在界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        /// <returns>是否存在界面</returns>
        public bool HasUIPanel(string assetName)
        {
            CheckStringIsNull(assetName);
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                if (uiGroup.Value.HasUIPanel(assetName).have)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion 判断界面

        #region 获得界面

        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>要获取的界面</returns>
        public IUIPanel GetUIPanel(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                IUIPanel uiPanel = uiGroup.Value.GetUIPanel(serialId);
                if (uiPanel != null)
                {
                    return uiPanel;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        public IUIPanel GetUIPanel(string assetName)
        {
            CheckStringIsNull(assetName);

            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                IUIPanel uiPanel = uiGroup.Value.GetUIPanel(assetName);
                if (uiPanel != null)
                {
                    return uiPanel;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        public IUIPanel[] GetUIPanels(string uiPanelAssetName)
        {
            if (string.IsNullOrEmpty(uiPanelAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            List<IUIPanel> results = new List<IUIPanel>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                results.AddRange(uiGroup.Value.GetUIPanels(uiPanelAssetName));
            }
            return results.ToArray();
        }

        /// <summary>
        /// 获取所有已加载的界面
        /// </summary>
        /// <returns>所有已加载的界面</returns>
        public IUIPanel[] GetAllLoadedUIPanels()
        {
            List<IUIPanel> results = new List<IUIPanel>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                results.AddRange(uiGroup.Value.GetAllUIPanels());
            }
            return results.ToArray();
        }

        #endregion 获得界面

        #region 打开界面

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, UISetting.DefaultPriority, false, null);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, priority, false, null);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, bool pauseCovereduiPanel)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, UISetting.DefaultPriority, pauseCovereduiPanel, null);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, object userData)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, UISetting.DefaultPriority, false, userData);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority, bool pauseCovereduiPanel)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, priority, pauseCovereduiPanel, null);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority, object userData)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, priority, false, userData);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, bool pauseCovereduiPanel, object userData)
        {
            return OpenUIPanel(uiPanelAssetName, uiGroupName, UISetting.DefaultPriority, pauseCovereduiPanel, userData);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        /// <param name="groupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string assetName, string groupName, int priority, bool pauseCovereduiPanel, object userData)
        {
            if (uiPanelHelper == null) throw new Exception("你必须设置一个UIPanelHelper");
            if (string.IsNullOrEmpty(assetName)) throw new Exception("UI资源名为空");
            if (string.IsNullOrEmpty(groupName)) throw new Exception("UI组名为空");

            UIGroup uiGroup = (UIGroup)GetUIGroup(groupName);
            if (uiGroup == null) throw new Exception($"UI组是未找到{groupName}");

            int serialId = ++serial;
            //TODO:对象池
            UIPanelInstanceObject uiPanelInstanceObject = null;/*instancePool?.Get("")*/

            if (uiPanelInstanceObject == null)
            {
                //TODO 需要完整的资源加载器
                UnityEngine.Object panelAsset = Resources.Load("UI/" + assetName + "/" + assetName);//资源
                if (panelAsset == null)
                {
                    Debug.LogError($"UI页面加载失败:{assetName}");
                    throw new Exception();
                }

                uiPanelInstanceObject = UIPanelInstanceObject.Create(assetName, panelAsset, uiPanelHelper.InstantiateUIPanel(panelAsset), uiPanelHelper);

                InternalOpenUIPanel(serialId, assetName, uiGroup, uiPanelInstanceObject.Target, uiPanelInstanceObject, pauseCovereduiPanel, true, 0f, userData);
            }
            else
            {
                //打开界面
                InternalOpenUIPanel(serialId, assetName, uiGroup, uiPanelInstanceObject.Target, uiPanelInstanceObject, pauseCovereduiPanel, false, 0f, userData);
            }

            return serialId;
        }

        #endregion 打开界面

        #region 激活界面

        /// <summary>
        /// 激活界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        public void RefocusUIPanel(IUIPanel uiPanel)
        {
            RefocusUIPanel(uiPanel, null);
        }

        /// <summary>
        /// 激活界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void RefocusUIPanel(IUIPanel uiPanel, object userData)
        {
            if (uiPanel == null)
            {
                throw new Exception("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiPanel.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            uiGroup.RefocusUIPanel(uiPanel, userData);
            uiGroup.Refresh();
            uiPanel.OnRefocus(userData);
        }

        #endregion 激活界面

        #region 关闭界面

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        public void CloseUIPanel(int serialId)
        {
            CloseUIPanel(serialId, null);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        /// <param name="userData">用户自定义数据</param>
        public void CloseUIPanel(int serialId, object userData)
        {
            IUIPanel uiPanel = GetUIPanel(serialId);
            if (uiPanel == null)
            {
                throw new Exception(string.Format("Can not find UI form '{0}'.", serialId.ToString()));
            }
            CloseUIPanel(uiPanel, userData);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        public void CloseUIPanel(IUIPanel uiPanel)
        {
            CloseUIPanel(uiPanel, null);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面/param>
        /// <param name="userData">用户自定义数据</param>
        public void CloseUIPanel(IUIPanel uiPanel, object userData)
        {
            if (uiPanel == null)
            {
                throw new Exception("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiPanel.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            uiGroup.RemoveUIPanel(uiPanel);
            uiPanel.OnClose(isShutdown, userData);
            uiGroup.Refresh();

            if (CloseUIPanelComplete != null)
            {
                OnUIPanelCloseEventArgs closeuiPanelCompleteEventArgs = OnUIPanelCloseEventArgs.Create(uiPanel.SerialId, uiPanel.UIPanelAssetName, uiGroup, userData);
                CloseUIPanelComplete(this, closeuiPanelCompleteEventArgs);
            }

            recycleQueue.Enqueue(uiPanel);
        }

        /// <summary>
        /// 关闭所有已加载的界面
        /// </summary>
        public void CloseAllLoadedUIPanels()
        {
            CloseAllLoadedUIPanels(null);
        }

        /// <summary>
        /// 关闭所有已加载的界面
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public void CloseAllLoadedUIPanels(object userData)
        {
            IUIPanel[] uiPanels = GetAllLoadedUIPanels();
            foreach (IUIPanel uiPanel in uiPanels)
            {
                if (!HasUIPanel(uiPanel.SerialId))
                {
                    continue;
                }
                CloseUIPanel(uiPanel, userData);
            }
        }

        #endregion 关闭界面

        #region 内部方法

        /// <summary>
        /// 打开界面
        /// </summary>
        private void InternalOpenUIPanel(int serialId, string uiPanelAssetName, UIGroup uiGroup, object uiPanelInstance, UIPanelInstanceObject uIPanelInstanceObject, bool pauseCovereduiPanel, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IUIPanel uiPanel = uiPanelHelper.CreateUIPanel(uiPanelInstance, uiGroup, userData);
                if (uiPanel == null) throw new Exception("Can not create UI form in UI form helper.");

                uiPanel.OnInit(serialId, uiPanelAssetName, uiGroup, pauseCovereduiPanel, isNewInstance, userData);
                uiGroup.AddUIPanel(uiPanel);
                uiPanel.OnOpen(userData);
                uiGroup.Refresh();
                uiPanel.Handle = (object)uIPanelInstanceObject;

                if (OpenUIPanelSuccess != null)
                {
                    UIPanelOpenSucceedEventArgs openuiPanelSuccessEventArgs = UIPanelOpenSucceedEventArgs.Create(uiPanel, duration, userData);
                    OpenUIPanelSuccess(this, openuiPanelSuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (OpenUIPanelFailure != null)
                {
                    UIPanelOpenFailedEventArgs openuiPanelFailureEventArgs = UIPanelOpenFailedEventArgs.Create(serialId, uiPanelAssetName, uiGroup.Name, pauseCovereduiPanel, exception.ToString(), userData);
                    OpenUIPanelFailure(this, openuiPanelFailureEventArgs);
                }
                throw;
            }
        }

        #endregion 内部方法

        #region 工具

        public static void CheckStringIsNull(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new Exception($"UI资源名称不合法:{str}");
            }
        }

        #endregion 工具
    }

    /// <summary>
    /// UI界面组
    /// </summary>
    internal sealed partial class UIGroup : IUIGroup
    {
        private readonly string name;//名称
        private int depth;//深度
        private bool pause;//暂停
        private readonly IUIGroupHelper uiGroupHelper;//ui组辅助类
        private readonly LinkedList<UIPanelInfo> uiPanelInfos;//ui页面信息
        private LinkedListNode<UIPanelInfo> cachedNode;//缓存节点

        #region 属性

        /// <summary>
        /// 获取界面组名称
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// 获取或设置界面组深度
        /// </summary>
        public int Depth
        {
            get
            {
                return depth;
            }
            set
            {
                if (depth == value)
                {
                    return;
                }

                depth = value;
                uiGroupHelper.SetDepth(depth);
                Refresh();
            }
        }

        /// <summary>
        /// 获取或设置界面组是否暂停
        /// </summary>
        public bool Pause
        {
            get
            {
                return pause;
            }
            set
            {
                if (pause == value)
                {
                    return;
                }

                pause = value;
                Refresh();
            }
        }

        /// <summary>
        /// 获取界面组中界面数量
        /// </summary>
        public int uiPanelCount
        {
            get
            {
                return uiPanelInfos.Count;
            }
        }

        /// <summary>
        /// 获取当前界面
        /// </summary>
        public IUIPanel CurrentUIPanel
        {
            get
            {
                return uiPanelInfos.First != null ? uiPanelInfos.First.Value.UIPanel : null;
            }
        }

        /// <summary>
        /// 获取界面组辅助器
        /// </summary>
        public IUIGroupHelper Helper
        {
            get
            {
                return uiGroupHelper;
            }
        }

        #endregion 属性

        /// <summary>
        /// 初始化界面组
        /// </summary>
        /// <param name="name">界面组名称</param>
        /// <param name="depth">界面组深度</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        public UIGroup(string name, int depth, IUIGroupHelper uiGroupHelper)
        {
            YangUIManager.CheckStringIsNull(name);

            if (uiGroupHelper == null)
            {
                throw new Exception("UI group helper is invalid.");
            }
            this.name = name;
            pause = false;
            this.uiGroupHelper = uiGroupHelper;
            uiPanelInfos = new LinkedList<UIPanelInfo>();
            cachedNode = null;
            Depth = depth;
        }

        #region 方法

        /// <summary>
        /// UI界面组轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        public void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            LinkedListNode<UIPanelInfo> current = uiPanelInfos.First;
            while (current != null)
            {
                if (current.Value.Paused)
                {
                    break;
                }
                cachedNode = current.Next;
                current.Value.UIPanel.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
                current = cachedNode;
                cachedNode = null;
            }
        }

        /// <summary>
        /// UI界面组中是否存在界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        public (bool have, UIPanelInfo panelInfo) HasUIPanel(int serialId)
        {
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.SerialId == serialId)
                {
                    return (true, uiPanelInfo);
                }
            }

            return (false, null);
        }

        /// <summary>
        /// UI界面组中是否存在界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        public (bool have, UIPanelInfo panelInfo) HasUIPanel(string assetName)
        {
            YangUIManager.CheckStringIsNull(assetName);

            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.UIPanelAssetName == assetName)
                {
                    return (true, uiPanelInfo);
                }
            }
            return (false, null);
        }

        /// <summary>
        /// UI界面组中是否有打开的目标界面
        /// </summary>
        public (bool have, UIPanelInfo panelInfo) UIPanelIsOpen(string assetName)
        {
            (bool have, UIPanelInfo panelInfo) info = HasUIPanel(assetName);
            return info;
        }

        /// <summary>
        /// 从UI界面组中获取界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        public IUIPanel GetUIPanel(int serialId)
        {
            (bool have, UIPanelInfo panelInfo) info = HasUIPanel(serialId);
            return info.have ? info.panelInfo.UIPanel : null;
        }

        /// <summary>
        /// 从UI界面组中获取界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        public IUIPanel GetUIPanel(string assetName)
        {
            YangUIManager.CheckStringIsNull(assetName);
            (bool have, UIPanelInfo panelInfo) info = HasUIPanel(assetName);
            return info.have ? info.panelInfo.UIPanel : null;
        }

        /// <summary>
        /// 从UI界面组中获取界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        public IUIPanel[] GetUIPanels(string assetName)
        {
            YangUIManager.CheckStringIsNull(assetName);

            List<IUIPanel> results = new List<IUIPanel>();
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.UIPanelAssetName == assetName)
                {
                    results.Add(uiPanelInfo.UIPanel);
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// 从UI界面组中获取所有界面
        /// </summary>
        public IUIPanel[] GetAllUIPanels()
        {
            List<IUIPanel> results = new List<IUIPanel>();
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                results.Add(uiPanelInfo.UIPanel);
            }
            return results.ToArray();
        }

        /// <summary>
        /// 往UI界面组增加界面
        /// </summary>
        /// <param name="uiPanel">要增加的界面</param>
        public void AddUIPanel(IUIPanel uiPanel)
        {
            uiPanelInfos.AddFirst(UIPanelInfo.Create(uiPanel));
        }

        /// <summary>
        /// 从UI界面组移除界面
        /// </summary>
        /// <param name="uiPanel">要移除的界面</param>
        public void RemoveUIPanel(IUIPanel uiPanel)
        {
            UIPanelInfo uiPanelInfo = GetUIPanelInfo(uiPanel);
            if (uiPanelInfo == null)
            {
                throw new Exception(string.Format("Can not find UI form info for serial id '{0}', UI form asset name is '{1}'.", uiPanel.SerialId.ToString(), uiPanel.UIPanelAssetName));
            }
            if (!uiPanelInfo.Covered)
            {
                uiPanelInfo.Covered = true;
                uiPanel.OnCover();
            }
            if (!uiPanelInfo.Paused)
            {
                uiPanelInfo.Paused = true;
                uiPanel.OnPause();
            }

            if (cachedNode != null && cachedNode.Value.UIPanel == uiPanel)
            {
                cachedNode = cachedNode.Next;
            }

            if (!uiPanelInfos.Remove(uiPanelInfo))
            {
                throw new Exception(string.Format("UI group '{0}' not exists specified UI form '[{1}]{2}'.", name, uiPanel.SerialId.ToString(), uiPanel.UIPanelAssetName));
            }

            //ReferencePool.Release(uiPanelInfo);
        }

        /// <summary>
        /// 激活UI界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void RefocusUIPanel(IUIPanel uiPanel, object userData)
        {
            UIPanelInfo uiPanelInfo = GetUIPanelInfo(uiPanel);
            if (uiPanelInfo == null)
            {
                throw new Exception("Can not find UI form info.");
            }

            uiPanelInfos.Remove(uiPanelInfo);
            uiPanelInfos.AddFirst(uiPanelInfo);
        }

        /// <summary>
        /// 刷新UI界面组
        /// </summary>
        public void Refresh()
        {
            LinkedListNode<UIPanelInfo> current = uiPanelInfos.First;
            bool pause = this.pause;
            bool cover = false;//覆盖
            int depth = uiPanelCount;
            while (current != null && current.Value != null)
            {
                LinkedListNode<UIPanelInfo> next = current.Next;
                current.Value.UIPanel.OnDepthChanged(Depth, depth--);
                if (current.Value == null)
                {
                    return;
                }

                if (pause)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        current.Value.UIPanel.OnCover();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }

                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        current.Value.UIPanel.OnPause();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        current.Value.UIPanel.OnResume();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }

                    if (current.Value.UIPanel.PauseCoveredUIPanel)
                    {
                        pause = true;
                    }

                    if (cover)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            current.Value.UIPanel.OnCover();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (current.Value.Covered)
                        {
                            current.Value.Covered = false;
                            current.Value.UIPanel.OnReveal();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }

                        cover = true;
                    }
                }

                current = next;
            }
        }

        internal void InternalGetUIPanels(string uiPanelAssetName, List<IUIPanel> results)
        {
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.UIPanelAssetName == uiPanelAssetName)
                {
                    results.Add(uiPanelInfo.UIPanel);
                }
            }
        }

        internal void InternalGetAllUIPanels(List<IUIPanel> results)
        {
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                results.Add(uiPanelInfo.UIPanel);
            }
        }

        /// <summary>
        /// 获得UI界面信息
        /// </summary>
        private UIPanelInfo GetUIPanelInfo(IUIPanel uiPanel)
        {
            if (uiPanel == null)
            {
                throw new Exception("UI form is invalid.");
            }

            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel == uiPanel)
                {
                    return uiPanelInfo;
                }
            }

            return null;
        }

        #endregion 方法
    }

    /// <summary>
    /// UI界面信息
    /// </summary>
    public sealed class UIPanelInfo
    {
        private bool paused;//暂停
        private bool covered;//覆盖
        private IUIPanel uiPanel;
        public IUIPanel UIPanel => uiPanel;

        public bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
            }
        }

        public bool Covered
        {
            get
            {
                return covered;
            }
            set
            {
                covered = value;
            }
        }

        public UIPanelInfo()
        {
        }

        public static UIPanelInfo Create(IUIPanel uiPanel)
        {
            if (uiPanel == null) throw new Exception("UI panel is invalid.");
            UIPanelInfo uiPanelInfo = new UIPanelInfo();
            uiPanelInfo.uiPanel = uiPanel;
            return uiPanelInfo;
        }

        public void Clear()
        {
            uiPanel = null;
            paused = false;
            covered = false;
        }
    }

    /// <summary>
    /// UI界面实例对象
    /// </summary>
    internal sealed class UIPanelInstanceObject : IPoolItem<UIPanelInstanceObject>
    {
        public string PoolKey { get; set; }
        private string name;//名字
        private object target;//目标
        private int priority;//优先级
        private DateTime lastUseTime;//上一次使用时间
        private object uiPanelAsset;//UI资源
        private IUIPanelHelper uiPanelHelper;//UI页面辅助类
        public object Target => target;
        public bool IsInPool { get; set; }

        public UIPanelInstanceObject()
        {
        }

        public static UIPanelInstanceObject Create(string name, object uiPanelAsset, object uiPanelInstance, IUIPanelHelper uiPanelHelper)
        {
            if (uiPanelAsset == null) throw new Exception("UI form asset is invalid.");
            if (uiPanelHelper == null) throw new Exception("UI form helper is invalid.");

            UIPanelInstanceObject uiPanelInstanceObject = new UIPanelInstanceObject();
            uiPanelInstanceObject.Init(name, uiPanelInstance, 0);
            uiPanelInstanceObject.uiPanelAsset = uiPanelAsset;
            uiPanelInstanceObject.uiPanelHelper = uiPanelHelper;
            return uiPanelInstanceObject;
        }

        /// <summary>
        /// 初始化对象基类
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <param name="target">对象</param>
        /// <param name="priority">对象的优先级</param>
        public void Init(string name, object target, int priority)
        {
            this.name = name ?? string.Empty;
            this.target = target;
            this.priority = priority;
            lastUseTime = DateTime.UtcNow;
        }

        public void Clear()
        {
            uiPanelAsset = null;
            uiPanelHelper = null;
        }

        public void Release(bool isShutdown)
        {
            uiPanelHelper.ReleaseUIPanel(uiPanelAsset, target);
        }

        public void OnGet()
        {
        }

        public void OnRecycle()
        {
        }

        public void OnDestroy()
        {
        }
    }
}