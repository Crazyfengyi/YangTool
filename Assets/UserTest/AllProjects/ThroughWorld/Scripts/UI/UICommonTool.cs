/** 
 *Copyright(C) 2020 by Test 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2022.1.0f1c1 
 *创建时间:         2023-02-02 
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YangTools;
using YangTools.UGUI;

public class UICommonTool : Singleton<UICommonTool>
{

    #region 提示
    public void ShowTip(string tipStr)
    {
        (bool have, UIPanelInfo panelInfo) info = UIMonoInstance.Instance.IsOpening("TipsPanel");
        TipsPanel panel = null;
        if (!info.have)
        {
            UIMonoInstance.Instance.OpenUIPanel("TipsPanel", "Top");
            panel = UIMonoInstance.Instance.GetUIPanel<TipsPanel>("TipsPanel");
        }
        else
        {
            panel = (info.panelInfo.UIPanel as UIPanel).UGUIPanel as TipsPanel;
        }

        panel.ShowTip(tipStr);
    }
    #endregion

    #region Loading界面
    public void SetLoadingShow(bool isShow)
    {
        (bool have, UIPanelInfo panelInfo) info = UIMonoInstance.Instance.IsOpening("LoadingPanel");
        LoadingPanel panel = null;
        if (!info.have)
        {
            UIMonoInstance.Instance.OpenUIPanel("LoadingPanel", "Top");
            panel = UIMonoInstance.Instance.GetUIPanel<LoadingPanel>("LoadingPanel");
        }
        else
        {
            panel = (info.panelInfo.UIPanel as UIPanel).UGUIPanel as LoadingPanel;
        }

        if (isShow)
        {
            panel.OpenLoading();
        }
        else
        {
            panel.CloseLoading();
        }
    }
    #endregion

    #region 通用二级确认弹窗

    public void ShowConfirmPanel(string str, Action okBtnCallBack, Action cancelBtnCallBack)
    {
        //TODO: 根据回调 动态显隐按钮
        UIMonoInstance.Instance.OpenUIPanel("LoadingPanel", "Top");
        CommonConfirmPanel panel = UIMonoInstance.Instance.GetUIPanel<CommonConfirmPanel>("LoadingPanel");
        panel.Init(str, okBtnCallBack, cancelBtnCallBack);
    }
    #endregion
}