using System;
using System.Collections.Generic;
using GameMain;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangUGUI;
/// <summary>
/// 物品选择界面
/// </summary>
public class ItemSelectWindow : UGUIPanelBase<DefaultUGUIDataBase>
{
    [Header("UI")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private SelectItemUI selectItemUIPrefab;
    [SerializeField] private ToggleGroup singleSelectGroup;
    [SerializeField] private UICustomButton okButton;
    [SerializeField] private UICustomButton cancelButton;

    public event Action<IReadOnlyList<ISelectableItem>> OnSelectionChanged;
    public ItemSelectionMode SelectionMode => selectionMode;
    public IReadOnlyList<ISelectableItem> ItemsData => itemsData;
    
    private ItemSelectionMode selectionMode = ItemSelectionMode.Single;
    private readonly List<ISelectableItem> itemsData = new List<ISelectableItem>();
    private readonly List<SelectItemUI> allItemsUI = new List<SelectItemUI>();
    private readonly HashSet<string> selectedIds = new HashSet<string>();

    private Action<IReadOnlyList<ISelectableItem>> okCallback;
    private Action cancelCallback;

    private void Start()
    {
        if (singleSelectGroup == null)
        {
            singleSelectGroup = GetComponent<ToggleGroup>();
        }

        if (singleSelectGroup == null)
        {
            singleSelectGroup = gameObject.AddComponent<ToggleGroup>();
        }

        if (okButton != null)
        {
            okButton.AddListener(OKBtn_OnClick);
        }

        if (cancelButton != null)
        {
            cancelButton.AddListener(CancelBtn_OnClick);
        }
    }

    /// <summary>
    /// 单选打开
    /// </summary>
    public void OpenSingle(
        IEnumerable<ISelectableItem> sourceItems,
        string initialSelectedId,
        Action<IReadOnlyList<ISelectableItem>> onOk,
        Action onCancel = null)
    {
        Open(sourceItems, ItemSelectionMode.Single, ToSingleIdList(initialSelectedId), onOk, onCancel);
    }
    /// <summary>
    /// 多选打开
    /// </summary>
    public void OpenMultiple(
        IEnumerable<ISelectableItem> sourceItems,
        IEnumerable<string> initialSelectedIds,
        Action<IReadOnlyList<ISelectableItem>> onOk,
        Action onCancel = null)
    {
        Open(sourceItems, ItemSelectionMode.Multiple, initialSelectedIds, onOk, onCancel);
    }
    /// <summary>
    /// 通用打开
    /// </summary>
    private void Open(
        IEnumerable<ISelectableItem> sourceItems,
        ItemSelectionMode mode,
        IEnumerable<string> initialSelectedIds,
        Action<IReadOnlyList<ISelectableItem>> onOk,
        Action onCancel = null)
    {
        selectionMode = mode;
        okCallback = onOk;
        cancelCallback = onCancel;

        itemsData.Clear();
        if (sourceItems != null)
        {
            foreach (var item in sourceItems)
            {
                if (item != null)
                {
                    itemsData.Add(item);
                }
            }
        }

        selectedIds.Clear();
        if (initialSelectedIds != null)
        {
            foreach (var id in initialSelectedIds)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    selectedIds.Add(id);
                    if (selectionMode == ItemSelectionMode.Single)
                    {
                        break;
                    }
                }
            }
        }

        gameObject.SetActive(true);
        Rebuild();
        NotifySelectChanged();
    }

    /// <summary>
    /// 获取所有选中Item
    /// </summary>
    public IReadOnlyList<ISelectableItem> GetSelectedItems()
    {
        return AllSelectedItems();
    }
    /// <summary>
    /// 所有选中Item
    /// </summary>
    private List<ISelectableItem> AllSelectedItems()
    {
        var selectedItems = new List<ISelectableItem>();
        for (int i = 0; i < itemsData.Count; i++)
        {
            var item = itemsData[i];
            if (item != null && selectedIds.Contains(item.Id))
            {
                selectedItems.Add(item);
            }
        }

        return selectedItems;
    }
    /// <summary>
    /// 确认按钮
    /// </summary>
    public void OKBtn_OnClick()
    {
        List<ISelectableItem> selectedItems = AllSelectedItems();
        okCallback?.Invoke(selectedItems);
        CloseSelfPanel();
    }
    /// <summary>
    /// 关闭按钮
    /// </summary>
    public void CancelBtn_OnClick()
    {
        cancelCallback?.Invoke();
        CloseSelfPanel();
    }
    /// <summary>
    /// 重新创建
    /// </summary>
    private void Rebuild()
    {
        ClearItems();
        if (contentRoot == null || selectItemUIPrefab == null)
        {
            Debug.LogWarning("ItemSelectionPage needs contentRoot and itemViewPrefab.", this);
            return;
        }

        UnityEngine.UI.ToggleGroup group = selectionMode == ItemSelectionMode.Single ? singleSelectGroup : null;
        for (int i = 0; i < itemsData.Count; i++)
        {
            ISelectableItem item = itemsData[i];
            SelectItemUI itemUI = Instantiate(selectItemUIPrefab, contentRoot);
            itemUI.Init(item, selectedIds.Contains(item.Id), group, HandleSelectionChanged);
            allItemsUI.Add(itemUI);
        }
    }
    /// <summary>
    /// 清空Item
    /// </summary>
    private void ClearItems()
    {
        for (int i = 0; i < allItemsUI.Count; i++)
        {
            if (allItemsUI[i] != null)
            {
                Destroy(allItemsUI[i].gameObject);
            }
        }
        allItemsUI.Clear();
    }
    /// <summary>
    /// 处理选择改变
    /// </summary>
    private void HandleSelectionChanged(ISelectableItem item, bool selected)
    {
        if (item == null || string.IsNullOrEmpty(item.Id)) return;

        if (selectionMode == ItemSelectionMode.Single)
        {
            selectedIds.Clear();
            if (selected)
            {
                selectedIds.Add(item.Id);
            }
        }
        else if (selected)
        {
            selectedIds.Add(item.Id);
        }
        else
        {
            selectedIds.Remove(item.Id);
        }
        SyncItemSelect();
        NotifySelectChanged();
    }
    /// <summary>
    /// 同步刷新Item选择
    /// </summary>
    private void SyncItemSelect()
    {
        for (int i = 0; i < allItemsUI.Count; i++)
        {
            SelectItemUI itemUI = allItemsUI[i];
            if (itemUI == null || itemUI.Item == null) continue;
            itemUI.SetSelected(selectedIds.Contains(itemUI.Item.Id), false);
        }
    }
    /// <summary>
    /// 通知选择更改
    /// </summary>
    private void NotifySelectChanged()
    {
        OnSelectionChanged?.Invoke(AllSelectedItems());
    }
    /// <summary>
    /// 获取有效ID
    /// </summary>
    private static IEnumerable<string> ToSingleIdList(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            yield break;
        }
        yield return id;
    }
}

//测试：
// if (Input.GetKeyDown(KeyCode.A))
// {
//     List<ISelectableItem> tempList = new List<ISelectableItem>()
//     {
//         new SelectableItem("1","测试1"),
//         new SelectableItem("2","测试2"),
//         new SelectableItem("3","测试3")
//     };
//     (int id, IUGUIPanel panel) panelData = await ItemSelectWindow.OpenPanel(WindowLayerType.中间层.ToString(),null,"ItemSelectionWindow");
//     //((ItemSelectWindow)panelData.panel).OpenSingle(tempList,"1",null,null);
//     ((ItemSelectWindow)panelData.panel).OpenMultiple(tempList,new []{"1"},null,null);
// }
