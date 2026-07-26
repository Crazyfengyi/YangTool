using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 血条 Buff 图标条目。
/// </summary>
public class HealthBarBuffItem : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text stackText;
    [SerializeField] private TMP_Text remainingTimeText;

    /// <summary>
    /// 自动查找预制体中的 Buff 子节点引用。
    /// </summary>
    private void Awake()
    {
        if (icon == null) icon = transform.Find("Icon")?.GetComponent<Image>();
        if (stackText == null) stackText = transform.Find("StackText")?.GetComponent<TMP_Text>();
        if (remainingTimeText == null) remainingTimeText = transform.Find("RemainingTime")?.GetComponent<TMP_Text>();
    }

    /// <summary>
    /// 刷新 Buff 图标、层数和剩余时间。
    /// </summary>
    public void Refresh(HealthBarBuffData data)
    {
        if (data == null) return;

        if (icon != null)
        {
            icon.sprite = data.Icon;
            icon.enabled = data.Icon != null;
        }

        if (stackText != null)
        {
            stackText.text = data.StackCount > 1 ? data.StackCount.ToString() : string.Empty;
        }

        if (remainingTimeText != null)
        {
            remainingTimeText.text = data.RemainingTime >= 0f ? Mathf.CeilToInt(data.RemainingTime).ToString() : string.Empty;
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 清理条目显示状态。
    /// </summary>
    public void Clear()
    {
        if (icon != null) icon.sprite = null;
        if (stackText != null) stackText.text = string.Empty;
        if (remainingTimeText != null) remainingTimeText.text = string.Empty;
        gameObject.SetActive(false);
    }
}
