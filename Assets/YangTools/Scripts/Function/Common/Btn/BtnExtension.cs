using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

public class BtnExtension : Button
{
    #region 单机双击
 
    [SerializeField]
    public bool singleClickEnabled = true;
    public bool doubleClickEnabled = false;
    public float doubleClickTime = 0.3f;
    
    private float lastClickTime = float.NegativeInfinity;
    private int clickCount = 0;
    
    //TODO:unity面板序列化显示
    [SerializeField]
    public ButtonClickedEvent doubleClickEvent = new ();
    
    public ButtonClickedEvent OnDoubleClick
    {
        get => doubleClickEvent;
        set => doubleClickEvent = value;
    }
 
    public override void OnPointerClick(PointerEventData eventData)
    {
        if(!IsActive() && !interactable) 
            return;
 
        if (singleClickEnabled)
        {
            UISystemProfilerApi.AddMarker("Button.onClick", this);
            onClick?.Invoke();
            Debug.Log("单击");   
        }
 
        if (doubleClickEnabled)
        {
            clickCount++;
            if (clickCount >= 2)
            {
                if (Time.realtimeSinceStartup - lastClickTime < doubleClickTime)
                {
                    UISystemProfilerApi.AddMarker("Button.onDoubleClick", this);
                    OnDoubleClick?.Invoke();
                    Debug.Log("双击");
                    lastClickTime = float.NegativeInfinity;
                    clickCount = 0;   
                }
                else
                {
                    clickCount = 1;
                    lastClickTime = Time.unscaledTime;
                }
            }
            else
            {
                lastClickTime = Time.unscaledTime;
            }
        }
    }
 
    #endregion
 
    #region 长按
 
    public bool longPressEnabled = false;
    public float minPressTime = 0.5f;
 
    private float lastPressTime = 0;
    private bool isPressing = false;
    private bool hasInvokedLongPress = false;
 
    [SerializeField]
    private ButtonClickedEvent longPressEvent = new();
    
    public ButtonClickedEvent OnLongPress
    {
        get => longPressEvent;
        set => longPressEvent = value;
    }
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (longPressEnabled)
        {
            hasInvokedLongPress = false;
            isPressing = true;
            lastPressTime = Time.unscaledTime;   
        }
    }
 
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isPressing = false;
        hasInvokedLongPress = false;
    }
 
    private void Update()
    {
        DealLongPress();
    }
    
    private void DealLongPress()
    {
        if(hasInvokedLongPress) return;
        if (isPressing)
        {
            if (Time.unscaledTime - lastPressTime >= minPressTime)
            {
                UISystemProfilerApi.AddMarker("Button.onLongPress", this);
                OnLongPress?.Invoke();
                hasInvokedLongPress = true;
                Debug.Log("执行长按事件");
            }
        }
    }
    #endregion

    #region 动画

    public bool useSelfAni;
    private Transform aniTrans;
    /// <summary>
    /// 状态转换
    /// </summary>
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (!useSelfAni)
        {
            base.DoStateTransition(state, instant);
        }
        else
        {
            if (aniTrans == null) aniTrans = GetComponent<RectTransform>();
            aniTrans.DOKill(true);

            switch (state)
            {
                case SelectionState.Normal:
                    aniTrans.DOScale(1, 0.26f);
                    break;
                case SelectionState.Highlighted:
                    aniTrans.DOScale(1.1f, 0.26f);
                    break;
                case SelectionState.Pressed:
                    aniTrans.DOScale(1.2f, 0.26f);
                    break;
                case SelectionState.Selected:
                    aniTrans.DOScale(1f, 0.26f);
                    break;
                case SelectionState.Disabled:
                    aniTrans.DOScale(1f, 0f);
                    break;
                default:
                    break;
            }
        }
    }

    #endregion
}