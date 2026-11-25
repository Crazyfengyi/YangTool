using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 封装按钮的选中外观与单击 双击 长按回调
/// </summary>
public class TemplateBtn : MonoBehaviour
{
    // 选中状态显示的按钮
    public BtnExtension clickBtn;
    // 未选中状态显示的按钮
    public BtnExtension unClickBtn;

    /// <summary>
    /// 设置是否显示点击
    /// </summary>
    public void SetShowClick(bool isShowClick)
    {
        clickBtn.gameObject.SetActive(isShowClick);
        unClickBtn.gameObject.SetActive(!isShowClick);
    }
    
    /// <summary>
    /// 设置两种外观的回调 当前实现绑定双击事件
    /// </summary>
    public void SetClick(Action clickBtnAction = null,Action unClickBtnAction = null)
    {
        var temp = new Button.ButtonClickedEvent();
        temp.AddListener(() =>
        {
            clickBtnAction?.Invoke();
        });
        clickBtn.OnDoubleClick = temp;
        var temp2 = new Button.ButtonClickedEvent();
        temp2.AddListener(() =>
        {
            unClickBtnAction?.Invoke();
        });
        unClickBtn.OnDoubleClick = temp2;
    }
    /// <summary>
    /// 设置双击
    /// </summary>
    public void SetDoubleClick(Action clickBtnAction = null,Action unClickBtnAction = null)
    {
        var temp = new Button.ButtonClickedEvent();
        temp.AddListener(() =>
        {
            clickBtnAction?.Invoke();
        });
        clickBtn.OnDoubleClick = temp;
        var temp2 = new Button.ButtonClickedEvent();
        temp2.AddListener(() =>
        {
            unClickBtnAction?.Invoke();
        });
        unClickBtn.OnDoubleClick = temp2;
    }
    /// <summary>
    /// 设置长按
    /// </summary>
    public void SetPressClick(Action longPressBtnAction = null,Action unLongPressBtnAction = null)
    {
        var temp = new Button.ButtonClickedEvent();
        temp.AddListener(() =>
        {
            longPressBtnAction?.Invoke();
        });
        clickBtn.OnLongPress = temp;
        var temp2 = new Button.ButtonClickedEvent();
        temp2.AddListener(() =>
        {
            unLongPressBtnAction?.Invoke();
        });
        unClickBtn.OnLongPress = temp2;
    }
}
