using UnityEngine;
using YangTools.Scripts.Core.YangUGUI;

public class CommonMaskWindow : UGUIPanelBase<DefaultUGUIDataBase>
{
    #region 通用界面

    private static int openMaskNum;
    private static int commonMaskVersion;
    private static int commonMaskOpeningVersion;
    private static bool commonMaskOpening;
    private static int commonMaskPanelId;

    /// <summary>
    /// 打开通用遮罩
    /// </summary>
    public static async void OpenCommonMaskWindow()
    {
        // 增加遮罩计数器和版本号
        openMaskNum++;
        commonMaskVersion++;

        // 如果遮罩正在打开中，更新目标版本号并直接返回
        if (commonMaskOpening)
        {
            commonMaskOpeningVersion = commonMaskVersion;
            return;
        }

        // 检查是否已有遮罩窗口打开或正在打开中
        if (commonMaskPanelId > 0 || UIMonoInstance.Instance.IsOpening(nameof(CommonMaskWindow)).have)
        {
            return;
        }

        commonMaskOpening = true;
        commonMaskOpeningVersion = commonMaskVersion;
        (int id, CommonMaskWindow panel) panel;
        try
        {
            panel = await UIMonoInstance.OpenPanel<CommonMaskWindow>(GroupType.Top);
        }
        finally
        {
            commonMaskOpening = false;
        }

        commonMaskPanelId = panel.id;

        // 异步打开完成时重新确认计数，避免 Close 先执行导致遮罩残留。
        if (openMaskNum <= 0 || commonMaskOpeningVersion != commonMaskVersion)
        {
            UIMonoInstance.Instance.ClosePanel(panel.id);
            commonMaskPanelId = 0;
        }
    }

    /// <summary>
    /// 关闭通用遮罩
    /// </summary>
    public static void CloseCommonMaskWindow()
    {
        // 如果打开计数大于0，则减少计数
        if (openMaskNum > 0)
        {
            openMaskNum--;
        }

        // 如果计数仍然大于0，说明还有其他窗口依赖此遮罩，直接返回
        if (openMaskNum > 0)
        {
            return;
        }

        // 将计数归零
        openMaskNum = 0;
        // 只有计数归零时才让未完成的打开请求过期。
        commonMaskVersion++;

        // 面板还在异步打开时，等 Open 返回后按计数立即关闭。
        if (commonMaskOpening)
        {
            return;
        }

        UIMonoInstance.ClosePanel<CommonMaskWindow>();
        commonMaskPanelId = 0;
    }

    #endregion
}