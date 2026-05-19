using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectItemUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private GameObject selectedRoot;

    private ISelectableItem item;
    private Action<ISelectableItem, bool> onSelectionChanged;
    private bool suppressNotify;

    public ISelectableItem Item => item;

    private void Awake()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(HandleToggleChanged);
        }
    }

    public void Init(ISelectableItem item, bool selected, ToggleGroup toggleGroup,
        Action<ISelectableItem, bool> onSelectionChanged)
    {
        this.item = item;
        this.onSelectionChanged = onSelectionChanged;

        if (toggle != null)
        {
            toggle.group = toggleGroup;
            SetSelected(selected, false);
        }

        if (nameText != null)
        {
            nameText.text = item != null ? item.DisplayName : string.Empty;
        }

        if (iconImage != null)
        {
            iconImage.sprite = item != null ? item.Icon : null;
            iconImage.enabled = item != null && item.Icon != null;
        }
    }

    public void SetSelected(bool selected, bool notify)
    {
        if (toggle != null)
        {
            suppressNotify = !notify;
            toggle.isOn = selected;
            suppressNotify = false;
        }

        if (selectedRoot != null)
        {
            selectedRoot.SetActive(selected);
        }
    }

    /// <summary>
    /// 处理选中
    /// </summary>
    private void HandleToggleChanged(bool selected)
    {
        if (selectedRoot != null)
        {
            selectedRoot.SetActive(selected);
        }

        if (!suppressNotify)
        {
            onSelectionChanged?.Invoke(item, selected);
        }
    }
}

public enum ItemSelectionMode
{
    Single,
    Multiple
}
