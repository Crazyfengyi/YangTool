//****************************************
//作者：ChenHua
//邮箱：1184923569@qq.com
//创建时间：2022年08月22日 星期一 11:54:19
//****************************************

using System;
using System.Collections.Generic;
using ResizeSettingLib.Core.Config;
using ResizeSettingLib.Core.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 设置界面
/// </summary>
public class UISettingWindow:MonoBehaviour
{
    #region UI元素

    [SerializeField]
    private UISettingDragObject m_DragObjectPrefab;
    [SerializeField]
    private Transform m_DragObjectContainer;

    [SerializeField]
    private UISettingLimitAreaObject m_LimitAreaObjectPrefab;
    [SerializeField]
    private Transform m_LimitAreaContainer;
    
    [SerializeField]
    private RectTransform m_SelectCover;
    
    [SerializeField]
    private Button m_BtnMinus;
    [SerializeField]
    private Button m_BtnPlus;
    
    [SerializeField]
    private Button m_BtnRevert;
    [SerializeField]
    private Button m_BtnCancel;
    [SerializeField]
    private Button m_BtnSave;
    
    #endregion

    //拖动对象列表
    private List<UISettingDragObject> m_DragObjectList = new List<UISettingDragObject>();
    //限制区域对象列表
    private List<UISettingLimitAreaObject> m_LimitAreaList = new List<UISettingLimitAreaObject>();
    
    private RectTransform m_SelectTransform;
    private UISettingDragObject m_SelectDragObject;
    private ResizeConfigData m_CurResizeConfigData;
    private ResizeConfigData m_SrcResizeConfigData;

    private Action<ResizeConfigData> m_OnSaveAction;
    
    /// <summary>
    /// 绑定回调
    /// </summary>
    public void BindCallback(Action<ResizeConfigData> saveAction)
    {
        m_OnSaveAction = saveAction;
    }
    
    /// <summary>
    /// 打开界面
    /// </summary>
    /// <param name="resizeConfigData">配置数据</param>
    public void Open(ResizeConfigData resizeConfigData)
    {
        m_CurResizeConfigData = resizeConfigData;
        //复制一份数据出来存储，用于还原
        m_SrcResizeConfigData = m_CurResizeConfigData.CopySelf();

        gameObject.SetActive(true);
        AddEventListeners();
        RefreshUI();
    }

    private void AddEventListeners()
    {
        m_BtnMinus.onClick.AddListener(OnBtnMinusClick);
        m_BtnPlus.onClick.AddListener(OnBtnPlusClick);
        m_BtnRevert.onClick.AddListener(OnBtnRevertClick);
        m_BtnCancel.onClick.AddListener(OnBtnCancelClick);
        m_BtnSave.onClick.AddListener(OnBtnSaveClick);
    }

    private void RefreshUI()
    {
        ClearSelectItems();
        SpawnItems();
        SetLimitRelation();
        BindCallback();
    }

    private void ClearSelectItems()
    {
        m_SelectCover.gameObject.SetActive(false);
        m_SelectTransform = null;
        m_SelectDragObject = null;
    }

    //绑定回调
    private void BindCallback()
    {
        foreach (UISettingDragObject dragObject in m_DragObjectList)
        {
            dragObject.SetUpdateBtnCallback(OnDragObjectUpdate);
            dragObject.onClick.AddListener(OnDragObjectClick);
        }
    }

    //选择回调
    private void OnDragObjectClick(UISettingDragObject dragObject)
    {
        UpdateSelectCoverUI(dragObject);
    }

    private void OnDragObjectUpdate(UISettingDragObject dragObject)
    {
        UpdateSelectCoverUI(dragObject);
    }

    //设置限制关系
    private void SetLimitRelation()
    {
        foreach (UISettingDragObject dragObject in m_DragObjectList)
        {
            int id = dragObject.ConfigNode.id;
            List<RectTransform> limitAreas = new List<RectTransform>();
            foreach (UISettingLimitAreaObject limitAreaObject in m_LimitAreaList)
            {
                if (limitAreaObject.ConfigNode.limitIds.Contains(id))
                {
                    limitAreas.Add(limitAreaObject.transform as RectTransform);
                }
            }
            
            //设置拖动对象的限制区域
            dragObject.SetLimitRectTransList(limitAreas);
        }
    }

    //根据配置生成ui元素对象
    private void SpawnItems()
    {
        SpawnButtonDragObjects();
        SpawnLimitAreaObjects();
    }

    //生成限制区域
    private void SpawnLimitAreaObjects()
    {
        m_LimitAreaList.Clear();
        ClearChildren(m_LimitAreaContainer);

        foreach (LimitAreaConfigNode configNode in m_CurResizeConfigData.limitAreaConfigNodes)
        {
            UISettingLimitAreaObject limitAreaObject = Instantiate(m_LimitAreaObjectPrefab,m_LimitAreaContainer);
            limitAreaObject.gameObject.SetActive(true);
            m_LimitAreaList.Add(limitAreaObject);
            
            limitAreaObject.SetConfigData(configNode);
        }
    }

    //生成拖动对象
    private void SpawnButtonDragObjects()
    {
        m_DragObjectList.Clear();
        ClearChildren(m_DragObjectContainer);

        foreach (DragObjectConfigNode configNode in m_CurResizeConfigData.buttonConfigNodes.Values)
        {
            UISettingDragObject dragObject = Instantiate(m_DragObjectPrefab,m_DragObjectContainer);
            dragObject.gameObject.SetActive(true);
            m_DragObjectList.Add(dragObject);
            
            dragObject.SetConfigData(configNode);
        }
    }

    /// <summary>
    /// 清理子元素
    /// </summary>
    /// <param name="trans"></param>
    private void ClearChildren(Transform trans)
    {
        int childNum = trans.childCount;
        for (int i = childNum -1;i>=0;i--)
        {
            GameObject.DestroyImmediate(trans.GetChild(i).gameObject);
        }
        trans.DetachChildren();
    }

    //更新选中覆盖UI
    private void UpdateSelectCoverUI(UISettingDragObject dragObject)
    {
        if (dragObject == null) return;
        m_SelectCover.gameObject.SetActive(true);
        m_SelectDragObject = dragObject;
        m_SelectTransform = dragObject.GetComponent<RectTransform>();
        var sizeDelta = m_SelectTransform.sizeDelta;
        m_SelectCover.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y);
        m_SelectCover.pivot = m_SelectTransform.pivot;
        m_SelectCover.localPosition = m_SelectTransform.localPosition;
        
        //显示加减号
        m_BtnMinus.gameObject.SetActive(m_SelectCover.sizeDelta.x > dragObject.ConfigNode.sizeMin);
        m_BtnPlus.gameObject.SetActive(m_SelectCover.sizeDelta.x < dragObject.ConfigNode.sizeMax);
    }

    private void OnBtnPlusClick()
    {
        ChangeSelectDragObjectSize(true);
    }
    
    private void OnBtnMinusClick()
    {
        ChangeSelectDragObjectSize(false);
    }

    private void ChangeSelectDragObjectSize(bool isAdd)
    {
        if (m_SelectDragObject == null) return;
        
        //改变尺寸
        float newSize;
        if (isAdd)
        {
            newSize = m_SelectDragObject.ConfigNode.size + m_SelectDragObject.ConfigNode.sizeStep;
        }
        else
        {
            newSize = m_SelectDragObject.ConfigNode.size - m_SelectDragObject.ConfigNode.sizeStep;
        }
        
        newSize = Mathf.Clamp(newSize, m_SelectDragObject.ConfigNode.sizeMin, m_SelectDragObject.ConfigNode.sizeMax);
        m_SelectDragObject.ConfigNode.size = newSize;
        //刷新UI
        m_SelectDragObject.RefreshUI();
        UpdateSelectCoverUI(m_SelectDragObject);
    }
    
    private void OnBtnSaveClick()
    {
        Debug.Log("保存新数据！");
        m_SrcResizeConfigData = m_CurResizeConfigData.CopySelf();
        
        //执行回调
        m_OnSaveAction?.Invoke(m_CurResizeConfigData);
        
        Close();
    }
    
    private void OnBtnCancelClick()
    {
        Debug.Log("关闭界面，不修改数据！");
        Close();
    }

    private void OnBtnRevertClick()
    {
        Debug.Log("还原数据!重置UI");
        m_CurResizeConfigData = m_SrcResizeConfigData.CopySelf();
        RefreshUI();
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}
