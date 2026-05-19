using TMPro;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 轮盘区块UI类
/// </summary>
public class RouletteWheelItemUI : MonoBehaviour
{

    // 序列化字段，用于在Inspector面板中设置UI元素的引用
    [SerializeField] private RectTransform root;          // 根节点RectTransform
    [SerializeField] private Image backgroundImage;     // 背景图片
    [SerializeField] private Image iconImage;           // 图标图片
    [SerializeField] private TextMeshProUGUI nameText;  // 名称文本

    // 数据字段
    private WheelItemData data;
    // 属性：提供对数据的只读访问
    public WheelItemData Data => data;

    /// <summary>
    /// 设置数据
    /// </summary>
    public void Init(WheelItemData itemData)
    {
        data = itemData;
        if (nameText != null)
        {
            nameText.text = itemData != null ? itemData.DisplayName : string.Empty;
        }
        if (iconImage != null)
        {
            iconImage.sprite = itemData != null ? itemData.Icon : null;
            iconImage.enabled = itemData != null && itemData.Icon != null;
        }
    }

    public void BindReferences(RectTransform root, Image backgroundImage, Image iconImage, TextMeshProUGUI nameText)
    {
        this.root = root;
        this.backgroundImage = backgroundImage;
        this.iconImage = iconImage;
        this.nameText = nameText;
    }

    public void SetLayout(
        float angle,
        float radius)
    {
        RectTransform rect = GetRoot();
        Vector2 direction = DirectionFromTopClockwise(angle);

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = direction * radius;
        rect.localRotation = Quaternion.Euler(0f, 0f, -angle);

        if (iconImage != null)
        {
            RectTransform iconRect = iconImage.rectTransform;
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            //iconRect.sizeDelta = iconSize;
            iconRect.anchoredPosition = Vector2.zero;
            //iconRect.localRotation = rotateTextWithSector ? Quaternion.identity : Quaternion.Euler(0f, 0f, angle);
        }

        if (nameText != null)
        {
            RectTransform textRect = nameText.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            //textRect.localRotation = rotateTextWithSector ? Quaternion.identity : Quaternion.Euler(0f, 0f, angle);
        }

        if (backgroundImage != null)
        {
            backgroundImage.transform.SetAsFirstSibling();
        }
    }

    private RectTransform GetRoot()
    {
        if (root == null)
        {
            root = transform as RectTransform;
        }

        return root;
    }

    private static Vector2 DirectionFromTopClockwise(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }
}
