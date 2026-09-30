/*
 *Copyright(C) 2020 by Test
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2022.1.0f1c1
 *创建时间:         2023-02-02
*/

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YangTools;
using YangTools.Scripts.Core;
using YangTools.Scripts.Core.YangUGUI;

public class UICommonTool : Singleton<UICommonTool>
{
    private Task<(int id, TipsPanel panel)> tipsOpening; //合并提示页面打开请求
    private Task<(int id, LoadingPanel panel)> loadingOpening; //合并加载页面打开请求
    private int loadingVersion; //最近一次加载页面显示请求
    #region 飘字提示

    /// <summary>
    /// 异步获取提示页面后显示文字
    /// </summary>
    public void ShowTip(string tipStr)
    {
        ShowTipAsync(tipStr).Forget(exception => Debug.LogException(exception));
    }

    /// <summary>
    /// 共享未完成的提示页面加载任务
    /// </summary>
    private async UniTask ShowTipAsync(string tipStr)
    {
        TipsPanel panel = UIMonoInstance.Instance.GetUIPanel<TipsPanel>("TipsPanel"); //已打开页面
        if (panel)
        {
            panel.ShowTip(tipStr);
            return;
        }
        tipsOpening ??= UIMonoInstance.OpenPanel<TipsPanel>(GroupType.顶部).AsTask();
        Task<(int id, TipsPanel panel)> request = tipsOpening; //当前共享任务
        try
        {
            var result = await request; //完成的打开结果
            if (result.panel) result.panel.ShowTip(tipStr);
        }
        finally
        {
            if (ReferenceEquals(tipsOpening, request)) tipsOpening = null;
        }
    }

    #endregion 飘字提示

    #region Loading界面

    /// <summary>
    /// 按最近一次请求更新加载页面
    /// </summary>
    public void SetLoadingShow(bool isShow)
    {
        SetLoadingShowAsync(isShow, ++loadingVersion).Forget(exception => Debug.LogException(exception));
    }

    /// <summary>
    /// 等待共享加载任务并忽略过期的显示请求
    /// </summary>
    private async UniTask SetLoadingShowAsync(bool isShow, int version)
    {
        LoadingPanel panel = UIMonoInstance.Instance.GetUIPanel<LoadingPanel>("LoadingPanel"); //已打开页面
        if (!panel)
        {
            loadingOpening ??= UIMonoInstance.OpenPanel<LoadingPanel>(GroupType.顶部).AsTask();
            Task<(int id, LoadingPanel panel)> request = loadingOpening; //当前共享任务
            try
            {
                panel = (await request).panel;
            }
            finally
            {
                if (ReferenceEquals(loadingOpening, request)) loadingOpening = null;
            }
        }
        if (version != loadingVersion || !panel) return;
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

        OpenConfirmPanelAsync(data).Forget(exception => Debug.LogException(exception));
    }

    /// <summary>
    /// 将确认弹窗数据传递到每次打开生命周期
    /// </summary>
    private async UniTask OpenConfirmPanelAsync(ConfirmData data)
    {
        await UIMonoInstance.Instance.OpenPanel("CommonConfirmPanel", GroupType.顶部, userData: data);
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
