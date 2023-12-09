/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       WIN-VJ19D9AB7HB 
 *UnityVersion：2023.2.0b16 
 *创建时间:         2023-12-10 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using DG.Tweening;

public class CustomButton : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,IPointerDownHandler, IPointerUpHandler
{
    [BoxGroup("基础设置")]
    [SerializeField, LabelText("默认数据索引号")]
    [Tooltip("如果这个节点被无限滚动列表所引用，那么这个值是无效的。但是如果没有，那么DataIndex等于这个值.（通常用于提前做好的Toggle）")]
    private int defaultDataIndex;
    private int? m_overrideDataIndex;

    [BoxGroup("基础设置")]
    [SerializeField, LabelText("是否可交互")]
    protected bool m_interactable = true;
    protected virtual bool Interactable => m_interactable;

    [ToggleGroup(nameof(hasButtonClick), "按钮点击功能")]
    [SerializeField]
    protected bool hasButtonClick = false;

    [SerializeField, ToggleGroup(nameof(hasButtonClick))]
    [LabelText("按键响应")]
    [Tooltip("指定当什么按钮点击时,能触发点击事件.")]
    protected PointerEventData.InputButton interactButton = PointerEventData.InputButton.Left;

    [SerializeReference, ToggleGroup(nameof(hasButtonClick))]
    [LabelText("点击行为列表")]
    [Tooltip("当按钮按下时,列表中所有动画状态会同时执行.")]
    private IUIFadeState[] clickUIElements;

    [SerializeField, ToggleGroup(nameof(hasButtonClick))]
    [LabelText("点击事件")]
    [Tooltip("当触发点击行为的时候,会执行的事件.")]
    public UnityEvent onClickEvent;

    [SerializeField, ToggleGroup(nameof(hasButtonClick))]
    [LabelText("点击事件")]
    [Tooltip("当触发点击行为的时候,会执行的事件.传递EventData")]
    public UnityEvent<PointerEventData> onClickEventWithData;

    [ShowInInspector, HideInEditorMode, ReadOnly]
    public bool IsPressing { get; private set; }


    [ToggleGroup(nameof(hasRedDot), "红点功能"), SerializeField]
    protected bool hasRedDot = false;

    [SerializeField, ToggleGroup(nameof(hasRedDot))]
    private Image redDotDisplay;

    [SerializeField, ToggleGroup(nameof(hasRedDot))]
    private bool removeRedDotWhenHover;

    [SerializeField, ToggleGroup(nameof(hasRedDot))]
    private bool removeRedDotWhenClick;
    public int DataIndex
    {
        get => m_overrideDataIndex ?? defaultDataIndex;
        set => m_overrideDataIndex = value;
    }


    [ShowInInspector, HideInEditorMode]
    private int[] builtRedDotPath;

    private bool isDisplayingRedDot;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (hasRedDot)
        {
            isDisplayingRedDot = RefreshUnreadMessageState();
        }
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        if (IsPressing)
        {
            IsPressing = false;

            if (Interactable && hasButtonClick && clickUIElements != null)
            {
                foreach (var uiElement in clickUIElements)
                {
                    uiElement.OnDisable();
                }
            }
        }

        if (hasRedDot)
        {
            isDisplayingRedDot = false;
        }
    }
    protected virtual void Update()
    {
        var dt = Time.deltaTime;

        //TODO 点击按钮后马上设置按钮为不可点击，动画会终止
        if (Interactable && hasButtonClick && clickUIElements != null)
        {
            var target = IsPressing ? 1 : 0;

            foreach (var uiElement in clickUIElements)
            {
                uiElement.UpdateAlpha(target, dt);
            }
        }

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        //避免滚动过程中，还一直触发HoverEffect
        if (eventData.dragging) return;

        if (hasRedDot && removeRedDotWhenHover)
        {
            MarkMessageRead();
            RefreshUnreadMessageState();
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (IsPressing)
        {
            IsPressing = false;

            if (Interactable && hasButtonClick && clickUIElements != null)
            {
                foreach (var uiElement in clickUIElements)
                {
                    uiElement.ResetDampingVelocities();
                }
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPressing)
        {
            IsPressing = true;

            if (Interactable && hasButtonClick && clickUIElements != null)
            {
                foreach (var uiElement in clickUIElements)
                {
                    uiElement.ResetDampingVelocities();
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Interactable && hasButtonClick && eventData.button == interactButton
               && eventData.eligibleForClick && !eventData.dragging)
        {
            if (hasRedDot && removeRedDotWhenClick)
            {
                MarkMessageRead();
                RefreshUnreadMessageState();
            }

            onClickEvent.Invoke();
            onClickEventWithData.Invoke(eventData);
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {

    }


    public void SimulateClick()
    {
        if (Interactable && hasButtonClick)
        {
            if (hasRedDot && removeRedDotWhenClick)
            {
                MarkMessageRead();
                RefreshUnreadMessageState();
            }

            onClickEvent.Invoke();
        }
    }

    private bool RefreshUnreadMessageState()
    {
        if (hasRedDot)
        {
            builtRedDotPath ??= GenerateUnreadMessageSystemPath();
            var has = UIRedDotManager.Instance.HasUnreadMessage(builtRedDotPath, DataIndex);
            redDotDisplay.enabled = has;
            return has;
        }

        return false;
    }

    private void MarkMessageRead()
    {
        if (hasRedDot && isDisplayingRedDot && UIRedDotManager.DirectInstance != null)
        {
            builtRedDotPath ??= GenerateUnreadMessageSystemPath();
            UIRedDotManager.DirectInstance.RemoveUnreadMessage(builtRedDotPath, DataIndex);
        }
    }
    //生成未读消息系统路径
    protected virtual int[] GenerateUnreadMessageSystemPath() { return null; }
}

public interface IUIFadeState
{
    void UpdateAlpha(int targetAlpha, float deltaTime);
    void OnDisable();
    void ResetDampingVelocities();
}

[Serializable]
internal sealed class ScaleFade : IUIFadeState
{
    [SerializeField] private RectTransform targetTransform;
    [SerializeField] private float targetScale = 0.95f;
    [SerializeField] private float duration = 0.05f;
    private float velocity;
    private Vector3 kEpsilonVec = new Vector3(Vector3.kEpsilon, Vector3.kEpsilon, Vector3.kEpsilon);
    private Vector3 originScale = new Vector3(Vector3.kEpsilon, Vector3.kEpsilon, Vector3.kEpsilon);
    float tempScale = float.Epsilon;

    private const float minAccurate = 0.00001f;

    void IUIFadeState.UpdateAlpha(int targetAlpha, float deltaTime)
    {
        if (originScale == kEpsilonVec)
        {
            originScale = targetTransform.localScale;
        }
        var currentIntensity = Mathf.InverseLerp(1.0f, targetScale, tempScale == float.Epsilon ? targetTransform.localScale.x : tempScale);
        if (Mathf.Abs(targetAlpha - currentIntensity) > minAccurate)
        {
            currentIntensity = Mathf.SmoothDamp(currentIntensity, targetAlpha, ref velocity,
                duration, float.PositiveInfinity, deltaTime);

            var s = Mathf.Lerp(1.0f, targetScale, currentIntensity);
            if (s != tempScale)
            {
                tempScale = s;
                var scale = originScale.x - (originScale.x - targetScale) * currentIntensity;
                targetTransform.localScale = new Vector3(scale, scale, scale);
            }
            // targetTransform.localScale = new Vector3(s, s, s);
            // targetTransform.localScale = originScale * s;
        }
    }

    void IUIFadeState.OnDisable()
    {
        velocity = 0.0f;
        if (targetTransform == null) return;
        targetTransform.localScale = originScale;
        tempScale = float.Epsilon;
    }

    void IUIFadeState.ResetDampingVelocities()
    {
        velocity = 0.0f;
    }
}