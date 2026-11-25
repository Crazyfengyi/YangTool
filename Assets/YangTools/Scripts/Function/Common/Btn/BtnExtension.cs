using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 扩展按钮的单击 双击 长按和按压缩放交互
/// </summary>
public class BtnExtension : Button
{
    #region 单机双击
 
    // 是否响应单击事件
    [SerializeField]
    public bool singleClickEnabled = true;
    // 是否响应双击事件
    public bool doubleClickEnabled = false;
    // 判定双击的最大点击间隔秒数
    public float doubleClickTime = 0.3f;
    
    // 上次点击的非缩放时间
    private float lastClickTime = float.NegativeInfinity;
    // 当前双击判定累计点击次数
    private int clickCount = 0;
    
    //TODO:unity面板序列化显示
    [SerializeField]
    public ButtonClickedEvent doubleClickEvent = new ();
    
    public ButtonClickedEvent OnDoubleClick
    {
        get => doubleClickEvent;
        set => doubleClickEvent = value;
    }
 
    /// <summary>
    /// 按配置触发单击与双击事件 两类事件可能在同次点击触发
    /// </summary>
    /// <param name="eventData">本次指针交互数据</param>
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
 
    // 是否响应长按事件
    public bool longPressEnabled = false;
    // 触发长按所需的最短秒数
    public float minPressTime = 0.5f;
 
    // 本次按下的非缩放时间
    private float lastPressTime = 0;
    // 指针是否仍处于按下状态
    private bool isPressing = false;
    // 本次按压是否已触发长按
    private bool hasInvokedLongPress = false;
 
    // 达到长按阈值时的回调
    [SerializeField]
    private ButtonClickedEvent longPressEvent = new();
    
    public ButtonClickedEvent OnLongPress
    {
        get => longPressEvent;
        set => longPressEvent = value;
    }
    
    /// <summary>
    /// 记录按压开始时间并按配置播放按下缩放
    /// </summary>
    /// <param name="eventData">本次指针交互数据</param>
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
 
    /// <summary>
    /// 结束按压计时并按配置恢复缩放
    /// </summary>
    /// <param name="eventData">本次指针交互数据</param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        isPressing = false;
        hasInvokedLongPress = false;
    }
 
    /// <summary>
    /// 检查本次持续按压是否达到长按阈值
    /// </summary>
    private void Update()
    {
        DealLongPress();
    }
    
    /// <summary>
    /// 达到长按阈值后触发一次回调 直到松开前不重复触发
    /// </summary>
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

    // 是否启用自带的按压动画
    public bool useSelfAni;
    // 按压缩放作用的变换节点
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