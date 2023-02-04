/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-02
*/

using System;
using TMPro;
using UnityEngine.UI;
using YangTools.UGUI;

/// <summary>
/// 通用二级确认界面
/// </summary>
public class CommonConfirmPanel : UGUIPanelBase<ConfirmData>
{
    public TMP_Text text;
    public Button okBtn;
    public Button cancelBtn;

    private ConfirmData confirmData;
    private ConfirmData ConfirmData => confirmData;

    private void Awake()
    {
        okBtn.onClick.AddListener(OK_OnClick);
        cancelBtn.onClick.AddListener(Cancel_OnClick);
    }

    public override void OnInit(ConfirmData _confirmData)
    {
        confirmData = _confirmData;
        //TODO: 根据回调 动态显隐按钮
    }

    public void OK_OnClick()
    {
        confirmData?.okCallBack?.Invoke();
        ClosePanel();
    }

    public void Cancel_OnClick()
    {
        confirmData?.cancelCallBack?.Invoke();
        ClosePanel();
    }
}