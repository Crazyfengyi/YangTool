/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YangTools.Scripts.Core.YangUGUI
{
    /// <summary>
    /// 连接 UGUI 场景配置和界面管理器
    /// </summary>
    public class UIMonoInstance : MonoBehaviour
    {
        public static UIMonoInstance Instance { get; private set; }

        public Camera uiCamera;
        public Canvas uiCanvas;

        public Material gray;
        //UI管理类
        private IUIManager uiManager;
        private readonly UniTaskCompletionSource initialization = new(); //初始化完成信号

        [SerializeField] private Transform instanceRoot = null;

        //UI页面辅助类
        public IUICreateHelper PanelHelper { get; } = null;
        //UI组辅助类
        public IUIGroupHelper GroupHelper { get; } = null;
        //UI组设置
        [SerializeField] private UIGroupSetting[] uiGroups = null;
        //UI列表
        private readonly List<IUIPanel> uiPanelResults = new List<IUIPanel>();

        #region 初始化

        /// <summary>
        /// 游戏框架组件初始化
        /// </summary>
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogError("UIMonoInstance有重复");
                Destroy(gameObject);
                return;
            }

            try
            {
                uiManager = YangToolsManager.GetModule<YangUIManager>();
                if (uiManager == null) throw new InvalidOperationException("UI管理器不存在");
            }
            catch (Exception exception)
            {
                initialization.TrySetException(exception);
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// 根据配置完成界面辅助器和组初始化
        /// </summary>
        private void Start()
        {
            if (Instance != this || uiManager == null) return;
            try
            {
                InitializeUI();
                initialization.TrySetResult();
            }
            catch (Exception exception)
            {
                initialization.TrySetException(exception);
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// 复用现有运行时辅助器创建流程
        /// </summary>
        private void InitializeUI()
        {
            //父节点
            if (instanceRoot == null)
            {
                instanceRoot = new GameObject("UIPanelInstances").transform;
                instanceRoot.SetParent(gameObject.transform);
                instanceRoot.localScale = Vector3.one;
                instanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            //界面辅助器
            GameObject uiPanelHelper = new GameObject("UIPanelHelper");
            UGUIPanelCreateHelper uiPanelCreateHelperScript = uiPanelHelper.AddComponent<UGUIPanelCreateHelper>(); 
            Transform tempTransform = uiPanelHelper.transform;
            tempTransform.SetParent(transform);
            tempTransform.localScale = Vector3.one;
            uiManager.SetUIPanelHelper(uiPanelCreateHelperScript);

            //根据设置添加组
            for (int i = 0; uiGroups != null && i < uiGroups.Length; i++)
            {
                if (!AddUIGroup(uiGroups[i].GroupName, uiGroups[i].Depth))
                {
                    Debug.LogWarning($"添加UI组失败:{uiGroups[i].GroupName}");
                    continue;
                }
            }
        }

        /// <summary>
        /// 释放自身单例引用并取消尚未完成的初始化等待
        /// </summary>
        private void OnDestroy()
        {
            initialization.TrySetCanceled();
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// 增加UI界面组
        /// </summary>
        /// <param name="groupName">界面组名称</param>
        /// <param name="depth">界面组深度</param>
        /// <returns>是否增加界面组成功</returns>
        private bool AddUIGroup(string groupName, int depth = 0)
        {
            if (uiManager.HasGroup(groupName)) return false;

            //添加UI组
            GameObject uiGroupHelper = new GameObject();
            UGUIGroupHelper uiPanelHelperScript = uiGroupHelper.AddComponent<UGUIGroupHelper>();
            if (uiGroupHelper == null)
            {
                Debug.LogError("Can not create UI group helper.");
                return false;
            }

            uiGroupHelper.name = string.Format("UI Group-{0}", groupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = uiGroupHelper.transform;
            transform.SetParent(instanceRoot);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            return uiManager.AddGroup(groupName, depth, uiPanelHelperScript);
        }

        #endregion 初始化

        #region 获取界面信息

        public (bool have, UIPanelInfo panelInfo) IsOpening(string assetName)
        {
            return uiManager.PanelIsOpen(assetName);
        }

        #endregion 获取界面信息

        #region 获取UI界面

        /// <summary>
        /// 获取界面逻辑类
        /// </summary>
        /// <param name="serialId">序列编号</param>
        /// <returns>要获取的界面逻辑类</returns>
        public T GetUIPanel<T>(int serialId) where T : class, IUGUIPanel
        {
            UIPanel panel = uiManager?.GetPanel(serialId) as UIPanel; //查询结果
            return panel && YangUIManager.IsAlive(panel.UGUIPanel) ? panel.UGUIPanel as T : null;
        }

        /// <summary>
        /// 获取界面逻辑类
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <returns>要获取的界面</returns>
        public T GetUIPanel<T>(string assetName) where T : class, IUGUIPanel
        {
            UIPanel panel = uiManager?.GetPanel(assetName) as UIPanel; //查询结果
            return panel && YangUIManager.IsAlive(panel.UGUIPanel) ? panel.UGUIPanel as T : null;
        }

        /// <summary>
        /// 获取界面类
        /// </summary>
        /// <param name="assetName">资源名称</param>
        public UIPanel[] GetPanels(string assetName)
        {
            IUIPanel[] uiPanels = uiManager.GetPanels(assetName);
            UIPanel[] uiPanelImpls = new UIPanel[uiPanels.Length];
            for (int i = 0; i < uiPanels.Length; i++)
            {
                uiPanelImpls[i] = (UIPanel) uiPanels[i];
            }

            return uiPanelImpls;
        }

        /// <summary>
        /// 获取所有已加载的界面
        /// </summary>
        public UIPanel[] GetAllLoadedPanels()
        {
            IUIPanel[] uiPanels = uiManager.GetAllLoadedPanels();
            UIPanel[] uiPanelImpls = new UIPanel[uiPanels.Length];
            for (int i = 0; i < uiPanels.Length; i++)
            {
                uiPanelImpls[i] = (UIPanel) uiPanels[i];
            }

            return uiPanelImpls;
        }

        #endregion 获取UI界面

        #region 打开UI界面

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="groupName">组名</param>
        /// <param name="priority">加载资源的优先级</param>
        /// <param name="pauseCoveredPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列号</returns>
        public async UniTask<(int id, IUGUIPanel panel)> OpenPanel(string assetName, GroupType groupType,
            int priority = UIConstDefine.DefaultPriority, bool pauseCoveredPanel = false, object userData = null)
        {
            await initialization.Task;
            if (!this) throw new OperationCanceledException("UI入口已销毁");
            return await uiManager.OpenPanel(assetName, groupType.ToString(), priority, pauseCoveredPanel, userData);
        }

        /// <summary>
        /// 静态打开方法
        /// </summary>
        public static async UniTask<(int id, T panel)> OpenPanel<T>(GroupType groupType, object userData = default,
            string assetName = "") where T : class, IUGUIPanel
        {
            if (string.IsNullOrEmpty(assetName))
            {
                assetName = typeof(T).Name;
            }
            
            if (!Instance) throw new InvalidOperationException("UI入口尚未创建");
            (int id, IUGUIPanel panel) result = await Instance.OpenPanel(assetName, groupType, userData: userData);
            if (result.panel is T panel) return (result.id, panel);
            Instance.ClosePanel(result.id);
            throw new InvalidOperationException($"UI页面类型不匹配 {assetName} {typeof(T).Name}");
        }

        #endregion 打开UI界面

        #region 关闭界面

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        /// <param name="userData">用户自定义数据</param>
        public void ClosePanel(int serialId, object userData = null)
        {
            uiManager.ClosePanel(serialId, userData);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void ClosePanel(UIPanel uiPanel, object userData = null)
        {
            uiManager.ClosePanel(uiPanel, userData);
        }

        /// <summary>
        /// 按页面类型默认资源名关闭最上方实例
        /// </summary>
        public static void ClosePanel<T>() where T : IUGUIPanel
        {
            ClosePanel<T>(typeof(T).Name);
        }

        /// <summary>
        /// 按自定义资源地址关闭最上方实例
        /// </summary>
        public static void ClosePanel<T>(string assetName) where T : IUGUIPanel
        {
            if (!Instance || Instance.uiManager == null) return;
            IUIPanel[] panel = Instance.uiManager.GetPanels(assetName);
            if (panel.Length <= 0)
            {
                return;
            }

            Instance.uiManager.ClosePanel(panel[0],null);
        }
        #endregion 关闭界面
    }
}
