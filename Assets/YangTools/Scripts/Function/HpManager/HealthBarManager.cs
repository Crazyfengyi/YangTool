using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 通用血条管理器，负责注册、跟随、显示、隐藏和回收血条实例。
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
    [LabelText("是否在生命值满时隐藏生命值条")]
    [SerializeField] private bool hideWhenFullHealth = true;
    [LabelText("生命值条显示持续时间(在数值变化后)")]
    [SerializeField] private float showDurationAfterChange = 5f;
    [LabelText("初始对象池大小")]
    [SerializeField] private int initialPoolSize = 8;
    [LabelText("默认的世界坐标偏移量")]
    [SerializeField] private Vector3 defaultWorldOffset = new Vector3(0f, 2f, 0f);

    // 存储数据源与生命值条绑定的字典
    // 键：实现了IHealthBarDataSource接口的对象
    // 值：Binding对象，包含绑定相关信息
    private readonly Dictionary<IHealthBarDataSource, Binding> bindings =
        new Dictionary<IHealthBarDataSource, Binding>();

    // 存储可重用的生命值条对象的栈（对象池）
    private readonly Stack<HealthBarUI> pool = new Stack<HealthBarUI>();

    private sealed class Binding
    {
        public IHealthBarDataSource Source;
        public HealthBarUI View;
        public HealthBarDisplayData Data;
        public float VisibleUntil;
    }

    /// <summary>
    /// 初始化摄像机、根节点和血条对象池。
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
    /// 每帧更新血条世界坐标和可见性。
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
    /// 注册血条目标。
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
    /// 注销血条目标并回收其 UI 实例。
    /// </summary>
    public void UnregisterTarget(IHealthBarDataSource target)
    {
        if (target == null || !bindings.TryGetValue(target, out Binding binding)) return;
        target.OnHealthBarDataChanged -= OnTargetDataChanged;
        bindings.Remove(target);
        RecycleView(binding.View);
    }

    /// <summary>
    /// 手动显示指定目标的血条。
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
    /// 手动隐藏指定目标的血条。
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
    /// 立即从数据源刷新指定目标。
    /// </summary>
    public void RefreshTarget(IHealthBarDataSource target)
    {
        if (bindings.TryGetValue(target, out Binding binding)) RefreshBinding(binding, true);
    }

    /// <summary>
    /// 释放所有已注册目标和对象池实例。
    /// </summary>
    private void OnDestroy()
    {
        foreach (var pair in bindings) pair.Key.OnHealthBarDataChanged -= OnTargetDataChanged;
        bindings.Clear();
        pool.Clear();
    }

    private void OnTargetDataChanged()
    {
        foreach (var pair in bindings)
        {
            if (pair.Key == null) continue;
            RefreshBinding(pair.Value, false);
        }
    }

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

    private void UpdateVisibility(Binding binding, bool respectTimer)
    {
        bool visible = !hideWhenFullHealth || binding.VisibleUntil > Time.unscaledTime;
        binding.View.SetVisible(visible);
    }

    /// <summary>
    /// 判断目标是否处于满血状态。
    /// </summary>
    private static bool IsFullHealth(HealthBarDisplayData data)
    {
        return data == null || data.CurrentHealth >= data.MaxHealth;
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < Mathf.Max(0, initialPoolSize); i++) pool.Push(CreateView());
    }

    private HealthBarUI GetView()
    {
        HealthBarUI view = pool.Count > 0 ? pool.Pop() : CreateView();
        view.gameObject.SetActive(true);
        return view;
    }

    private HealthBarUI CreateView()
    {
        HealthBarUI view = Instantiate(healthBarPrefab, healthBarRoot);
        view.ResetView();
        return view;
    }

    private void RecycleView(HealthBarUI view)
    {
        if (view == null) return;
        view.ResetView();
        pool.Push(view);
    }
}
