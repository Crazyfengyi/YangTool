using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 支持嵌套滚动区域的自定义滚动组件--消息传递--继承Scroll Rect重写传递消息给父节点 
/// </summary>
public class CustomScrollRect : ScrollRect
{
    // 父级滚动区域
    private ScrollRect m_Parent;

    // 当前拖拽是否转发给父级
    private bool m_ForwardDragToParent;

    /// <summary>
    /// 滚动方向
    /// </summary>
    public enum Direction
    {
        Horizontal,
        Vertical
    }

    // 当前组件的主要滚动方向
    private Direction m_Direction = Direction.Horizontal;

    // 当前拖拽操作方向
    private Direction m_BeginDragDirection = Direction.Horizontal;

    /// <summary>
    /// 初始化滚动组件
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        CacheParentScrollRect();
        RefreshDirection();
    }

    /// <summary>
    /// 启用滚动组件并刷新父级引用
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        CacheParentScrollRect();
        RefreshDirection();
        m_ForwardDragToParent = false;
    }

    /// <summary>
    /// 禁用滚动组件时清理拖拽状态
    /// </summary>
    protected override void OnDisable()
    {
        m_ForwardDragToParent = false;
        base.OnDisable();
    }

    /// <summary>
    /// 开始拖拽滚动区域
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        RefreshDirection();
        m_BeginDragDirection = GetEventDirection(eventData);
        m_ForwardDragToParent = ShouldForwardToParent(m_BeginDragDirection);

        if (m_ForwardDragToParent)
        {
            ExecuteEvents.Execute(m_Parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
            return;
        }

        base.OnBeginDrag(eventData);
    }

    /// <summary>
    /// 处理拖拽滚动
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    public override void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        if (m_ForwardDragToParent && m_Parent != null)
        {
            ExecuteEvents.Execute(m_Parent.gameObject, eventData, ExecuteEvents.dragHandler);
            return;
        }

        base.OnDrag(eventData);
    }

    /// <summary>
    /// 结束拖拽滚动区域
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (eventData == null)
        {
            m_ForwardDragToParent = false;
            return;
        }

        if (m_ForwardDragToParent && m_Parent != null)
        {
            ExecuteEvents.Execute(m_Parent.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
        else if (!m_ForwardDragToParent)
        {
            base.OnEndDrag(eventData);
        }

        m_ForwardDragToParent = false;
    }

    /// <summary>
    /// 处理滚轮或触控板滚动
    /// </summary>
    /// <param name="eventData">滚动事件数据</param>
    public override void OnScroll(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        RefreshDirection();
        Direction scrollDirection = GetEventDirection(eventData.scrollDelta);
        if (ShouldForwardToParent(scrollDirection))
        {
            ExecuteEvents.Execute(m_Parent.gameObject, eventData, ExecuteEvents.scrollHandler);
            return;
        }

        base.OnScroll(eventData);
    }

    /// <summary>
    /// 缓存最近的父级滚动区域
    /// </summary>
    private void CacheParentScrollRect()
    {
        Transform parent = transform.parent;
        m_Parent = parent == null ? null : parent.GetComponentInParent<ScrollRect>();
    }

    /// <summary>
    /// 刷新当前组件的滚动方向
    /// </summary>
    private void RefreshDirection()
    {
        m_Direction = horizontal ? Direction.Horizontal : Direction.Vertical;
    }

    /// <summary>
    /// 判断指定方向是否需要转发给父级
    /// </summary>
    /// <param name="eventDirection">输入事件方向</param>
    /// <returns>是否转发给父级</returns>
    private bool ShouldForwardToParent(Direction eventDirection)
    {
        if (m_Parent == null || (horizontal && vertical))
        {
            return false;
        }

        return eventDirection != m_Direction;
    }

    /// <summary>
    /// 获取拖拽事件的主要方向
    /// </summary>
    /// <param name="eventData">拖拽事件数据</param>
    /// <returns>拖拽方向</returns>
    private static Direction GetEventDirection(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return Direction.Horizontal;
        }

        Vector2 delta = eventData.delta;
        if (delta.sqrMagnitude <= Mathf.Epsilon)
        {
            delta = eventData.position - eventData.pressPosition;
        }

        return GetEventDirection(delta);
    }

    /// <summary>
    /// 获取输入位移的主要方向
    /// </summary>
    /// <param name="delta">输入位移</param>
    /// <returns>输入方向</returns>
    private static Direction GetEventDirection(Vector2 delta)
    {
        return Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) ? Direction.Horizontal : Direction.Vertical;
    }
}
