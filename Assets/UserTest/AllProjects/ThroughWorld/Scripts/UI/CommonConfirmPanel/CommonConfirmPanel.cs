/**
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

    private ConfirmData confirmData;
    private ConfirmData ConfirmData => confirmData;

    private void Awake()
    {
        okBtn.onClick.AddListener(OK_OnClick);
        cancelBtn.onClick.AddListener(Cancel_OnClick);
    }

    public override void OnInit(ConfirmData pConfirmData)
    {
        base.OnInit(pConfirmData);
        confirmData = pConfirmData;
        Init();
    }

    private void Init()
    {
        okBtn.gameObject.SetActive(confirmData.okCallBack != null);
        cancelBtn.gameObject.SetActive(confirmData.cancelCallBack != null);

        if (okBtnText) okBtnText.text = GetOkBtnText();
        if (cancelBtnText) cancelBtnText.text = GetCancelBtnText();
    }

    public override void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
    {
        base.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);

        if (confirmData.isCountDownSelect)
        {
            confirmData.countDownTime -= Time.unscaledDeltaTime;

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

    public void OK_OnClick()
    {
        confirmData?.okCallBack?.Invoke();
        CloseSelfPanel(); 
    }

    public void Cancel_OnClick()
    {
        confirmData?.cancelCallBack?.Invoke();
        CloseSelfPanel();
    }
}