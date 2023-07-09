/**
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-02
*/

using System;
using YangTools;
using YangTools.UGUI;

public class UICommonTool : Singleton<UICommonTool>
{
    #region 飘字提示

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

    #endregion 飘字提示

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
            GameInputManager.Instance.DisablePlayer();
            panel.OpenLoading();
        }
        else
        {
            GameInputManager.Instance.EnablePlayer();
            panel.CloseLoading();
        }
    }

    #endregion Loading界面

    #region 通用二级确认弹窗

    /// <summary>
    /// 显示二级确认弹窗
    /// </summary>
    /// <param name="showMsg">显示的弹窗文字</param>
    /// <param name="okBtnText">确认按钮文字</param>
    /// <param name="cancelBtnText">关闭按钮文字</param>
    /// <param name="okBtnCallBack">确认按钮回调</param>
    /// <param name="cancelBtnCallBack">关闭按钮回调</param>
    /// <param name="isCountDownSelect">是否倒计时自动选择</param>
    /// <param name="countDownTime">倒计时时间</param>
    /// <param name="autoSelectOk">是否倒计时完选择确定按钮</param>
    public void ShowConfirmPanel(string showMsg, string okBtnText = "", string cancelBtnText = "",
        Action okBtnCallBack = null, Action cancelBtnCallBack = null,
        bool isCountDownSelect = false, float countDownTime = 10f, bool autoSelectOk = true)
    {
        ConfirmData data = new ConfirmData();
        data.showMsg = showMsg;
        data.okBtnText = okBtnText;
        data.cancelBtnText = cancelBtnText;

        data.isCountDownSelect = isCountDownSelect;
        data.countDownTime = countDownTime;
        data.autoSelectOk = autoSelectOk;

        data.okCallBack = okBtnCallBack;
        data.cancelCallBack = cancelBtnCallBack;

        //保底有个关闭按钮--按钮显隐是通过回调是否为null判断的
        if (okBtnCallBack == null && cancelBtnCallBack == null)
        {
            data.cancelCallBack = () => { };
        }

        CommonConfirmPanel.OpenPanel(data, "CommonConfirmPanel", "Top");
    }

    #endregion 通用二级确认弹窗

    #region 场景加载页面
    /// <summary>
    /// 场景加载页面显隐
    /// </summary>
    public void SetSeceneLoading(bool isShow)
    {
        if (isShow)
        {
            GameUIManager.Instance.StartSeceneLoading();
        }
        else
        {
            GameUIManager.Instance.EndSeceneLoading();
        }
    }
    #endregion
}