using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 以索引管理一组按钮的选中外观与选择回调
/// </summary>
public class YangToggleGroup : MonoBehaviour
{
    // 按索引管理的按钮集合
    public List<TemplateBtn> btns;

    // 选择索引变化时的回调
    public Action<int> onSelectCallback;
    
    // 当前选中按钮索引
    private int currentSelectIndex;
    public int CurrentSelectIndex => currentSelectIndex;

    /// <summary>
    /// 为各按钮的未选中入口注册组内点击处理
    /// </summary>
    private void Awake()
    {
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].unClickBtn.onClick.AddListener(OnBtnClick);
        }
    }
    
    /// <summary>
    /// 设置当前选中回调
    /// </summary>
    public void SetAction(Action<int> callback)
    {
        onSelectCallback = callback;
    }
    /// <summary>
    /// 根据当前事件系统选中的对象更新组内外观与索引回调
    /// </summary>
    public void OnBtnClick()
    {
        for (int i = 0; i < btns.Count; i++)
        {
            bool result = EventSystem.current.currentSelectedGameObject == btns[i].unClickBtn.gameObject;
            btns[i].SetShowClick(result);
            if (result) currentSelectIndex = i;
        }
        onSelectCallback?.Invoke(currentSelectIndex);
    }

    /// <summary>
    /// 将选择恢复为首项并同步外观和回调
    /// </summary>
    public void Init()
    {
        for (int i = 0; i < btns.Count; i++)
        {
            btns[i].SetShowClick(false);
        }

        if (btns.Count > 0) btns[0].SetShowClick(true);
        currentSelectIndex = 0;
        onSelectCallback?.Invoke(currentSelectIndex);
    }
}