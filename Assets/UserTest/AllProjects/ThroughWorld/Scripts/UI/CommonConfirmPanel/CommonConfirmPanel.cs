/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-02
*/

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YangTools.Scripts.Core.YangUGUI;
using static UnityEngine.Application;

/// <summary>
/// 通用二级确认界面
/// </summary>
public class CommonConfirmPanel : UGUIPanelBase<ConfirmData>
{
    public TMP_Text text;//显示文字
    public Button okBtn;//ok按钮
    public TMP_Text okBtnText;//ok按钮文字
    public Button cancelBtn;//关闭按钮
    public TMP_Text cancelBtnText;//关闭按钮文字

    private ConfirmData confirmData; //本次确认数据
    private bool isHandled; //是否已经确认或取消
    private ConfirmData ConfirmData => confirmData;

    private void Awake()
    {
        okBtn.onClick.AddListener(OK_OnClick);
        cancelBtn.onClick.AddListener(Cancel_OnClick);
    }

    /// <summary>
    /// 首次创建时初始化界面组件
    /// </summary>
    public override void OnInit(ConfirmData pConfirmData)
    {
        base.OnInit(pConfirmData);
    }

    /// <summary>
    /// 每次打开刷新文案和倒计时数据
    /// </summary>
    public override void OnOpen(object userData)
    {
        if (userData is not ConfirmData data) throw new ArgumentException("确认弹窗缺少 ConfirmData", nameof(userData));
        confirmData = data;
        isHandled = false;
        base.OnOpen(userData);
        Init();
    }

    /// <summary>
    /// 刷新按钮和提示文字
    /// </summary>
    private void Init()
    {
        okBtn.gameObject.SetActive(confirmData.okCallBack != null);
        cancelBtn.gameObject.SetActive(confirmData.cancelCallBack != null);

        if (okBtnText) okBtnText.text = GetOkBtnText();
        if (cancelBtnText) cancelBtnText.text = GetCancelBtnText();
        if (text) text.text = confirmData.showMsg;
    }

    /// <summary>
    /// 推进本次弹窗的自动选择倒计时
    /// </summary>
    public override void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
        base.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
        if (isHandled || confirmData == null) return;

        if (confirmData.isCountDownSelect)
        {
            confirmData.countDownTime -= unscaledDeltaTimeSeconds;

            if (confirmData.autoSelectOk)
            {
                okBtnText.text = GetOkBtnText() + $"({(int)confirmData.countDownTime})";
            }
            else
            {
                cancelBtnText.text = GetCancelBtnText() + $"({(int)confirmData.countDownTime})";
            }

            if (confirmData.countDownTime <= 0)
            {
                if (confirmData.autoSelectOk)
                {
                    OK_OnClick();
                }
                else
                {
                    Cancel_OnClick();
                }
            }
        }
    }

    private string GetOkBtnText()
    {
        return string.IsNullOrEmpty(confirmData.okBtnText) ? "确定" : confirmData.okBtnText;
    }

    private string GetCancelBtnText()
    {
        return string.IsNullOrEmpty(confirmData.cancelBtnText) ? "取消" : confirmData.cancelBtnText;
    }

    /// <summary>
    /// 只执行一次确认操作
    /// </summary>
    public void OK_OnClick()
    {
        if (isHandled) return;
        isHandled = true;
        try
        {
            confirmData?.okCallBack?.Invoke();
        }
        finally
        {
            CloseSelfPanel();
        }
    }

    /// <summary>
    /// 只执行一次取消操作
    /// </summary>
    public void Cancel_OnClick()
    {
        if (isHandled) return;
        isHandled = true;
        try
        {
            confirmData?.cancelCallBack?.Invoke();
        }
        finally
        {
            CloseSelfPanel();
        }
    }

    /// <summary>
    /// 关闭时释放本次弹窗数据和业务回调
    /// </summary>
    public override void OnClose(bool isShutdown, object userData)
    {
        isHandled = true;
        confirmData = null;
        base.OnClose(isShutdown, userData);
    }

    /// <summary>
    /// 回收时清理数据引用
    /// </summary>
    public override void OnRecycle()
    {
        isHandled = true;
        confirmData = null;
        base.OnRecycle();
    }
}
