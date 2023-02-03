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
public class CommonConfirmPanel : UGUIPanelBase<DefaultUGUIDataBase>
{
    public TMP_Text text;
    public Button okBtn;
    public Button cancelBtn;

    private Action okBtnCallback;
    private Action cancelBtnCallback;

    private void Awake()
    {
        okBtn.onClick.AddListener(OK_OnClick);
        cancelBtn.onClick.AddListener(cancel_OnClick);
    }

    public void Init(params object[] data)
    {
        if (data.Length > 0)
        {
            text.text = (string)data[0];
        }
        if (data.Length > 1)
        {
            okBtnCallback = (Action)data[1];
        }
        else
        {
            okBtnCallback = null;
        }

        if (data.Length > 2)
        {
            cancelBtnCallback = (Action)data[2];
        }
        else
        {
            cancelBtnCallback = null;
        }
    }

    public void OK_OnClick()
    {
        okBtnCallback?.Invoke();
        ClosePanel();
    }

    public void cancel_OnClick()
    {
        cancelBtnCallback?.Invoke();
        ClosePanel();
    }
}