using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 单个目标的血条显示组件，负责数据渲染和血量缓动动画。
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [Header("基础引用")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image delayedHealthFill;
    [SerializeField] private Image shieldFill;
    [SerializeField] private Transform statusRoot;
    [SerializeField] private Transform buffRoot;
    [SerializeField] private HealthBarBuffItem buffItemPrefab;
    [SerializeField] private List<GameObject> segmentMarkers = new List<GameObject>();

    [Header("显示配置")]
    [SerializeField] private Color highHealthColor = new Color(0.2f, 0.9f, 0.25f);
    [SerializeField] private Color middleHealthColor = new Color(1f, 0.75f, 0.1f);
    [SerializeField] private Color lowHealthColor = new Color(0.95f, 0.15f, 0.1f);
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private float delayedSmoothTime = 0.35f;
    [SerializeField] private int segmentCount = 10;

    private readonly List<HealthBarBuffItem> buffItems = new List<HealthBarBuffItem>();
    private float targetHealthAmount;
    private float currentHealthAmount;
    private float delayedHealthAmount;
    private float currentHealthVelocity;
    private float delayedHealthVelocity;
    private float lastHealthAmount = 1f;
    private bool hasRefreshData;

    /// <summary>
    /// 初始化血条引用并隐藏条目模板。
    /// </summary>
    private void Awake()
    {
        AutoBindReferences();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (buffItemPrefab != null) buffItemPrefab.gameObject.SetActive(false);
        currentHealthAmount = 1f;
        delayedHealthAmount = 1f;
    }

    /// <summary>
    /// 按约定的子节点名称自动绑定 UI 引用，减少预制体手动拖拽配置。
    /// </summary>
    private void AutoBindReferences()
    {
        if (nameText == null) nameText = transform.Find("Name")?.GetComponent<TMP_Text>();
        if (levelText == null) levelText = transform.Find("Level")?.GetComponent<TMP_Text>();
        if (statusRoot == null) statusRoot = transform.Find("StatusRoot");
        if (buffRoot == null) buffRoot = transform.Find("BuffRoot");
        if (buffItemPrefab == null) buffItemPrefab = transform.Find("BuffItemTemplate")?.GetComponent<HealthBarBuffItem>();

        Transform barRoot = transform.Find("BarRoot");
        if (barRoot != null)
        {
            if (healthFill == null) healthFill = barRoot.Find("HealthFill")?.GetComponent<Image>();
            if (delayedHealthFill == null) delayedHealthFill = barRoot.Find("DelayedHealthFill")?.GetComponent<Image>();
            if (shieldFill == null) shieldFill = barRoot.Find("ShieldFill")?.GetComponent<Image>();

            ConfigureFill(healthFill);
            ConfigureFill(delayedHealthFill);
            ConfigureFill(shieldFill);

            Transform segmentRoot = barRoot.Find("SegmentRoot");
            if (segmentRoot != null && segmentMarkers.Count == 0)
            {
                for (int i = 0; i < segmentRoot.childCount; i++) segmentMarkers.Add(segmentRoot.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// 将血量图片配置为水平填充模式。
    /// </summary>
    private static void ConfigureFill(Image image)
    {
        if (image == null) return;
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = 0;
        image.fillClockwise = true;
    }

    /// <summary>
    /// 每帧执行血量缓动。
    /// </summary>
    private void Update()
    {
        if (!hasRefreshData) return;

        currentHealthAmount = Mathf.SmoothDamp(currentHealthAmount, targetHealthAmount,
            ref currentHealthVelocity, smoothTime);
        delayedHealthAmount = Mathf.SmoothDamp(delayedHealthAmount, targetHealthAmount,
            ref delayedHealthVelocity, delayedSmoothTime);

        SetFillAmount(healthFill, currentHealthAmount);
        SetFillAmount(delayedHealthFill, delayedHealthAmount);
    }

    /// <summary>
    /// 绑定并刷新血条数据。
    /// </summary>
    public void Refresh(HealthBarDisplayData data)
    {
        if (data == null) return;

        float nextHealthAmount = Mathf.Clamp01(data.CurrentHealth / Mathf.Max(0.01f, data.MaxHealth));
        if (!hasRefreshData)
        {
            currentHealthAmount = nextHealthAmount;
            delayedHealthAmount = nextHealthAmount;
        }
        else if (nextHealthAmount < lastHealthAmount)
        {
            currentHealthAmount = nextHealthAmount;
            currentHealthVelocity = 0f;
        }

        targetHealthAmount = nextHealthAmount;
        lastHealthAmount = nextHealthAmount;
        hasRefreshData = true;

        if (nameText != null) nameText.text = data.DisplayName;
        if (levelText != null)
        {
            if (data.Level > 0) levelText.SetText("Lv.{0}", data.Level);
            else levelText.text = string.Empty;
            levelText.gameObject.SetActive(data.Level > 0);
        }

        RefreshHealthColor(nextHealthAmount);
        RefreshShield(data.Shield, data.MaxHealth);
        RefreshStatuses(data.Statuses);
        RefreshBuffs(data.Buffs);
        RefreshSegments();
    }

    /// <summary>
    /// 控制血条透明度和显示状态。
    /// </summary>
    public void SetVisible(bool visible, bool immediate = false)
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(visible);
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        if (immediate) gameObject.SetActive(visible);
    }

    /// <summary>
    /// 重置血条状态，供管理器回收到对象池。
    /// </summary>
    public void ResetView()
    {
        hasRefreshData = false;
        targetHealthAmount = 1f;
        currentHealthAmount = 1f;
        delayedHealthAmount = 1f;
        currentHealthVelocity = 0f;
        delayedHealthVelocity = 0f;
        ClearBuffs();
        ClearStatuses();
        SetVisible(false, true);
    }

    private void RefreshHealthColor(float amount)
    {
        if (healthFill == null) return;
        healthFill.color = amount <= 0.3f ? lowHealthColor : amount <= 0.7f ? middleHealthColor : highHealthColor;
    }

    private void RefreshShield(float shield, float maxHealth)
    {
        if (shieldFill == null) return;
        shieldFill.fillAmount = Mathf.Clamp01(shield / Mathf.Max(0.01f, maxHealth));
        shieldFill.gameObject.SetActive(shield > 0f);
    }

    private void RefreshStatuses(IList<HealthBarStatusData> statuses)
    {
        if (statusRoot == null) return;
        for (int i = 0; i < statusRoot.childCount; i++) statusRoot.GetChild(i).gameObject.SetActive(false);
        if (statuses == null) return;

        for (int i = 0; i < statuses.Count && i < statusRoot.childCount; i++)
        {
            var image = statusRoot.GetChild(i).GetComponent<Image>();
            if (image == null) continue;
            image.sprite = statuses[i].Icon;
            image.enabled = statuses[i].Icon != null;
            image.gameObject.SetActive(statuses[i].Icon != null);
        }
    }

    private void RefreshBuffs(IList<HealthBarBuffData> buffs)
    {
        ClearBuffs();
        if (buffs == null || buffRoot == null || buffItemPrefab == null) return;

        for (int i = 0; i < buffs.Count; i++)
        {
            HealthBarBuffItem item = i < buffItems.Count ? buffItems[i] : CreateBuffItem();
            item.Refresh(buffs[i]);
        }
    }

    private HealthBarBuffItem CreateBuffItem()
    {
        var item = Instantiate(buffItemPrefab, buffRoot);
        item.gameObject.SetActive(false);
        buffItems.Add(item);
        return item;
    }

    private void ClearBuffs()
    {
        for (int i = 0; i < buffItems.Count; i++) buffItems[i].Clear();
        if (buffItemPrefab != null) buffItemPrefab.gameObject.SetActive(false);
    }

    private void ClearStatuses()
    {
        if (statusRoot == null) return;
        for (int i = 0; i < statusRoot.childCount; i++) statusRoot.GetChild(i).gameObject.SetActive(false);
    }

    private void RefreshSegments()
    {
        int visibleCount = Mathf.Clamp(segmentCount, 0, segmentMarkers.Count);
        for (int i = 0; i < segmentMarkers.Count; i++)
        {
            if (segmentMarkers[i] != null) segmentMarkers[i].SetActive(i < visibleCount);
        }
    }

    private static void SetFillAmount(Image image, float value)
    {
        if (image != null) image.fillAmount = Mathf.Clamp01(value);
    }
}
