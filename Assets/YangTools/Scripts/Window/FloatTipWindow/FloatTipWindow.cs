using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangUGUI;

/// <summary>
/// 浮动提示窗口类，用于显示游戏中的浮动提示信息
/// 继承自UGUIPanelBase，使用默认的UI数据结构
/// </summary>
public sealed class FloatTipWindow : UGUIPanelBase<DefaultUGUIDataBase>
{
    // 单例实例
    private static FloatTipWindow instance;

    // 窗口是否正在打开
    private static bool isOpening;

    // 待处理的提示请求队列
    private static readonly Queue<FloatTipRequest> pendingRequests = new Queue<FloatTipRequest>();

    // 内容区域的根节点
    [SerializeField] private RectTransform contentRoot;

    // 两种预制体，用于不同样式的提示
    public FloatTipItem prefab;

    public FloatTipItem prefab2;

    // 当前活动的提示项数量
    private int activeItemCount;

    // 窗口是否正在关闭
    private bool isClosing;

    /// <summary>
    /// 在屏幕中心上方显示浮动提示
    /// </summary>
    /// <param name="text">提示文本内容</param>
    public static void Show(string text)
    {
        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 screenPosition = center + new Vector2(0, 200);
        Show(text, screenPosition);
    }

    /// <summary>
    /// 在指定屏幕位置显示浮动提示
    /// </summary>
    /// <param name="text">提示文本内容</param>
    /// <param name="screenPosition">屏幕坐标位置</param>
    /// <param name="prefabId">使用的预制体ID，0表示使用第一个预制体，1表示使用第二个预制体</param>
    public static async void Show(string text, Vector2 screenPosition, int prefabId = 0)
    {
        // 如果文本为空，直接返回
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        // 创建新的提示请求
        FloatTipRequest request = new FloatTipRequest(text, screenPosition, prefabId);
        // 如果窗口实例已存在，直接生成提示
        if (instance != null)
        {
            instance.Spawn(request);
            return;
        }

        // 将请求加入队列
        pendingRequests.Enqueue(request);
        // 如果窗口正在打开，直接返回
        if (isOpening)
        {
            return;
        }

        // 标记窗口正在打开
        isOpening = true;
        try
        {
            // 异步打开窗口面板
            (int id, FloatTipWindow panel) result = await UIMonoInstance.OpenPanel<FloatTipWindow>(GroupType.Top);
            // 如果面板打开成功，处理待处理的请求
            if (result.panel != null)
            {
                result.panel.FlushPendingRequests();
            }
        }
        finally
        {
            // 无论成功与否，都重置打开状态
            isOpening = false;
        }
    }

    /// <summary>
    /// 窗口打开时的回调函数
    /// </summary>
    /// <param name="userData">用户数据</param>
    public override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        // 设置实例
        instance = this;
        // 重置活动项计数
        activeItemCount = 0;
        // 重置关闭状态
        isClosing = false;
        // 处理所有待处理的请求
        FlushPendingRequests();
    }

    /// <summary>
    /// 窗口关闭时的回调函数
    /// </summary>
    /// <param name="isShutdown">是否是关闭应用</param>
    /// <param name="userData">用户数据</param>
    public override void OnClose(bool isShutdown, object userData)
    {
        // 清除实例引用
        if (instance == this)
        {
            instance = null;
        }

        base.OnClose(isShutdown, userData);
    }

    /// <summary>
    /// 处理所有待处理的提示请求
    /// </summary>
    private void FlushPendingRequests()
    {
        // 依次处理队列中的所有请求
        while (pendingRequests.Count > 0)
        {
            Spawn(pendingRequests.Dequeue());
        }
    }

    /// <summary>
    /// 生成一个新的提示项
    /// </summary>
    /// <param name="request">提示请求信息</param>
    private void Spawn(FloatTipRequest request)
    {
        isClosing = false;

        FloatTipItem target = null;
        if (request.usePrefabId == 0)
        {
            target = prefab;
        }
        else
        {
            target = prefab2;
        }

        FloatTipItem itemObject = Instantiate(target, contentRoot);
        Vector2 anchoredPosition = ScreenToAnchoredPosition(request.ScreenPosition);
        activeItemCount++;
        itemObject.Play(request.Text, anchoredPosition, OnItemPlayComplete);
    }

    /// <summary>
    /// 飘字播放完成时尝试关闭窗口。
    /// </summary>
    private void OnItemPlayComplete()
    {
        activeItemCount = Mathf.Max(0, activeItemCount - 1);
        TryCloseWhenIdle();
    }

    /// <summary>
    /// 所有飘字播放完成后关闭窗口。
    /// </summary>
    private void TryCloseWhenIdle()
    {
        if (isClosing || activeItemCount > 0 || pendingRequests.Count > 0)
        {
            return;
        }

        isClosing = true;
        CloseSelfPanel();
    }

    /// <summary>
    /// 将屏幕坐标转换为锚点定位的本地坐标
    /// </summary>
    /// <param name="screenPosition">屏幕坐标位置</param>
    /// <returns>转换后的本地坐标位置</returns>
    private Vector2 ScreenToAnchoredPosition(Vector2 screenPosition)
    {
        // 获取父级对象中的Canvas组件
        Canvas canvas = GetComponentInParent<Canvas>();
        // 初始化相机变量为null
        Camera camera = null;
        // 如果Canvas存在且渲染模式不是ScreenSpaceOverlay
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // 获取Canvas关联的世界相机
            camera = canvas.worldCamera;
        }

        // 尝试将屏幕点转换为矩形区域的本地点
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(contentRoot, screenPosition, camera,
                out Vector2 localPoint))
        {
            // 如果转换成功，返回本地坐标
            return localPoint;
        }

        // 如果转换失败，返回原始屏幕坐标
        return screenPosition;
    }
}

/// <summary>
/// 只读结构体，用于存储浮点提示请求的信息
/// </summary>
readonly struct FloatTipRequest
{
    /// <summary>
    /// 构造函数，初始化浮点提示请求
    /// </summary>
    /// <param name="text">提示文本内容</param>
    /// <param name="screenPosition">屏幕位置坐标</param>
    /// <param name="prefabId">预制体ID，默认为0</param>
    public FloatTipRequest(string text, Vector2 screenPosition, int prefabId = 0)
    {
        Text = text;
        ScreenPosition = screenPosition;
        usePrefabId = prefabId;
    }

    /// <summary>
    /// 获取提示文本
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// 获取提示在屏幕上的位置
    /// </summary>
    public Vector2 ScreenPosition { get; }
    /// <summary>
    /// 获取或设置使用的预制体ID
    /// </summary>
    public int usePrefabId { get; }
}