using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 通用血条管理器 负责注册 跟随 显示 隐藏和回收血条实例
/// </summary>
public class HealthBarManager : MonoBehaviour
{
    // 生命值条UI预制体
    [SerializeField] private HealthBarUI healthBarPrefab;
    // 生命值条UI的根节点RectTransform
    [SerializeField] private RectTransform healthBarRoot;
    // 目标相机，用于将世界坐标转换为屏幕坐标
    [SerializeField] private Camera targetCamera;
    // 血条所在的Canvas，用于兼容Overlay、Camera和World Space模式
    [SerializeField] private Canvas targetCanvas;
    // 是否在生命值满时隐藏生命值条
    [LabelText("是否在生命值满时隐藏生命值条")]
    [SerializeField] private bool hideWhenFullHealth = true;
    // 生命值条显示持续时间(在数值变化后)
    [LabelText("生命值条显示持续时间(在数值变化后)")]
    [SerializeField] private float showDurationAfterChange = 5f;
    // 初始对象池大小
    [LabelText("初始对象池大小")]
    [SerializeField] private int initialPoolSize = 8;
    // 默认的世界坐标偏移量
    [LabelText("默认的世界坐标偏移量")]
    [SerializeField] private Vector3 defaultWorldOffset = new Vector3(0f, 2f, 0f);

    // 存储数据源与生命值条绑定的字典
    // 键：实现了IHealthBarDataSource接口的对象
    // 值：Binding对象，包含绑定相关信息
    private readonly Dictionary<IHealthBarDataSource, Binding> bindings =
        new Dictionary<IHealthBarDataSource, Binding>();

    // 存储可重用的生命值条对象的栈（对象池）
    private readonly Stack<HealthBarUI> pool = new Stack<HealthBarUI>();

    /// <summary>
    /// 记录目标数据源 血条视图与临时显示截止时间
    /// </summary>
    private sealed class Binding
    {
        // 提供当前血条数据的目标
        public IHealthBarDataSource Source;
        // 当前目标绑定的血条视图
        public HealthBarUI View;
        // 上次刷新保存的数据副本
        public HealthBarDisplayData Data;
        // 按非缩放时间计算的临时显示截止时刻
        public float VisibleUntil;
    }

    /// <summary>
    /// 初始化摄像机 根节点和血条对象池
    /// </summary>
    private void Awake()
    {
        if (healthBarRoot == null) healthBarRoot = GetComponent<RectTransform>();
        if (targetCanvas == null && healthBarRoot != null)
        {
            targetCanvas = healthBarRoot.GetComponentInParent<Canvas>();
        }

        if (targetCamera == null && targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            targetCamera = targetCanvas.worldCamera;
        }

        if (targetCamera == null) targetCamera = Camera.main;
        if (healthBarRoot == null) healthBarRoot = transform as RectTransform;
        PrewarmPool();
    }

    /// <summary>
    /// 每帧更新血条世界坐标和可见性
    /// </summary>
    private void LateUpdate()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        foreach (var pair in bindings)
        {
            UpdateBinding(pair.Value);
        }
    }

    /// <summary>
    /// 注册血条目标
    /// </summary>
    public void RegisterTarget(IHealthBarDataSource target)
    {
        if (target == null || bindings.ContainsKey(target) || healthBarPrefab == null) return;

        var binding = new Binding
        {
            Source = target,
            View = GetView(),
            Data = target.GetHealthBarData()?.Clone()
        };
        if (binding.Data == null)
        {
            RecycleView(binding.View);
            return;
        }

        binding.View.Refresh(binding.Data);
        target.OnHealthBarDataChanged += OnTargetDataChanged;
        bindings.Add(target, binding);
        if (!IsFullHealth(binding.Data)) binding.VisibleUntil = Time.unscaledTime + showDurationAfterChange;
        UpdateVisibility(binding, false);
    }

    /// <summary>
    /// 注销血条目标并回收其 UI 实例
    /// </summary>
    public void UnregisterTarget(IHealthBarDataSource target)
    {
        if (target == null || !bindings.TryGetValue(target, out Binding binding)) return;
        target.OnHealthBarDataChanged -= OnTargetDataChanged;
        bindings.Remove(target);
        RecycleView(binding.View);
    }

    /// <summary>
    /// 手动显示指定目标的血条
    /// </summary>
    public void ShowHealthBar(IHealthBarDataSource target)
    {
        if (bindings.TryGetValue(target, out Binding binding))
        {
            binding.VisibleUntil = Time.unscaledTime + showDurationAfterChange;
            binding.View.SetVisible(true);
        }
    }

    /// <summary>
    /// 手动隐藏指定目标的血条
    /// </summary>
    public void HideHealthBar(IHealthBarDataSource target)
    {
        if (bindings.TryGetValue(target, out Binding binding))
        {
            binding.VisibleUntil = 0f;
            binding.View.SetVisible(false);
        }
    }

    /// <summary>
    /// 立即从数据源刷新指定目标
    /// </summary>
    public void RefreshTarget(IHealthBarDataSource target)
    {
        if (bindings.TryGetValue(target, out Binding binding)) RefreshBinding(binding, true);
    }

    /// <summary>
    /// 释放所有已注册目标和对象池实例
    /// </summary>
    private void OnDestroy()
    {
        foreach (var pair in bindings) pair.Key.OnHealthBarDataChanged -= OnTargetDataChanged;
        bindings.Clear();
        pool.Clear();
    }

    /// <summary>
    /// 在目标发出变化通知时刷新所有仍有效的绑定
    /// </summary>
    private void OnTargetDataChanged()
    {
        foreach (var pair in bindings)
        {
            if (pair.Key == null) continue;
            RefreshBinding(pair.Value, false);
        }
    }

    /// <summary>
    /// 复制目标数据并刷新视图 生命变化或强制刷新时延长显示时间
    /// </summary>
    /// <param name="binding">目标与血条视图的绑定记录</param>
    /// <param name="forceShow">是否重新延长临时显示时间</param>
    private void RefreshBinding(Binding binding, bool forceShow)
    {
        HealthBarDisplayData nextData = binding.Source.GetHealthBarData()?.Clone();
        if (nextData == null) return;

        bool healthChanged = !Mathf.Approximately(nextData.CurrentHealth, binding.Data.CurrentHealth);
        binding.Data = nextData;
        binding.View.Refresh(nextData);
        if (forceShow || healthChanged)
        {
            binding.VisibleUntil = Time.unscaledTime + showDurationAfterChange;
            binding.View.SetVisible(true);
        }
    }

    /// <summary>
    /// 将目标锚点投影到血条容器并隐藏相机背后或屏幕外的视图
    /// </summary>
    /// <param name="binding">目标与血条视图的绑定记录</param>
    private void UpdateBinding(Binding binding)
    {
        Transform anchor = binding.Source.HealthBarAnchor;
        if (targetCamera == null || anchor == null)
        {
            binding.View.SetVisible(false);
            return;
        }

        Vector3 worldPosition = anchor.position + defaultWorldOffset;
        Vector3 screenPosition = targetCamera.WorldToScreenPoint(worldPosition);
        bool inFront = screenPosition.z > 0f;
        bool inScreen = inFront && screenPosition.x >= 0f && screenPosition.x <= Screen.width &&
                        screenPosition.y >= 0f && screenPosition.y <= Screen.height;
        if (!inScreen)
        {
            binding.View.SetVisible(false);
            return;
        }

        RectTransform viewRect = binding.View.transform as RectTransform;
        if (healthBarRoot != null && viewRect != null)
        {
            Camera uiCamera = targetCanvas != null && targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : targetCanvas != null && targetCanvas.worldCamera != null
                    ? targetCanvas.worldCamera
                    : targetCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                healthBarRoot, screenPosition, uiCamera, out Vector2 localPosition);
            viewRect.anchoredPosition = localPosition;
        }

        UpdateVisibility(binding, true);
    }

    /// <summary>
    /// 根据显示策略和临时显示截止时间切换视图可见性
    /// </summary>
    /// <param name="binding">目标与血条视图的绑定记录</param>
    /// <param name="respectTimer">预留的计时策略参数 当前实现未使用</param>
    private void UpdateVisibility(Binding binding, bool respectTimer)
    {
        bool visible = !hideWhenFullHealth || binding.VisibleUntil > Time.unscaledTime;
        binding.View.SetVisible(visible);
    }

    /// <summary>
    /// 判断目标是否处于满血状态
    /// </summary>
    private static bool IsFullHealth(HealthBarDisplayData data)
    {
        return data == null || data.CurrentHealth >= data.MaxHealth;
    }

    /// <summary>
    /// 按配置数量创建并缓存初始血条视图
    /// </summary>
    private void PrewarmPool()
    {
        for (int i = 0; i < Mathf.Max(0, initialPoolSize); i++) pool.Push(CreateView());
    }

    /// <summary>
    /// 优先从缓存池取得视图 否则创建新视图并激活
    /// </summary>
    private HealthBarUI GetView()
    {
        HealthBarUI view = pool.Count > 0 ? pool.Pop() : CreateView();
        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// 实例化血条模板并重置显示状态
    /// </summary>
    private HealthBarUI CreateView()
    {
        HealthBarUI view = Instantiate(healthBarPrefab, healthBarRoot);
        view.ResetView();
        return view;
    }

    /// <summary>
    /// 重置有效视图并归还血条缓存池
    /// </summary>
    /// <param name="view">需要重置并归还的血条视图</param>
    private void RecycleView(HealthBarUI view)
    {
        if (view == null) return;
        view.ResetView();
        pool.Push(view);
    }
}
