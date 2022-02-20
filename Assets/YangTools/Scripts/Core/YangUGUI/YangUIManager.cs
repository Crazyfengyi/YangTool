/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-19 
*/
using UnityEngine;
using System.Collections;
using YangTools;
using System;
using System.Collections.Generic;
using YangTools.ObjectPool;

namespace YangTools.UI
{
    internal class YangUIManager : GameModuleManager, IUIManager
    {
        #region 变量
        private readonly Dictionary<string, UIGroup> m_UIGroups;//UI组
        private readonly Dictionary<int, string> m_UIPanelsBeingLoaded;//界面正在加载
        private readonly HashSet<int> m_UIPanelsToReleaseOnLoad;//界面正在释放
        private readonly Queue<IUIPanel> m_RecycleQueue;//回收队列
        private IObjectPool<UIPanelInstanceObject> m_InstancePool;//对象池
        private IUIPanelHelper m_UIPanelHelper;//界面辅助类
        private int m_Serial;//序列号
        private bool m_IsShutdown;//是否关闭
        //默认界面优先级
        private int DefaultPriority = 0;

        public event EventHandler<OpenUIFormSuccessEventArgs> OpenUIPanelSuccess;//打开页面成功事件
        public event EventHandler<OpenUIFormFailureEventArgs> OpenUIPanelFailure;//打开页面失败事件
        public event EventHandler<OpenUIFormUpdateEventArgs> OpenUIPanelUpdate;//打开页面轮询事件
        public event EventHandler<OpenUIFormDependencyAssetEventArgs> OpenUIPanelDependencyAsset;//打开界面时
        public event EventHandler<CloseUIFormCompleteEventArgs> CloseUIPanelComplete;//关闭界面时

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get
            {
                return m_UIGroups.Count;
            }
        }

        #endregion
        
        /// <summary>
        /// 初始化界面管理器的新实例。
        /// </summary>
        public YangUIManager()
        {
            m_UIGroups = new Dictionary<string, UIGroup>(StringComparer.Ordinal);
            m_UIPanelsBeingLoaded = new Dictionary<int, string>();
            m_UIPanelsToReleaseOnLoad = new HashSet<int>();
            m_RecycleQueue = new Queue<IUIPanel>();
            m_InstancePool = null;
            m_UIPanelHelper = null;
            m_Serial = 0;
            m_IsShutdown = false;
            OpenUIPanelSuccess = null;
            OpenUIPanelFailure = null;
            OpenUIPanelUpdate = null;
            OpenUIPanelDependencyAsset = null;
            CloseUIPanelComplete = null;
        }

        #region 生命周期
        internal override void Init()
        {
        }
        /// <summary>
        /// 界面管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (m_RecycleQueue.Count > 0)
            {
                IUIPanel uiForm = m_RecycleQueue.Dequeue();
                uiForm.OnRecycle();
                m_InstancePool.Recycle((UIPanelInstanceObject)uiForm.Handle);
            }
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                uiGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }
        internal override void CloseModule()
        {
            m_IsShutdown = true;
            CloseAllLoadedUIPanels();
            m_UIGroups.Clear();
            m_UIPanelsBeingLoaded.Clear();
            m_UIPanelsToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }
        #endregion

        #region 设置和查询
        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiPanelHelper">界面辅助器。</param>
        public void SetUIFormHelper(IUIPanelHelper uiPanelHelper)
        {
            if (uiPanelHelper == null)
            {
                throw new Exception("UI form helper is invalid.");
            }
            m_UIPanelHelper = uiPanelHelper;
        }
        /// <summary>
        /// 是否存在界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>是否存在界面组。</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }
            return m_UIGroups.ContainsKey(uiGroupName);
        }
        /// <summary>
        /// 获取界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>要获取的界面组。</returns>
        public IUIGroup GetUIGroup(string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }
            UIGroup uiGroup = null;
            if (m_UIGroups.TryGetValue(uiGroupName, out uiGroup))
            {
                return uiGroup;
            }
            return null;
        }
        /// <summary>
        /// 获取所有界面组。
        /// </summary>
        /// <returns>所有界面组。</returns>
        public IUIGroup[] GetAllUIGroups()
        {
            int index = 0;
            IUIGroup[] results = new IUIGroup[m_UIGroups.Count];
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                results[index++] = uiGroup.Value;
            }
            return results;
        }
        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIPanel(int serialId)
        {
            return m_UIPanelsBeingLoaded.ContainsKey(serialId);
        }
        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIPanel(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI Panel asset name is invalid.");
            }
            return m_UIPanelsBeingLoaded.ContainsValue(uiFormAssetName);
        }
        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIPanel(IUIPanel uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }
            return HasUIPanel(uiForm.SerialId);
        }
        #endregion

        #region 增加组
        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        /// <returns>是否增加界面组成功。</returns>
        public bool AddUIGroup(string uiGroupName, IUIGroupHelper uiGroupHelper)
        {
            return AddUIGroup(uiGroupName, 0, uiGroupHelper);
        }
        /// <summary>
        /// 增加界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
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
            m_UIGroups.Add(uiGroupName, new UIGroup(uiGroupName, uiGroupDepth, uiGroupHelper));
            return true;
        }
        #endregion

        #region 判断界面
        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIPanel(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIPanel(serialId))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIPanel(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIPanel(uiFormAssetName))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region 获得界面
       
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIPanel GetUIPanel(int serialId)
        {
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                IUIPanel uiForm = uiGroup.Value.GetUIPanel(serialId);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIPanel GetUIPanel(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                IUIPanel uiForm = uiGroup.Value.GetUIPanel(uiFormAssetName);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIPanel[] GetUIPanels(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            List<IUIPanel> results = new List<IUIPanel>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                results.AddRange(uiGroup.Value.GetUIPanels(uiFormAssetName));
            }
            return results.ToArray();
        }
        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIPanelSerialIds()
        {
            int index = 0;
            int[] results = new int[m_UIPanelsBeingLoaded.Count];
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in m_UIPanelsBeingLoaded)
            {
                results[index++] = uiFormBeingLoaded.Key;
            }
            return results;
        }
        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public IUIPanel[] GetAllLoadedUIPanels()
        {
            List<IUIPanel> results = new List<IUIPanel>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                results.AddRange(uiGroup.Value.GetAllUIPanels());
            }
            return results.ToArray();
        }
        #endregion

        #region 打开界面
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, DefaultPriority, false, null);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, priority, false, null);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, DefaultPriority, pauseCoveredUIForm, null);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, object userData)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, DefaultPriority, false, userData);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, null);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority, object userData)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, priority, false, userData);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm, object userData)
        {
            return OpenUIPanel(uiFormAssetName, uiGroupName, DefaultPriority, pauseCoveredUIForm, userData);
        }
        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="priority">加载界面资源的优先级。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>界面的序列编号。</returns>
        public int OpenUIPanel(string uiFormAssetName, string uiGroupName, int priority, bool pauseCoveredUIForm, object userData)
        {

            if (m_UIPanelHelper == null)
            {
                throw new Exception("You must set UI form helper first.");
            }
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }
            if (string.IsNullOrEmpty(uiGroupName))
            {
                throw new Exception("UI group name is invalid.");
            }

            UIGroup uiGroup = (UIGroup)GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                throw new Exception(string.Format("UI group '{0}' is not exist.", uiGroupName));
            }
            int serialId = ++m_Serial;
            UIPanelInstanceObject uiFormInstanceObject = m_InstancePool.Get();
            //TODO: UIManager
            //uiFormInstanceObject.OnGet(uiFormInstanceObject);
            //UIPanelInstanceObject uiFormInstanceObject = m_InstancePool.Get<UIPanelInstanceObject>(uiFormAssetName);
            if (uiFormInstanceObject == null)
            {
                m_UIPanelsBeingLoaded.Add(serialId, uiFormAssetName);
                //m_ResourceManager.LoadAsset(uiFormAssetName, priority, m_LoadAssetCallbacks, OpenUIFormInfo.Create(serialId, uiGroup, pauseCoveredUIForm, userData));
            }
            else
            {
                //InternalOpenUIPanel(serialId, uiFormAssetName, uiGroup, uiFormInstanceObject.Target, pauseCoveredUIForm, false, 0f, userData);
            }
            return serialId;
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(IUIPanel uiForm)
        {
            RefocusUIForm(uiForm, null);
        }
        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIPanel uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }
            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }
            uiGroup.RefocusUIForm(uiForm, userData);
            uiGroup.Refresh();
            uiForm.OnRefocus(userData);
        }

        #endregion

        #region 激活界面
        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void ActiveUIPanel(IUIPanel uiForm)
        {
            RefocusUIForm(uiForm, null);
        }
        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void ActiveUIPanel(IUIPanel uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            uiGroup.RefocusUIForm(uiForm, userData);
            uiGroup.Refresh();
            uiForm.OnRefocus(userData);
        }
        #endregion

        #region 关闭界面
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        public void CloseUIPanel(int serialId)
        {
            CloseUIPanel(serialId, null);
        }
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIPanel(int serialId, object userData)
        {
            if (IsLoadingUIPanel(serialId))
            {
                m_UIPanelsToReleaseOnLoad.Add(serialId);
                m_UIPanelsBeingLoaded.Remove(serialId);
                return;
            }
            IUIPanel uiForm = GetUIPanel(serialId);
            if (uiForm == null)
            {
                throw new Exception(string.Format("Can not find UI form '{0}'.", serialId.ToString()));
            }
            CloseUIPanel(uiForm, userData);
        }
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        public void CloseUIPanel(IUIPanel uiForm)
        {
            CloseUIPanel(uiForm, null);
        }
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面/param>
        /// <param name="userData">用户自定义数据</param>
        public void CloseUIPanel(IUIPanel uiForm, object userData)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            UIGroup uiGroup = (UIGroup)uiForm.UIGroup;
            if (uiGroup == null)
            {
                throw new Exception("UI group is invalid.");
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(m_IsShutdown, userData);
            uiGroup.Refresh();

            if (CloseUIPanelComplete != null)
            {
                CloseUIFormCompleteEventArgs closeUIFormCompleteEventArgs = CloseUIFormCompleteEventArgs.Create(uiForm.SerialId, uiForm.UIPanelAssetName, uiGroup, userData);
                CloseUIPanelComplete(this, closeUIFormCompleteEventArgs);
            }

            m_RecycleQueue.Enqueue(uiForm);
        }
        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIPanels()
        {
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in m_UIPanelsBeingLoaded)
            {
                m_UIPanelsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            m_UIPanelsBeingLoaded.Clear();
        }
        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        public void CloseAllLoadedUIPanels()
        {
            CloseAllLoadedUIPanels(null);
        }
        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseAllLoadedUIPanels(object userData)
        {
            IUIPanel[] uiForms = GetAllLoadedUIPanels();
            foreach (IUIPanel uiForm in uiForms)
            {
                if (!HasUIPanel(uiForm.SerialId))
                {
                    continue;
                }
                CloseUIPanel(uiForm, userData);
            }
        }
        #endregion

        #region 内部方法
        /// <summary>
        /// 打开界面
        /// </summary>
        private void InternalOpenUIForm(int serialId, string uiFormAssetName, UIGroup uiGroup, object uiFormInstance, bool pauseCoveredUIForm, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IUIPanel uiPanel = m_UIPanelHelper.CreateUIPanel(uiFormInstance, uiGroup, userData);
                if (uiPanel == null)
                {
                    throw new Exception("Can not create UI form in UI form helper.");
                }

                uiPanel.OnInit(serialId, uiFormAssetName, uiGroup, pauseCoveredUIForm, isNewInstance, userData);
                uiGroup.AddUIForm(uiPanel);
                uiPanel.OnOpen(userData);
                uiGroup.Refresh();

                if (OpenUIPanelSuccess != null)
                {
                    OpenUIFormSuccessEventArgs openUIFormSuccessEventArgs = OpenUIFormSuccessEventArgs.Create(uiPanel, duration, userData);
                    OpenUIPanelSuccess(this, openUIFormSuccessEventArgs);
                }
            }
            catch (Exception exception)
            {
                if (OpenUIPanelFailure != null)
                {
                    OpenUIFormFailureEventArgs openUIFormFailureEventArgs = OpenUIFormFailureEventArgs.Create(serialId, uiFormAssetName, uiGroup.Name, pauseCoveredUIForm, exception.ToString(), userData);
                    OpenUIPanelFailure(this, openUIFormFailureEventArgs);
                    return;
                }
                throw;
            }
        }
        #endregion
    }
    /// <summary>
    /// 界面组。
    /// </summary>
    internal sealed partial class UIGroup : IUIGroup
    {
        private readonly string m_Name;//名称
        private int m_Depth;//深度
        private bool m_Pause;//暂停
        private readonly IUIGroupHelper m_UIGroupHelper;
        private readonly LinkedList<UIPanelInfo> m_UIPanelInfos;
        private LinkedListNode<UIPanelInfo> m_CachedNode;
        /// <summary>
        /// 初始化界面组的新实例。
        /// </summary>
        /// <param name="name">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        public UIGroup(string name, int depth, IUIGroupHelper uiGroupHelper)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("UI group name is invalid.");
            }
            if (uiGroupHelper == null)
            {
                throw new Exception("UI group helper is invalid.");
            }
            m_Name = name;
            m_Pause = false;
            m_UIGroupHelper = uiGroupHelper;
            m_UIPanelInfos = new LinkedList<UIPanelInfo>();
            m_CachedNode = null;
            Depth = depth;
        }
        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string Name
        {
            get
            {
                return m_Name;
            }
        }
        /// <summary>
        /// 获取或设置界面组深度。
        /// </summary>
        public int Depth
        {
            get
            {
                return m_Depth;
            }
            set
            {
                if (m_Depth == value)
                {
                    return;
                }

                m_Depth = value;
                m_UIGroupHelper.SetDepth(m_Depth);
                Refresh();
            }
        }
        /// <summary>
        /// 获取或设置界面组是否暂停。
        /// </summary>
        public bool Pause
        {
            get
            {
                return m_Pause;
            }
            set
            {
                if (m_Pause == value)
                {
                    return;
                }

                m_Pause = value;
                Refresh();
            }
        }
        /// <summary>
        /// 获取界面组中界面数量。
        /// </summary>
        public int UIFormCount
        {
            get
            {
                return m_UIPanelInfos.Count;
            }
        }
        /// <summary>
        /// 获取当前界面。
        /// </summary>
        public IUIPanel CurrentUIPanel
        {
            get
            {
                return m_UIPanelInfos.First != null ? m_UIPanelInfos.First.Value.UIPanel : null;
            }
        }
        /// <summary>
        /// 获取界面组辅助器。
        /// </summary>
        public IUIGroupHelper Helper
        {
            get
            {
                return m_UIGroupHelper;
            }
        }
        /// <summary>
        /// 界面组轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<UIPanelInfo> current = m_UIPanelInfos.First;
            while (current != null)
            {
                if (current.Value.Paused)
                {
                    break;
                }
                m_CachedNode = current.Next;
                current.Value.UIPanel.OnUpdate(elapseSeconds, realElapseSeconds);
                current = m_CachedNode;
                m_CachedNode = null;
            }
        }
        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIPanel(int serialId)
        {
            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel.SerialId == serialId)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIPanel(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }
            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel.UIPanelAssetName == uiFormAssetName)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIPanel GetUIPanel(int serialId)
        {
            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel.SerialId == serialId)
                {
                    return uiFormInfo.UIPanel;
                }
            }
            return null;
        }
        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIPanel GetUIPanel(string uiPanelAssetName)
        {
            if (string.IsNullOrEmpty(uiPanelAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel.UIPanelAssetName == uiPanelAssetName)
                {
                    return uiFormInfo.UIPanel;
                }
            }

            return null;
        }
        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIPanel[] GetUIPanels(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new Exception("UI form asset name is invalid.");
            }

            List<IUIPanel> results = new List<IUIPanel>();
            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel.UIPanelAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIPanel);
                }
            }

            return results.ToArray();
        }
        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <returns>界面组中的所有界面。</returns>
        public IUIPanel[] GetAllUIPanels()
        {
            List<IUIPanel> results = new List<IUIPanel>();
            foreach (UIPanelInfo uiPanelInfo in m_UIPanelInfos)
            {
                results.Add(uiPanelInfo.UIPanel);
            }
            return results.ToArray();
        }
        /// <summary>
        /// 往界面组增加界面。
        /// </summary>
        /// <param name="uiForm">要增加的界面。</param>
        public void AddUIForm(IUIPanel uiForm)
        {
            m_UIPanelInfos.AddFirst(UIPanelInfo.Create(uiForm));
        }
        /// <summary>
        /// 从界面组移除界面。
        /// </summary>
        /// <param name="uiForm">要移除的界面。</param>
        public void RemoveUIForm(IUIPanel uiForm)
        {
            UIPanelInfo uiFormInfo = GetUIPanelInfo(uiForm);
            if (uiFormInfo == null)
            {
                throw new Exception(string.Format("Can not find UI form info for serial id '{0}', UI form asset name is '{1}'.", uiForm.SerialId.ToString(), uiForm.UIPanelAssetName));
            }
            if (!uiFormInfo.Covered)
            {
                uiFormInfo.Covered = true;
                uiForm.OnCover();
            }
            if (!uiFormInfo.Paused)
            {
                uiFormInfo.Paused = true;
                uiForm.OnPause();
            }

            if (m_CachedNode != null && m_CachedNode.Value.UIPanel == uiForm)
            {
                m_CachedNode = m_CachedNode.Next;
            }

            if (!m_UIPanelInfos.Remove(uiFormInfo))
            {
                throw new Exception(string.Format("UI group '{0}' not exists specified UI form '[{1}]{2}'.", m_Name, uiForm.SerialId.ToString(), uiForm.UIPanelAssetName));
            }

            //ReferencePool.Release(uiFormInfo);
        }
        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIPanel uiForm, object userData)
        {
            UIPanelInfo uiFormInfo = GetUIPanelInfo(uiForm);
            if (uiFormInfo == null)
            {
                throw new Exception("Can not find UI form info.");
            }

            m_UIPanelInfos.Remove(uiFormInfo);
            m_UIPanelInfos.AddFirst(uiFormInfo);
        }
        /// <summary>
        /// 刷新界面组。
        /// </summary>
        public void Refresh()
        {
            LinkedListNode<UIPanelInfo> current = m_UIPanelInfos.First;
            bool pause = m_Pause;
            bool cover = false;
            int depth = UIFormCount;
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
        internal void InternalGetUIForms(string uiFormAssetName, List<IUIPanel> results)
        {
            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel.UIPanelAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIPanel);
                }
            }
        }
        internal void InternalGetAllUIForms(List<IUIPanel> results)
        {
            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                results.Add(uiFormInfo.UIPanel);
            }
        }
        private UIPanelInfo GetUIPanelInfo(IUIPanel uiForm)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }

            foreach (UIPanelInfo uiFormInfo in m_UIPanelInfos)
            {
                if (uiFormInfo.UIPanel == uiForm)
                {
                    return uiFormInfo;
                }
            }

            return null;
        }
    }
    /// <summary>
    /// 界面信息。
    /// </summary>
    internal sealed class UIPanelInfo
    {
        private IUIPanel m_UIForm;
        private bool m_Paused;//被暂停
        private bool m_Covered;//被覆盖
        public UIPanelInfo()
        {
            m_UIForm = null;
            m_Paused = false;
            m_Covered = false;
        }
        public IUIPanel UIPanel
        {
            get
            {
                return m_UIForm;
            }
        }
        public bool Paused
        {
            get
            {
                return m_Paused;
            }
            set
            {
                m_Paused = value;
            }
        }
        public bool Covered
        {
            get
            {
                return m_Covered;
            }
            set
            {
                m_Covered = value;
            }
        }
        public static UIPanelInfo Create(IUIPanel uiForm)
        {
            if (uiForm == null)
            {
                throw new Exception("UI form is invalid.");
            }
            UIPanelInfo uiFormInfo = new UIPanelInfo();
            uiFormInfo.m_UIForm = uiForm;
            uiFormInfo.m_Paused = true;
            uiFormInfo.m_Covered = true;
            return uiFormInfo;
        }
        public void Clear()
        {
            m_UIForm = null;
            m_Paused = false;
            m_Covered = false;
        }
    }
    /// <summary>
    /// 界面实例对象。
    /// </summary>
    internal sealed class UIPanelInstanceObject : IPoolItem<UIPanelInstanceObject>
    {
        private string m_Name;
        private object m_Target;
        private int m_Priority;

        private DateTime m_LastUseTime;
        private object m_UIFormAsset;
        private IUIPanelHelper m_UIFormHelper;

        public bool IsInPool { get; set; }
        public UIPanelInstanceObject()
        {
            m_UIFormAsset = null;
            m_UIFormHelper = null;
        }
        public static UIPanelInstanceObject Create(string name, object uiFormAsset, object uiFormInstance, IUIPanelHelper uiFormHelper)
        {
            if (uiFormAsset == null)
            {
                throw new Exception("UI form asset is invalid.");
            }

            if (uiFormHelper == null)
            {
                throw new Exception("UI form helper is invalid.");
            }

            UIPanelInstanceObject uiFormInstanceObject = new UIPanelInstanceObject();
            uiFormInstanceObject.Init(name, uiFormInstance, 0);
            uiFormInstanceObject.m_UIFormAsset = uiFormAsset;
            uiFormInstanceObject.m_UIFormHelper = uiFormHelper;
            return uiFormInstanceObject;
        }
        /// <summary>
        /// 初始化对象基类。
        /// </summary>
        /// <param name="name">对象名称。</param>
        /// <param name="target">对象。</param>
        /// <param name="priority">对象的优先级。</param>
        protected void Init(string name, object target, int priority)
        {
            m_Name = name ?? string.Empty;
            m_Target = target;
            m_Priority = priority;
            m_LastUseTime = DateTime.UtcNow;
        }

        public void Clear()
        {
            m_UIFormAsset = null;
            m_UIFormHelper = null;
        }

        internal void Release(bool isShutdown)
        {
            m_UIFormHelper.ReleaseUIPanel(m_UIFormAsset, m_Target);
        }

        public UIPanelInstanceObject Create()
        {
            throw new NotImplementedException();
        }

        public void OnGet(UIPanelInstanceObject t)
        {
            throw new NotImplementedException();
        }

        public void OnRecycle(UIPanelInstanceObject t)
        {
            throw new NotImplementedException();
        }

        public void OnDestroy(UIPanelInstanceObject t)
        {
            throw new NotImplementedException();
        }
    }
}
