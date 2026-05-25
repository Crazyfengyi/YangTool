using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class YangToggleGroup : MonoBehaviour
{
    public List<TemplateBtn> btns;

    public Action<int> onSelectCallback;
    
    private int currentSelectIndex;
    public int CurrentSelectIndex => currentSelectIndex;

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