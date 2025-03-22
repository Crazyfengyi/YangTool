/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-20
*/

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace YangTools.Scripts.Core.YangUGUI
{
    public class UIMonoInstance : MonoBehaviour
    {
        public static UIMonoInstance Instance { get; private set; }

        //UI管理类
        private IUIManager uiManager;

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
            }
            else
            {
                Debug.LogError("UIMonoInstance有重复");
            }

            uiManager = Core.YangToolsManager.GetModule<YangUIManager>();
            if (uiManager == null)
            {
                Debug.LogError("UIManager is null.");
            }
        }

        private void Start()
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
            for (int i = 0; i < uiGroups.Length; i++)
            {
                if (!AddUIGroup(uiGroups[i].Name, uiGroups[i].Depth))
                {
                    Debug.LogWarning($"添加UI组失败:{uiGroups[i].Name}");
                    continue;
                }
            }
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
            return ((UIPanel) uiManager.GetPanel(serialId)).UGUIPanel as T;
        }

        /// <summary>
        /// 获取界面逻辑类
        /// </summary>
        /// <param name="assetName">资源名称</param>
        /// <returns>要获取的界面</returns>
        public T GetUIPanel<T>(string assetName) where T : class, IUGUIPanel
        {
            return ((UIPanel) uiManager.GetPanel(assetName)).UGUIPanel as T;
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
        public async UniTask<(int id, IUGUIPanel panel)> OpenPanel(string assetName, string groupName,
            int priority = UIConstDefine.DefaultPriority, bool pauseCoveredPanel = false, object userData = null)
        {
            return await uiManager.OpenPanel(assetName, groupName, priority, pauseCoveredPanel, userData);
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

        #endregion 关闭界面
    }
}