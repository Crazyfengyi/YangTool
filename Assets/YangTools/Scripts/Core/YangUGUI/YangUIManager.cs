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
using UnityEngine;
using YangTools.Scripts.Core.YangObjectPool;
using YangTools.Scripts.Core;

namespace YangTools.Scripts.Core.YangUGUI
{
    public class YangUIManager : GameModuleBase, IUIManager 
    {
        public static Font MainFont = null;//主字体

        private readonly Dictionary<string, UIGroup> uiGroups;//UI组
        private readonly Queue<IUIPanel> recycleQueue;//回收队列
        private readonly IObjectPool<UIPanelInstanceObject> instancePool;//对象池
        private int serial;//序列号
        private bool isShutdown;//是否关闭
        
        public IUICreateHelper UICreateHelper;//UI界面辅助类

        public event EventHandler<UIPanelOpenSucceedEventArgs> OpenUIPanelSuccess;//打开页面成功事件
        public event EventHandler<UIPanelOpenFailedEventArgs> OpenUIPanelFailure;//打开页面失败事件
        public event EventHandler<UIPanelClosedEventArgs> CloseUIPanelComplete;//关闭界面时

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
            instancePool = new ObjectPool<UIPanelInstanceObject>();
            UICreateHelper = null;
            serial = 0;
            isShutdown = false;
            OpenUIPanelSuccess = null;
            OpenUIPanelFailure = null;
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
            CloseAllLoadedPanels();
            uiGroups.Clear();
            recycleQueue.Clear();
        }

        #endregion 生命周期

        #region 设置和查询

        /// <summary>
        /// 设置界面辅助器
        /// </summary>
        public void SetUIPanelHelper(IUICreateHelper uiCreateHelper)
        {
            this.UICreateHelper = uiCreateHelper ?? throw new Exception("设置界面辅助器有问题");
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
            if (uiPanel == null)
            {
                return false;
            }
            return HasPanel(uiPanel.SerialId);
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
            uiGroups.Add(groupName, new UIGroup(groupName, uiGroupDepth, uiGroupHelper));
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
        public async UniTask<(int id,IUGUIPanel panel)> OpenPanel(string assetName, string groupName,
            int priority = UIConstDefine.DefaultPriority, bool pauseCovereduiPanel = false, object userData = null)
        {
            if (UICreateHelper == null) throw new Exception("你必须设置一个UIPanelHelper");
            if (string.IsNullOrEmpty(assetName)) throw new Exception("UI资源名为空");
            if (string.IsNullOrEmpty(groupName)) throw new Exception("UI组名为空");
            UIGroup uiGroup = (UIGroup)GetGroup(groupName);
            if (uiGroup == null) throw new Exception($"UI组是未找到{groupName}");

            int serialId = ++serial;
            //TODO:对象池
            UIPanelInstanceObject uiPanelInstanceObject = null;/*instancePool?.Get("")*/
            IUIPanel uiPanel = null;
            if (uiPanelInstanceObject == null)
            {
                //TODO 需要完整的资源加载器
                UnityEngine.Object panelAsset = Resources.Load("UI/" + assetName + "/" + assetName);//资源
                //await 异步加载
                if (!panelAsset)
                {
                    Debug.LogError($"UI页面加载失败:{assetName}");
                    throw new Exception();
                }

                uiPanelInstanceObject = UIPanelInstanceObject.Create(assetName, panelAsset, UICreateHelper.InstantiatePanel(panelAsset), UICreateHelper);
                uiPanel = OpenUIPanel(serialId, assetName, uiGroup, uiPanelInstanceObject.Target, uiPanelInstanceObject, pauseCovereduiPanel, true, 0f, userData);
            }
            else
            {
                //打开界面
                uiPanel = OpenUIPanel(serialId, assetName, uiGroup, uiPanelInstanceObject.Target, uiPanelInstanceObject, pauseCovereduiPanel, false, 0f, userData);
            }
            return (serialId,(uiPanelInstanceObject.Target as GameObject)?.GetComponent<IUGUIPanel>());
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
            if (uiPanel == null)throw new Exception($"Can not find UI form '{serialId.ToString()}'.");
            ClosePanel(uiPanel, userData);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void ClosePanel(IUIPanel uiPanel, object userData = null)
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

            uiGroup.RemovePanel(uiPanel);
            uiPanel.OnClose(isShutdown, userData);
            uiGroup.Refresh();

            if (CloseUIPanelComplete != null)
            {
                UIPanelClosedEventArgs closeuiPanelCompleteEventArgs = UIPanelClosedEventArgs.Create(uiPanel.SerialId, uiPanel.UIPanelAssetName, uiGroup, userData);
                CloseUIPanelComplete(this, closeuiPanelCompleteEventArgs);
            }
            
            recycleQueue.Enqueue(uiPanel);
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
                if (!HasPanel(uiPanel.SerialId))
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
        /// 打开界面
        /// </summary>
        private IUIPanel OpenUIPanel(int serialId, string panelAssetName, UIGroup group, object panelInstance, UIPanelInstanceObject panelInstanceObject, bool pauseCoveredPanel, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IUIPanel uiPanel = UICreateHelper.CreatePanel(panelInstance, group, userData);
                if (uiPanel == null) throw new Exception("Can not create UI form in UI form helper.");

                uiPanel.OnInit(serialId, panelAssetName, group, pauseCoveredPanel, isNewInstance, userData);
                group.AddUIPanel(uiPanel);
                uiPanel.OnOpen(userData);
                group.Refresh();
                uiPanel.Handle = (object)panelInstanceObject;

                if (OpenUIPanelSuccess != null)
                {
                    UIPanelOpenSucceedEventArgs openuiPanelSuccessEventArgs = UIPanelOpenSucceedEventArgs.Create(uiPanel, duration, userData);
                    OpenUIPanelSuccess(this, openuiPanelSuccessEventArgs);
                }
                return uiPanel;
            }
            catch (Exception exception)
            {
                if (OpenUIPanelFailure != null)
                {
                    UIPanelOpenFailedEventArgs openuiPanelFailureEventArgs = UIPanelOpenFailedEventArgs.Create(serialId, panelAssetName, group.Name, pauseCoveredPanel, exception.ToString(), userData);
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
        public int PanelCount
        {
            get
            {
                return uiPanelInfos.Count;
            }
        }

        /// <summary>
        /// 获取当前界面
        /// </summary>
        public IUIPanel CurrentPanel
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

            this.name = name;
            pause = false;
            this.uiGroupHelper = uiGroupHelper ?? throw new Exception("UI group helper is invalid.");
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
        /// 刷新UI界面组
        /// </summary>
        public void Refresh()
        {
            LinkedListNode<UIPanelInfo> current = uiPanelInfos.First;
            bool tempPause = this.pause;
            bool cover = false;//覆盖
            int tempDepth = PanelCount;
            while (current != null && current.Value != null)
            {
                LinkedListNode<UIPanelInfo> next = current.Next;
                current.Value.UIPanel.OnDepthChanged(Depth, tempDepth--);
                if (current.Value == null)
                {
                    return;
                }

                if (tempPause)
                {
                    if (!current.Value.Focus)
                    {
                        current.Value.Focus = true;
                        current.Value.UIPanel.OnLostFocus();
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
                        tempPause = true;
                    }

                    if (cover)
                    {
                        if (!current.Value.Focus)
                        {
                            current.Value.Focus = true;
                            current.Value.UIPanel.OnLostFocus();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (current.Value.Focus)
                        {
                            current.Value.Focus = false;
                            current.Value.UIPanel.OnReFocus();
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
        public string PoolKey { get; set; }
        private string name;//名字
        private object target;//目标
        private int priority;//优先级
        private DateTime lastUseTime;//上一次使用时间
        private object uiPanelAsset;//UI资源
        private IUICreateHelper iuiCreateHelper;//UI页面辅助类
        public object Target => target;
        public bool IsInPool { get; set; }

        public UIPanelInstanceObject()
        {
        }

        public static UIPanelInstanceObject Create(string name, object uiPanelAsset, object uiPanelInstance, IUICreateHelper iuiCreateHelper)
        {
            if (uiPanelAsset == null) throw new Exception("UI form asset is invalid.");
            if (iuiCreateHelper == null) throw new Exception("UI form helper is invalid.");

            UIPanelInstanceObject uiPanelInstanceObject = new UIPanelInstanceObject();
            uiPanelInstanceObject.Init(name, uiPanelInstance, 0);
            uiPanelInstanceObject.uiPanelAsset = uiPanelAsset;
            uiPanelInstanceObject.iuiCreateHelper = iuiCreateHelper;
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
            iuiCreateHelper = null;
        }

        public void Release(bool isShutdown)
        {
            iuiCreateHelper.ReleasePanel(uiPanelAsset, target);
        }

        public Task OnCreate()
        {
           return Task.CompletedTask;
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