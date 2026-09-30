/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:2022-02-19
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using YangTools.Scripts.Core.YangObjectPool;
using YangTools.Scripts.Core;
using Object = System.Object;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// 管理界面生命周期和默认 UGUI 排序
    /// </summary>
    public class YangUIManager : GameModuleBase, IUIManager
    {
        public static TMP_FontAsset MainFont = null; //主字体

        private readonly Dictionary<string, UIGroup> uiGroups; //UI组
        private readonly Queue<RecycleEntry> recycleQueue; //回收快照队列
        private readonly HashSet<IUIPanel> pendingRecyclePanels; //等待回收的界面
        private readonly HashSet<int> pendingRecycleSerialIds; //等待回收的界面序列号
        private readonly ObjectPool<UIPanelInstanceObject> instancePool; //对象池
        private readonly Dictionary<UIPanelInstanceObject, int> borrowedInstances = new(); //实例当前借出序列号
        private readonly List<UIGroup> groupOrder = new(); //组注册顺序
        private readonly List<UIGroup> updateGroups = new(); //轮询快照
        private readonly List<UIGroup> refreshGroups = new(); //刷新快照
        private readonly UISortingLayout sortingLayout = new(); //排序布局
        private int lifecycleVersion; //生命周期版本
        private bool isRefreshing; //是否正在刷新
        private bool refreshPending; //是否需要再次刷新
        private bool isUpdating; //是否正在轮询
        private int serial; //序列号
        private bool isShutdown; //是否关闭

        public IUICreateHelper UICreateHelper; //UI界面辅助类

        public event EventHandler<UIPanelOpenSucceedEventArgs> OpenUIPanelSuccess; //打开页面成功事件
        public event EventHandler<UIPanelOpenFailedEventArgs> OpenUIPanelFailure; //打开页面失败事件
        public event EventHandler<UIPanelClosedEventArgs> CloseUIPanelComplete; //关闭界面时

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
            recycleQueue = new Queue<RecycleEntry>();
            pendingRecyclePanels = new HashSet<IUIPanel>();
            pendingRecycleSerialIds = new HashSet<int>();
            instancePool = YangObjectPool.YangObjectPool.CreatePool<UIPanelInstanceObject>("UIPanelInstanceObject");

            UICreateHelper = null;
            serial = 0;
            isShutdown = false;
            OpenUIPanelSuccess = null;
            OpenUIPanelFailure = null;
            CloseUIPanelComplete = null;
        }

        #region 生命周期

        /// <summary>
        /// 开始新的管理器生命周期
        /// </summary>
        internal override void InitModule()
        {
            lifecycleVersion++;
            isShutdown = false;
        }

        /// <summary>
        /// UI界面管理器轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            if (isShutdown || isUpdating) return;
            isUpdating = true;
            try
            {
                ProcessRecycleQueue();
                if (refreshPending) RefreshGroups();
                updateGroups.Clear();
                updateGroups.AddRange(groupOrder);
                foreach (UIGroup group in updateGroups)
                {
                    if (isShutdown) break;
                    group.Update(delaTimeSeconds, unscaledDeltaTimeSeconds);
                }
            }
            finally
            {
                updateGroups.Clear();
                isUpdating = false;
            }
        }

        /// <summary>
        /// 取消未完成请求并释放借出和闲置的界面实例
        /// </summary>
        internal override void CloseModule()
        {
            if (isShutdown) return;
            isShutdown = true;
            lifecycleVersion++;
            CloseAllLoadedPanels();
            ProcessRecycleQueue();
            foreach (UIPanelInstanceObject item in borrowedInstances.Keys)
            {
                instancePool.DestroyBorrowed(item);
            }
            borrowedInstances.Clear();
            instancePool.Clear();
            uiGroups.Clear();
            groupOrder.Clear();
            sortingLayout.Clear();
            recycleQueue.Clear();
            pendingRecyclePanels.Clear();
            pendingRecycleSerialIds.Clear();
            refreshPending = false;
        }

        #endregion 生命周期

        #region 设置和查询

        /// <summary>
        /// 设置界面辅助器
        /// </summary>
        public void SetUIPanelHelper(IUICreateHelper uiCreateHelper)
        {
            if (!IsAlive(uiCreateHelper)) throw new ArgumentException("UI界面辅助器无效", nameof(uiCreateHelper));
            UICreateHelper = uiCreateHelper;
        }

        /// <summary>
        /// 是否存在界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <returns>是否存在界面组</returns>
        public bool HasGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new Exception($"UI group name is invalid:{groupName}");
            }

            return uiGroups.ContainsKey(groupName);
        }

        /// <summary>
        /// 获取界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <returns>要获取的界面组</returns>
        public IUIGroup GetGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            return uiGroups.GetValueOrDefault(groupName);
        }

        /// <summary>
        /// 获取所有界面组
        /// </summary>
        /// <returns>所有界面组</returns>
        public IUIGroup[] GetAllGroups()
        {
            var index = 0;
            var results = new IUIGroup[uiGroups.Count];
            foreach (var uiGroup in uiGroups)
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
        public bool IsValidPanel(IUIPanel uiPanel)
        {
            if (!IsAlive(uiPanel))
            {
                return false;
            }

            return ReferenceEquals(GetPanel(uiPanel.SerialId), uiPanel);
        }

        #endregion 设置和查询

        #region 增加组

        /// <summary>
        /// 增加界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <param name="groupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        public bool AddGroup(string groupName, IUIGroupHelper groupHelper)
        {
            return AddGroup(groupName, 0, groupHelper);
        }

        /// <summary>
        /// 增加界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <param name="uiGroupDepth">界面组深度</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        /// <returns>是否增加界面组成功</returns>
        public bool AddGroup(string groupName, int uiGroupDepth, IUIGroupHelper uiGroupHelper)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            if (uiGroupHelper == null)
            {
                throw new Exception("UI group helper is invalid.");
            }

            if (HasGroup(groupName))
            {
                return false;
            }

            if (isShutdown) throw new InvalidOperationException("UI管理器已关闭");
            UIGroup group = new UIGroup(groupName, uiGroupDepth, uiGroupHelper, RefreshGroups, groupOrder.Count,
                panel => ClosePanel(panel)); //新增组
            uiGroups.Add(groupName, group);
            groupOrder.Add(group);
            try
            {
                RefreshGroups();
            }
            catch
            {
                uiGroups.Remove(groupName);
                groupOrder.Remove(group);
                throw;
            }
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
        public bool HasPanel(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                if (uiGroup.Value.HasPanel(serialId).have)
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
        public bool HasPanel(string assetName)
        {
            CheckStringIsNull(assetName);
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                if (uiGroup.Value.HasPanel(assetName).have)
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
        public IUIPanel GetPanel(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                IUIPanel uiPanel = uiGroup.Value.GetPanel(serialId);
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
        public IUIPanel GetPanel(string assetName)
        {
            CheckStringIsNull(assetName);

            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                IUIPanel uiPanel = uiGroup.Value.GetPanel(assetName);
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
        public IUIPanel[] GetPanels(string uiPanelAssetName)
        {
            if (string.IsNullOrEmpty(uiPanelAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            List<IUIPanel> results = new List<IUIPanel>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                results.AddRange(uiGroup.Value.GetPanels(uiPanelAssetName));
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取所有已加载的界面
        /// </summary>
        /// <returns>所有已加载的界面</returns>
        public IUIPanel[] GetAllLoadedPanels()
        {
            List<IUIPanel> results = new List<IUIPanel>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in uiGroups)
            {
                results.AddRange(uiGroup.Value.GetAllPanels());
            }

            return results.ToArray();
        }

        #endregion 获得界面

        #region 打开界面

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="assetName">界面资源名称</param>
        /// <param name="groupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        public async UniTask<(int id, IUGUIPanel panel)> OpenPanel(string assetName, string groupName,
            int priority = UIConstDefine.DefaultPriority, bool pauseCovereduiPanel = false, object userData = null)
        {
            if (isShutdown) throw new OperationCanceledException("UI管理器已关闭");
            if (!IsAlive(UICreateHelper)) throw new Exception("你必须设置一个UIPanelHelper");
            if (string.IsNullOrEmpty(assetName)) throw new Exception("UI资源名为空");
            if (string.IsNullOrEmpty(groupName)) throw new Exception("UI组名为空");
            UIGroup uiGroup = (UIGroup) GetGroup(groupName);
            if (uiGroup == null) throw new Exception($"UI组是未找到{groupName}");

            int serialId = ++serial; //本次序列号
            int version = lifecycleVersion; //请求版本
            IUICreateHelper helper = UICreateHelper; //请求辅助器
            UIPanelInstanceObject item = null; //本次借出实例
            IUIPanel panel = null; //本次页面
            try
            {
                GameObject asset = await ResourceManager.ResourceManager.LoadAssetAsync<GameObject>(assetName); //页面资源
                ValidateOpenRequest(version, uiGroup, helper);
                if (!asset) throw new InvalidOperationException($"UI页面加载失败 {assetName}");

                ProcessRecycleQueue();
                ValidateOpenRequest(version, uiGroup, helper);
                (bool isNew, UIPanelInstanceObject instance) data = await instancePool.Get(assetName, asset, helper); //池获取结果
                item = data.instance;
                borrowedInstances[item] = serialId;
                ValidateOpenRequest(version, uiGroup, helper);

                panel = helper.CreatePanel(item.Target, uiGroup, userData);
                if (!IsAlive(panel)) throw new InvalidOperationException($"UI页面创建失败 {assetName}");
                panel.Handle = item;
                panel.OnInit(serialId, assetName, uiGroup, pauseCovereduiPanel, data.isNew, userData);
                ValidateOpenRequest(version, uiGroup, helper);
                uiGroup.AddUIPanel(panel);
                sortingLayout.Prepare(groupOrder);
                panel.OnOpen(userData);
                ValidateOpenRequest(version, uiGroup, helper);
                if (panel.SerialId != serialId || !IsValidPanel(panel)) throw new OperationCanceledException("UI页面在打开期间已关闭");
                RefreshGroups();
                ValidateOpenRequest(version, uiGroup, helper);
                if (panel.SerialId != serialId || !IsValidPanel(panel)) throw new OperationCanceledException("UI页面在刷新期间已关闭");

                IUGUIPanel logic = item.Target.GetComponent<IUGUIPanel>(); //页面逻辑
                if (!IsAlive(logic)) throw new InvalidOperationException($"UI页面缺少逻辑组件 {assetName}");
                InvokeEvent(OpenUIPanelSuccess, UIPanelOpenSucceedEventArgs.Create(panel, 0f, userData));
                return (serialId, logic);
            }
            catch (OperationCanceledException)
            {
                RollbackOpen(serialId, panel, item, uiGroup, userData);
                throw;
            }
            catch (Exception exception)
            {
                RollbackOpen(serialId, panel, item, uiGroup, userData);
                InvokeEvent(OpenUIPanelFailure, UIPanelOpenFailedEventArgs.Create(serialId, assetName,
                    groupName, pauseCovereduiPanel, exception.ToString(), userData));
                throw;
            }
        }

        #endregion 打开界面

        #region 关闭界面

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        /// <param name="userData">用户自定义数据</param>
        public void ClosePanel(int serialId, object userData = null)
        {
            IUIPanel uiPanel = GetPanel(serialId);
            if (uiPanel == null && pendingRecycleSerialIds.Contains(serialId))
            {
                return;
            }

            if (uiPanel == null) throw new Exception($"Can not find UI form '{serialId.ToString()}'.");
            ClosePanel(uiPanel, userData);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void ClosePanel(IUIPanel uiPanel, object userData = null)
        {
            if (!IsAlive(uiPanel))
            {
                throw new Exception("UI form is invalid.");
            }

            if (pendingRecyclePanels.Contains(uiPanel))
            {
                return;
            }

            UIGroup uiGroup = (UIGroup) uiPanel.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            if (!IsValidPanel(uiPanel)) throw new InvalidOperationException("UI页面不属于当前管理器");
            int serialId = uiPanel.SerialId; //关闭快照序列号
            UIPanelInstanceObject item = uiPanel.Handle as UIPanelInstanceObject; //关闭快照句柄
            UIPanelClosedEventArgs closeUIArgs = UIPanelClosedEventArgs.Create(serialId, uiPanel.UIPanelAssetName, uiGroup, userData); //关闭事件
            pendingRecyclePanels.Add(uiPanel);
            pendingRecycleSerialIds.Add(serialId);
            recycleQueue.Enqueue(new RecycleEntry(uiPanel, serialId, item));
            RunCleanup(() => uiGroup.RemovePanel(uiPanel));
            RunCleanup(() => uiPanel.OnClose(isShutdown, userData));
            RunCleanup(RefreshGroups);
            InvokeEvent(CloseUIPanelComplete, closeUIArgs);
        }

        /// <summary>
        /// 关闭所有已加载的界面
        /// </summary>
        /// <param name="userData">用户自定义数据</param>
        public void CloseAllLoadedPanels(object userData = null)
        {
            IUIPanel[] uiPanels = GetAllLoadedPanels();
            foreach (IUIPanel uiPanel in uiPanels)
            {
                if (!IsAlive(uiPanel) || !HasPanel(uiPanel.SerialId))
                {
                    continue;
                }

                ClosePanel(uiPanel, userData);
            }
        }

        public void ReFocusPanel(IUIPanel uiPanel, object userData)
        {
        }

        #endregion 关闭界面

        #region 内部方法

        /// <summary>
        /// 检查异步请求是否仍属于当前生命周期
        /// </summary>
        private void ValidateOpenRequest(int version, UIGroup group, IUICreateHelper helper)
        {
            if (isShutdown || lifecycleVersion != version || !IsAlive(helper) ||
                !ReferenceEquals(UICreateHelper, helper) || !IsAlive(group.Helper) ||
                !uiGroups.TryGetValue(group.Name, out UIGroup current) || !ReferenceEquals(group, current))
            {
                throw new OperationCanceledException("UI打开请求已失效");
            }
        }

        /// <summary>
        /// 回滚未完成的打开并弃置异常实例
        /// </summary>
        private void RollbackOpen(int serialId, IUIPanel panel, UIPanelInstanceObject item, UIGroup group, object userData)
        {
            if (item == null || !borrowedInstances.TryGetValue(item, out int currentId) || currentId != serialId) return;
            borrowedInstances.Remove(item);
            bool wasClosed = panel != null && pendingRecyclePanels.Remove(panel); //是否已主动关闭
            pendingRecycleSerialIds.Remove(serialId);
            if (IsAlive(panel) && panel.SerialId == serialId)
            {
                group.RemovePanelSilently(panel);
                if (!wasClosed) RunCleanup(() => panel.OnClose(isShutdown, userData));
                RunCleanup(panel.OnRecycle);
            }
            RunCleanup(() => instancePool.DestroyBorrowed(item));
            RunCleanup(RefreshGroups);
        }

        /// <summary>
        /// 回收快照句柄 关闭管理器时直接弃置实例
        /// </summary>
        private void ProcessRecycleQueue()
        {
            while (recycleQueue.Count > 0)
            {
                RecycleEntry entry = recycleQueue.Dequeue(); //回收快照
                if (!pendingRecycleSerialIds.Remove(entry.SerialId))
                {
                    continue;
                }

                pendingRecyclePanels.Remove(entry.Panel);
                if (IsAlive(entry.Panel)) RunCleanup(entry.Panel.OnRecycle);
                if (entry.Item == null || !borrowedInstances.TryGetValue(entry.Item, out int currentId) || currentId != entry.SerialId) continue;
                borrowedInstances.Remove(entry.Item);
                if (isShutdown || !entry.Item.Target)
                {
                    RunCleanup(() => instancePool.DestroyBorrowed(entry.Item));
                }
                else
                {
                    try
                    {
                        instancePool.Recycle(entry.Item);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                        RunCleanup(() => instancePool.DestroyBorrowed(entry.Item));
                    }
                }
            }
        }

        /// <summary>
        /// 使用稳定快照刷新状态并在预校验后提交全部排序
        /// </summary>
        private void RefreshGroups()
        {
            if (isShutdown) return;
            if (isRefreshing)
            {
                refreshPending = true;
                return;
            }
            isRefreshing = true;
            refreshPending = false;
            try
            {
                sortingLayout.Prepare(groupOrder);
                refreshGroups.Clear();
                refreshGroups.AddRange(groupOrder);
                foreach (UIGroup group in refreshGroups)
                {
                    if (isShutdown) return;
                    group.RefreshState();
                }
                if (isShutdown) return;
                sortingLayout.Prepare(groupOrder);
                sortingLayout.Apply();
            }
            finally
            {
                refreshGroups.Clear();
                isRefreshing = false;
            }
        }

        /// <summary>
        /// 逐个调用事件订阅者避免监听器异常改变管理器结果
        /// </summary>
        private void InvokeEvent<TArgs>(EventHandler<TArgs> handlers, TArgs args) where TArgs : EventArgs
        {
            if (handlers == null) return;
            foreach (EventHandler<TArgs> handler in handlers.GetInvocationList())
            {
                RunCleanup(() => handler(this, args));
            }
        }

        /// <summary>
        /// 隔离清理回调异常并继续后续清理
        /// </summary>
        private static void RunCleanup(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// 同时支持普通辅助器和 Unity 对象的存活判断
        /// </summary>
        internal static bool IsAlive(object value)
        {
            return value != null && (value is not UnityEngine.Object unityObject || unityObject != null);
        }

        /// <summary>
        /// 保存关闭时的数据避免回收回调重置页面字段
        /// </summary>
        private readonly struct RecycleEntry
        {
            public readonly IUIPanel Panel; //页面
            public readonly int SerialId; //原序列号
            public readonly UIPanelInstanceObject Item; //原句柄

            /// <summary>
            /// 创建回收快照
            /// </summary>
            public RecycleEntry(IUIPanel panel, int serialId, UIPanelInstanceObject item)
            {
                Panel = panel;
                SerialId = serialId;
                Item = item;
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
        private readonly string name; //名称
        private int depth; //深度
        private bool pause; //暂停
        private readonly IUIGroupHelper uiGroupHelper; //ui组辅助类
        private readonly LinkedList<UIPanelInfo> uiPanelInfos; //ui页面信息
        private LinkedListNode<UIPanelInfo> cachedNode; //缓存节点
        private readonly HashSet<IUIPanel> panels = new(); //组成员
        private readonly List<UIPanelInfo> refreshPanels = new(); //状态刷新快照
        private readonly Action requestRefresh; //请求管理器刷新
        private readonly Action<IUIPanel> closePanel; //请求所属管理器关闭
        internal int RegistrationOrder { get; } //注册顺序

        #region 属性

        /// <summary>
        /// 获取界面组名称
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// 获取或设置界面组深度
        /// </summary>
        public int Depth
        {
            get { return depth; }
            set
            {
                if (depth == value)
                {
                    return;
                }

                int oldDepth = depth; //变更前深度
                depth = value;
                try
                {
                    uiGroupHelper.SetDepth(depth);
                    Refresh();
                }
                catch
                {
                    depth = oldDepth;
                    uiGroupHelper.SetDepth(depth);
                    throw;
                }
            }
        }

        /// <summary>
        /// 获取或设置界面组是否暂停
        /// </summary>
        public bool Pause
        {
            get { return pause; }
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
        public int PanelCount
        {
            get { return uiPanelInfos.Count; }
        }

        /// <summary>
        /// 获取当前界面
        /// </summary>
        public IUIPanel CurrentPanel
        {
            get { return uiPanelInfos.First != null ? uiPanelInfos.First.Value.UIPanel : null; }
        }

        /// <summary>
        /// 获取界面组辅助器
        /// </summary>
        public IUIGroupHelper Helper
        {
            get { return uiGroupHelper; }
        }

        #endregion 属性

        /// <summary>
        /// 初始化界面组
        /// </summary>
        /// <param name="name">界面组名称</param>
        /// <param name="depth">界面组深度</param>
        /// <param name="uiGroupHelper">界面组辅助器</param>
        public UIGroup(string name, int depth, IUIGroupHelper uiGroupHelper)
            : this(name, depth, uiGroupHelper, null, 0)
        {
        }

        /// <summary>
        /// 创建由管理器协调排序的界面组
        /// </summary>
        internal UIGroup(string name, int depth, IUIGroupHelper uiGroupHelper, Action requestRefresh, int registrationOrder,
            Action<IUIPanel> closePanel = null)
        {
            YangUIManager.CheckStringIsNull(name);

            this.name = name;
            pause = false;
            this.uiGroupHelper = uiGroupHelper ?? throw new Exception("UI group helper is invalid.");
            this.requestRefresh = requestRefresh;
            this.closePanel = closePanel;
            RegistrationOrder = registrationOrder;
            uiPanelInfos = new LinkedList<UIPanelInfo>();
            cachedNode = null;
            this.depth = depth;
            uiGroupHelper.SetDepth(depth);
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
        public (bool have, UIPanelInfo panelInfo) HasPanel(int serialId)
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
        /// <param name="panelAssetName">界面资源名称</param>
        public (bool have, UIPanelInfo panelInfo) HasPanel(string panelAssetName)
        {
            YangUIManager.CheckStringIsNull(panelAssetName);

            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.UIPanelAssetName == panelAssetName)
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
            (bool have, UIPanelInfo panelInfo) info = HasPanel(assetName);
            return info;
        }

        /// <summary>
        /// 从UI界面组中获取界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        public IUIPanel GetPanel(int serialId)
        {
            (bool have, UIPanelInfo panelInfo) info = HasPanel(serialId);
            return info.have ? info.panelInfo.UIPanel : null;
        }

        /// <summary>
        /// 从UI界面组中获取界面
        /// </summary>
        /// <param name="panelAssetName">界面资源名称</param>
        public IUIPanel GetPanel(string panelAssetName)
        {
            YangUIManager.CheckStringIsNull(panelAssetName);
            (bool have, UIPanelInfo panelInfo) info = HasPanel(panelAssetName);
            return info.have ? info.panelInfo.UIPanel : null;
        }

        /// <summary>
        /// 从UI界面组中获取界面
        /// </summary>
        /// <param name="panelAssetName">界面资源名称</param>
        public IUIPanel[] GetPanels(string panelAssetName)
        {
            YangUIManager.CheckStringIsNull(panelAssetName);

            List<IUIPanel> results = new List<IUIPanel>();
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.UIPanelAssetName == panelAssetName)
                {
                    results.Add(uiPanelInfo.UIPanel);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 从UI界面组中获取所有界面
        /// </summary>
        public IUIPanel[] GetAllPanels()
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
            if (!panels.Add(uiPanel)) throw new InvalidOperationException("UI页面已在组中");
            uiPanelInfos.AddFirst(UIPanelInfo.Create(uiPanel));
        }

        /// <summary>
        /// 从UI界面组移除界面
        /// </summary>
        /// <param name="uiPanel">要移除的界面</param>
        public void RemovePanel(IUIPanel uiPanel)
        {
            UIPanelInfo uiPanelInfo = GetUIPanelInfo(uiPanel);
            if (uiPanelInfo == null)
            {
                throw new Exception($"未找到界面 id:{uiPanel.SerialId.ToString()},name:{uiPanel.UIPanelAssetName}");
            }

            RemovePanelSilently(uiPanel);
            if (!uiPanelInfo.Focus)
            {
                uiPanelInfo.Focus = true;
                uiPanel.OnLostFocus();
            }

            if (!uiPanelInfo.Paused)
            {
                uiPanelInfo.Paused = true;
                uiPanel.OnPause();
            }

        }

        /// <summary>
        /// 先解除注册并修正轮询游标避免回调重入重复移除
        /// </summary>
        internal void RemovePanelSilently(IUIPanel uiPanel)
        {
            UIPanelInfo info = GetUIPanelInfo(uiPanel); //页面信息
            if (info == null) return;
            if (cachedNode != null && ReferenceEquals(cachedNode.Value.UIPanel, uiPanel)) cachedNode = cachedNode.Next;
            panels.Remove(uiPanel);
            uiPanelInfos.Remove(info);
        }

        /// <summary>
        /// 将页面的自身关闭请求交给所属管理器
        /// </summary>
        internal void ClosePanel(IUIPanel uiPanel)
        {
            closePanel?.Invoke(uiPanel);
        }

        /// <summary>
        /// 刷新UI界面组
        /// </summary>
        public void Refresh()
        {
            if (requestRefresh != null) requestRefresh();
            else RefreshState();
        }

        /// <summary>
        /// 从稳定快照刷新深度和聚焦状态并跳过已移除的页面
        /// </summary>
        internal void RefreshState()
        {
            refreshPanels.Clear();
            refreshPanels.AddRange(uiPanelInfos);
            bool tempPause = this.pause;
            bool cover = false; //覆盖
            int tempDepth = PanelCount;
            foreach (UIPanelInfo info in refreshPanels)
            {
                if (!panels.Contains(info.UIPanel) || !YangUIManager.IsAlive(info.UIPanel)) continue;
                info.UIPanel.OnDepthChanged(Depth, tempDepth--);
                if (!panels.Contains(info.UIPanel)) continue;
                if (tempPause)
                {
                    if (!info.Focus)
                    {
                        info.Focus = true;
                        info.UIPanel.OnLostFocus();
                        if (!panels.Contains(info.UIPanel)) continue;
                    }
                    if (!info.Paused)
                    {
                        info.Paused = true;
                        info.UIPanel.OnPause();
                    }
                }
                else
                {
                    if (info.Paused)
                    {
                        info.Paused = false;
                        info.UIPanel.OnResume();
                        if (!panels.Contains(info.UIPanel)) continue;
                    }
                    if (info.UIPanel.PauseCoveredUIPanel) tempPause = true;
                    if (cover)
                    {
                        if (!info.Focus)
                        {
                            info.Focus = true;
                            info.UIPanel.OnLostFocus();
                        }
                    }
                    else
                    {
                        if (info.Focus)
                        {
                            info.Focus = false;
                            info.UIPanel.OnReFocus();
                            if (!panels.Contains(info.UIPanel)) continue;
                        }
                        cover = true;
                    }
                }
            }
            refreshPanels.Clear();
        }

        public void GetPanels(string uiPanelAssetName, List<IUIPanel> results)
        {
            foreach (UIPanelInfo uiPanelInfo in uiPanelInfos)
            {
                if (uiPanelInfo.UIPanel.UIPanelAssetName == uiPanelAssetName)
                {
                    results.Add(uiPanelInfo.UIPanel);
                }
            }
        }

        public void GetAllPanels(List<IUIPanel> results)
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
        public IUIPanel UIPanel { get; private set; }

        public bool Paused { get; set; }

        public bool Focus { get; set; }

        private UIPanelInfo()
        {
        }

        public static UIPanelInfo Create(IUIPanel uiPanel)
        {
            if (uiPanel == null) throw new Exception("UI界面是无效的");
            UIPanelInfo uiPanelInfo = new UIPanelInfo
            {
                UIPanel = uiPanel
            };
            return uiPanelInfo;
        }

        public void Clear()
        {
            UIPanel = null;
            Paused = false;
            Focus = false;
        }
    }

    /// <summary>
    /// UI界面实例对象
    /// </summary>
    internal sealed class UIPanelInstanceObject : IPoolItem<UIPanelInstanceObject>
    {
        public string Name { get; set; }
        public string PoolKey { get; set; }
        private string name; //名字
        private GameObject uiPanelAsset; //UI资源
        private GameObject target; //实例化的预制体
        private int priority; //优先级
        private DateTime lastUseTime; //上一次使用时间
        private IUICreateHelper iuiCreateHelper; //UI页面辅助类
        private bool isDestroyed; //是否已销毁
        public GameObject Target => target;
        public bool IsInPool { get; set; }

        /// <summary>
        /// 保留对象池要求的无参构造
        /// </summary>
        public UIPanelInstanceObject()
        {
        }

        /// <summary>
        /// 保存创建实例所需的预制体和辅助器
        /// </summary>
        public UIPanelInstanceObject(string name, object uiPanelAsset, IUICreateHelper uiCreateHelper)
        {
            if (uiPanelAsset == null) throw new Exception("UI form asset is invalid.");
            if (uiCreateHelper == null) throw new Exception("UI form helper is invalid.");

            Name = name;
            Init(name, 0);
            this.uiPanelAsset = (GameObject)uiPanelAsset;
            iuiCreateHelper = uiCreateHelper;
        }

        /// <summary>
        /// 初始化对象基类
        /// </summary>
        /// <param name="name">对象名称</param>
        /// <param name="target">对象</param>
        /// <param name="priority">对象的优先级</param>
        public void Init(string name, int priority)
        {
            this.name = name ?? string.Empty;
            Name = this.name;
            this.priority = priority;
            lastUseTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 清理辅助器和资源引用
        /// </summary>
        public void Clear()
        {
            uiPanelAsset = null;
            iuiCreateHelper = null;
        }

        /// <summary>
        /// 释放实例
        /// </summary>
        public void Release(bool isShutdown)
        {
            OnDestroy();
        }

        /// <summary>
        /// 创建实例并拒绝无效创建结果
        /// </summary>
        public Task OnCreate()
        {
            object panel = iuiCreateHelper.InstantiatePanel(uiPanelAsset);
            target = panel as GameObject;
            if (!target) throw new InvalidOperationException($"UI实例创建失败 {Name}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 页面激活由打开生命周期控制
        /// </summary>
        public void OnGet()
        {
        }

        /// <summary>
        /// 回收时禁用实例
        /// </summary>
        public void OnRecycle()
        {
            if (target) target.SetActive(false);
        }

        /// <summary>
        /// 幂等销毁实例并保留预制体的全局缓存策略
        /// </summary>
        public void OnDestroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;
            GameObject instance = target; //待销毁实例
            target = null;
            try
            {
                if (instance)
                {
                    if (YangUIManager.IsAlive(iuiCreateHelper)) iuiCreateHelper.ReleasePanel(uiPanelAsset, instance);
                    else UnityEngine.Object.Destroy(instance);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (instance) UnityEngine.Object.Destroy(instance);
            }
            finally
            {
                Clear();
            }
        }
    }
}
