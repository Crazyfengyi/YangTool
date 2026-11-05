//****************************************
//作者：ChenHua
//邮箱：1184923569@qq.com
//创建时间：2022年08月22日 星期一 15:43:53
//****************************************

using System;
using System.Collections.Generic;
using ResizeSettingLib.Core.Config;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 设置-可拖动对象
/// </summary>
public class UISettingDragObject : MonoBehaviour,IPointerDownHandler,IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public UnityEvent<UISettingDragObject> onClick = new UnityEvent<UISettingDragObject>();

    [SerializeField]
    private Image m_Icon;
    [SerializeField]
    private Camera m_UICamera;

    public RectTransform RectfTrans;
    
    private Vector2 m_StartPoint;
    private Vector2 m_MouseOffset;
    private RectTransform m_CanvasTrans;
    private List<RectTransform> m_LimitRectTransList;
    private Action<UISettingDragObject> m_UpdateBtnCallback;

    private DragObjectConfigNode m_ConfigNode;
    public DragObjectConfigNode ConfigNode => m_ConfigNode;
    
    private void Awake()
    {
        RectfTrans = transform.GetComponent<RectTransform>();
        m_CanvasTrans = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    public void SetConfigData(DragObjectConfigNode configNode)
    {
        m_ConfigNode = configNode;

        RefreshUI();
    }

    public void RefreshUI()
    {
        gameObject.name = m_ConfigNode.name;
        SetAnchor();
        RectfTrans.anchoredPosition = m_ConfigNode.anchorPosition;
        RectfTrans.sizeDelta = new Vector2(m_ConfigNode.size, m_ConfigNode.size);
        m_Icon.sprite = m_ConfigNode.sprite;
    }

    //设置锚点
    private void SetAnchor()
    {
        switch (m_ConfigNode.anchorType)
        {
            case AnchorTypeEnum.LeftDown:
                RectfTrans.anchorMin = new Vector2(0, 0);
                RectfTrans.anchorMax = new Vector2(0, 0);
                break;
            case AnchorTypeEnum.LeftTop:
                RectfTrans.anchorMin = new Vector2(0, 1);
                RectfTrans.anchorMax = new Vector2(0, 1);
                break;
            case AnchorTypeEnum.RightTop:
                RectfTrans.anchorMin = new Vector2(1, 1);
                RectfTrans.anchorMax = new Vector2(1, 1);
                break;
            case AnchorTypeEnum.RightDown:
                RectfTrans.anchorMin = new Vector2(1, 0);
                RectfTrans.anchorMax = new Vector2(1, 0);
                break;
        }
    }

    /// <summary>
    /// 设置禁用区域
    /// </summary>
    /// <param name="list"></param>
    public void SetLimitRectTransList(List<RectTransform> list)
    {
        m_LimitRectTransList = list;
    }

    /// <summary>
    /// 设置更新按钮回调
    /// </summary>
    /// <param name="action"></param>
    public void SetUpdateBtnCallback(Action<UISettingDragObject> action)
    {
        m_UpdateBtnCallback = action;
    }
    
    /// <summary>
    /// 鼠标按下
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        Vector2 mouseUIPos = new Vector2();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasTrans,mousePos,m_UICamera,out mouseUIPos))
        {
            m_MouseOffset = RectfTrans.anchoredPosition - mouseUIPos;
        }
    }

    /// <summary>
    /// 拖动过程中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        //检测是否在禁止区域内
        // if (IsInLimitAreaList())
        // {
        //     m_UpdateBtnCallback?.Invoke(this);
        //     return;
        // }
        
        Vector2 uiPoint = new Vector2();
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasTrans, eventData.position, m_UICamera, out uiPoint))
        {
            RectfTrans.anchoredPosition = uiPoint + m_MouseOffset;
            m_ConfigNode.anchorPosition = RectfTrans.anchoredPosition;
            
            m_UpdateBtnCallback?.Invoke(this);
        }
    }

    /// <summary>
    /// 拖动开始
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        m_StartPoint = RectfTrans.anchoredPosition;
    }

    /// <summary>
    /// 拖动结束
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsInLimitAreaList())
        {
            RectfTrans.anchoredPosition = m_StartPoint;
            m_ConfigNode.anchorPosition = RectfTrans.anchoredPosition;
            
            m_UpdateBtnCallback?.Invoke(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick.Invoke(this);
    }

    //屏幕点是否在限制区域内
    private bool IsInLimitAreaList()
    {
        if (m_LimitRectTransList == null || m_LimitRectTransList.Count == 0)
        {
            return false;
        }
        
        Vector3[] corners = new Vector3[4];
        RectfTrans.GetWorldCorners(corners);

        var point1 = RectTransformUtility.WorldToScreenPoint(m_UICamera, corners[0]);
        var point2 = RectTransformUtility.WorldToScreenPoint(m_UICamera, corners[1]);
        var point3 = RectTransformUtility.WorldToScreenPoint(m_UICamera, corners[2]);
        var point4 = RectTransformUtility.WorldToScreenPoint(m_UICamera, corners[3]);

        foreach (RectTransform rect in m_LimitRectTransList)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect,point1,m_UICamera))
            {
                return true;
            }
            
            if (RectTransformUtility.RectangleContainsScreenPoint(rect,point2,m_UICamera))
            {
                return true;
            }
            
            if (RectTransformUtility.RectangleContainsScreenPoint(rect,point3,m_UICamera))
            {
                return true;
            }
            
            if (RectTransformUtility.RectangleContainsScreenPoint(rect,point4,m_UICamera))
            {
                return true;
            }
        }

        return false;
    }
}
