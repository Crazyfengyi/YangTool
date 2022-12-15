/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-20 
*/
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.UGUI
{
    public class UIMonoInstance : MonoBehaviour
    {
        private static UIMonoInstance instance;
        public static UIMonoInstance Instance { get { return instance; } }

        //UI管理类
        private IUIManager UIManager = null;
        [SerializeField]
        //UI父节点
        private Transform instanceRoot = null;
        //UI页面辅助类
        private IUIPanelHelper panelHelper = null;
        //UI组辅助类
        private IUIGroupHelper groupHelper = null;

        //UI组设置
        [SerializeField]
        private UIGroupSetting[] m_UIGroups = null;
        //UI列表
        private readonly List<IUIPanel> internalUIPanelResults = new List<IUIPanel>();

        #region 初始化
        /// <summary>
        /// 游戏框架组件初始化
        /// </summary>
        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("UIMonoInstance 有重复");
            }

            UIManager = YangToolsManager.GetModule<YangUIManager>();
            if (UIManager == null)
            {
                Debug.LogError("UI manager is null.");
                return;
            }
        }
        private void Start()
        {
            //父节点
            if (instanceRoot == null)
            {
                instanceRoot = new GameObject("UI Panel Instances").transform;
                instanceRoot.SetParent(gameObject.transform);
                instanceRoot.localScale = Vector3.one;
            }
            instanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

            //界面辅助器
            GameObject uiPanelHelper = new GameObject();
            UGUIPanelHelper uiPanelHelperScript = uiPanelHelper.AddComponent<UGUIPanelHelper>();
            uiPanelHelper.name = "UI Panel Helper";
            Transform transform = uiPanelHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;
            UIManager.SetuiPanelHelper(uiPanelHelperScript);

            //根据设置添加组
            for (int i = 0; i < m_UIGroups.Length; i++)
            {
                if (!AddUIGroup(m_UIGroups[i].Name, m_UIGroups[i].Depth))
                {
                    Debug.LogWarning(string.Format("Add UI group '{0}' failure.", m_UIGroups[i].Name));
                    continue;
                }
            }
        }
        /// <summary>
        /// 增加UI界面组
        /// </summary>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="depth">界面组深度</param>
        /// <returns>是否增加界面组成功</returns>
        public bool AddUIGroup(string uiGroupName, int depth = 0)
        {
            if (UIManager.HasUIGroup(uiGroupName))
            {
                return false;
            }

            //添加UI组
            GameObject uiGroupHelper = new GameObject();
            UGUIGroupHelper uiPanelHelperScript = uiGroupHelper.AddComponent<UGUIGroupHelper>();
            if (uiGroupHelper == null)
            {
                Debug.LogError("Can not create UI group helper.");
                return false;
            }
            uiGroupHelper.name = string.Format("UI Group - {0}", uiGroupName);
            uiGroupHelper.gameObject.layer = LayerMask.NameToLayer("UI");
            Transform transform = uiGroupHelper.transform;
            transform.SetParent(instanceRoot);
            transform.localScale = Vector3.one;

            return UIManager.AddUIGroup(uiGroupName, depth, uiPanelHelperScript);
        }
        #endregion

        #region 获取UI界面
        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="serialId">界面序列编号</param>
        /// <returns>要获取的界面</returns>
        public UIPanel GetUIPanel(int serialId)
        {
            return (UIPanel)UIManager.GetUIPanel(serialId);
        }
        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        public UIPanel GetUIPanel(string uiPanelAssetName)
        {
            return (UIPanel)UIManager.GetUIPanel(uiPanelAssetName);
        }
        /// <summary>
        /// 获取界面
        /// </summary>
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <returns>要获取的界面</returns>
        public UIPanel[] GetUIPanels(string uiPanelAssetName)
        {
            IUIPanel[] uiPanels = UIManager.GetUIPanels(uiPanelAssetName);
            UIPanel[] uiPanelImpls = new UIPanel[uiPanels.Length];
            for (int i = 0; i < uiPanels.Length; i++)
            {
                uiPanelImpls[i] = (UIPanel)uiPanels[i];
            }
            return uiPanelImpls;
        }
        /// <summary>
        /// 获取所有已加载的界面
        /// </summary>
        /// <returns>所有已加载的界面</returns>
        public UIPanel[] GetAllLoadedUIPanels()
        {
            IUIPanel[] uiPanels = UIManager.GetAllLoadedUIPanels();
            UIPanel[] uiPanelImpls = new UIPanel[uiPanels.Length];
            for (int i = 0; i < uiPanels.Length; i++)
            {
                uiPanelImpls[i] = (UIPanel)uiPanels[i];
            }
            return uiPanelImpls;
        }
        #endregion

        #region 打开UI界面
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
        /// <param name="priority">界面的优先级</param>
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
        /// <param name="uiPanelAssetName">界面资源名称</param>
        /// <param name="uiGroupName">界面组名称</param>
        /// <param name="priority">加载界面资源的优先级</param>
        /// <param name="pauseCovereduiPanel">是否暂停被覆盖的界面</param>
        /// <param name="userData">用户自定义数据</param>
        /// <returns>界面的序列编号</returns>
        public int OpenUIPanel(string uiPanelAssetName, string uiGroupName, int priority, bool pauseCovereduiPanel, object userData)
        {
            return UIManager.OpenUIPanel(uiPanelAssetName, uiGroupName, priority, pauseCovereduiPanel, userData);
        }
        #endregion

        #region 关闭界面
        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        public void CloseUIPanel(int serialId)
        {
            UIManager.CloseUIPanel(serialId);
        }
        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号</param>
        /// <param name="userData">用户自定义数据</param>
        public void CloseUIPanel(int serialId, object userData)
        {
            UIManager.CloseUIPanel(serialId, userData);
        }
        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        public void CloseUIPanel(UIPanel uiPanel)
        {
            UIManager.CloseUIPanel(uiPanel);
        }
        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiPanel">要关闭的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void CloseUIPanel(UIPanel uiPanel, object userData)
        {
            UIManager.CloseUIPanel(uiPanel, userData);
        }
        #endregion

        #region 激活界面
        /// <summary>
        /// 激活界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        public void ActiveUIPanel(UIPanel uiPanel)
        {
            UIManager.RefocusUIPanel(uiPanel);
        }
        /// <summary>
        /// 激活界面
        /// </summary>
        /// <param name="uiPanel">要激活的界面</param>
        /// <param name="userData">用户自定义数据</param>
        public void ActiveUIPanel(UIPanel uiPanel, object userData)
        {
            UIManager.RefocusUIPanel(uiPanel, userData);
        }
        #endregion
    }
}