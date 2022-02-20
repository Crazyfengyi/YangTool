/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-20 
*/
using UnityEngine;
using System.Collections;
using YangTools.UI;
using System.Collections.Generic;
using YangTools;

public class UIMonoInstance : MonoBehaviour
{
    private static UIMonoInstance instance;
    public static UIMonoInstance Instance { get { return instance; } }
    private const int DefaultPriority = 0;
    private IUIManager m_UIManager = null;
    //内部UI列表
    private readonly List<IUIPanel> m_InternalUIFormResults = new List<IUIPanel>();
    //UI父节点
    [SerializeField]
    private Transform m_InstanceRoot = null;
    //页面辅助类
    private IUIPanelHelper panelHelper;
    //组辅助类
    private IUIGroupHelper groupHelper;
    //组设定
    [SerializeField]
    private UIGroupSetting[] m_UIGroups = null;

    /// <summary>
    /// 游戏框架组件初始化。
    /// </summary>
    public void Awake()
    {
        instance = this;
        m_UIManager = YangToolsManager.GetModule<IUIManager>();
        if (m_UIManager == null)
        {
            Debug.LogError("UI manager is invalid.");
            return;
        }
    }
    private void Start()
    {
        GameObject uiPanelHelper = new GameObject();
        UGUIPanelHelper uiPanelHelperScript = uiPanelHelper.AddComponent<UGUIPanelHelper>();
        uiPanelHelper.name = "UI Panel Helper";
        Transform transform = uiPanelHelper.transform;
        transform.SetParent(this.transform);
        transform.localScale = Vector3.one;
        m_UIManager.SetUIFormHelper(uiPanelHelperScript);

        if (m_InstanceRoot == null)
        {
            m_InstanceRoot = new GameObject("UI Panel Instances").transform;
            m_InstanceRoot.SetParent(gameObject.transform);
            m_InstanceRoot.localScale = Vector3.one;
        }
        m_InstanceRoot.gameObject.layer = LayerMask.NameToLayer("UI");

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
    /// 增加界面组。
    /// </summary>
    /// <param name="uiGroupName">界面组名称。</param>
    /// <param name="depth">界面组深度。</param>
    /// <returns>是否增加界面组成功。</returns>
    public bool AddUIGroup(string uiGroupName, int depth = 0)
    {
        if (m_UIManager.HasUIGroup(uiGroupName))
        {
            return false;
        }

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
        transform.SetParent(m_InstanceRoot);
        transform.localScale = Vector3.one;
        return m_UIManager.AddUIGroup(uiGroupName, depth, uiPanelHelperScript);
    }

    #region 获取页面
    /// <summary>
    /// 获取界面。
    /// </summary>
    /// <param name="serialId">界面序列编号。</param>
    /// <returns>要获取的界面。</returns>
    public UIPanelBase GetUIForm(int serialId)
    {
        return (UIPanelBase)m_UIManager.GetUIPanel(serialId);
    }
    /// <summary>
    /// 获取界面。
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称。</param>
    /// <returns>要获取的界面。</returns>
    public UIPanelBase GetUIForm(string uiFormAssetName)
    {
        return (UIPanelBase)m_UIManager.GetUIPanel(uiFormAssetName);
    }
    /// <summary>
    /// 获取界面。
    /// </summary>
    /// <param name="uiFormAssetName">界面资源名称。</param>
    /// <returns>要获取的界面。</returns>
    public UIPanelBase[] GetUIForms(string uiFormAssetName)
    {
        IUIPanel[] uiForms = m_UIManager.GetUIPanels(uiFormAssetName);
        UIPanelBase[] uiFormImpls = new UIPanelBase[uiForms.Length];
        for (int i = 0; i < uiForms.Length; i++)
        {
            uiFormImpls[i] = (UIPanelBase)uiForms[i];
        }
        return uiFormImpls;
    }

    /// <summary>
    /// 获取所有已加载的界面。
    /// </summary>
    /// <returns>所有已加载的界面。</returns>
    public UIPanelBase[] GetAllLoadedUIForms()
    {
        IUIPanel[] uiForms = m_UIManager.GetAllLoadedUIPanels();
        UIPanelBase[] uiFormImpls = new UIPanelBase[uiForms.Length];
        for (int i = 0; i < uiForms.Length; i++)
        {
            uiFormImpls[i] = (UIPanelBase)uiForms[i];
        }

        return uiFormImpls;
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
        return m_UIManager.OpenUIPanel(uiFormAssetName, uiGroupName, priority, pauseCoveredUIForm, userData);
    }
    #endregion

    #region 关闭界面
    /// <summary>
    /// 关闭界面。
    /// </summary>
    /// <param name="serialId">要关闭界面的序列编号。</param>
    public void CloseUIPanel(int serialId)
    {
        m_UIManager.CloseUIPanel(serialId);
    }
    /// <summary>
    /// 关闭界面。
    /// </summary>
    /// <param name="serialId">要关闭界面的序列编号。</param>
    /// <param name="userData">用户自定义数据。</param>
    public void CloseUIPanel(int serialId, object userData)
    {
        m_UIManager.CloseUIPanel(serialId, userData);
    }
    /// <summary>
    /// 关闭界面。
    /// </summary>
    /// <param name="uiForm">要关闭的界面。</param>
    public void CloseUIPanel(UIPanelBase uiForm)
    {
        m_UIManager.CloseUIPanel(uiForm);
    }
    /// <summary>
    /// 关闭界面。
    /// </summary>
    /// <param name="uiForm">要关闭的界面。</param>
    /// <param name="userData">用户自定义数据。</param>
    public void CloseUIPanel(UIPanelBase uiForm, object userData)
    {
        m_UIManager.CloseUIPanel(uiForm, userData);
    }
    #endregion

    #region 激活界面
    /// <summary>
    /// 激活界面。
    /// </summary>
    /// <param name="uiForm">要激活的界面。</param>
    public void ActiveUIPanel(UIPanelBase uiForm)
    {
        m_UIManager.ActiveUIPanel(uiForm);
    }
    /// <summary>
    /// 激活界面。
    /// </summary>
    /// <param name="uiForm">要激活的界面。</param>
    /// <param name="userData">用户自定义数据。</param>
    public void ActiveUIPanel(UIPanelBase uiForm, object userData)
    {
        m_UIManager.ActiveUIPanel(uiForm, userData);
    }
    #endregion
}