using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using GameMain;
using UnityEngine;
using YangTools.Scripts.Core;
using YangTools.Scripts.Core.ResourceManager;
using YangTools.Scripts.Core.YangSaveData;

public class CostPowerManager : MonoSingleton<CostPowerManager>
{
    public bool IsInfiniteCostPower => YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>().IsInfiniteCostPower;

    protected override void Awake()
    {
        base.Awake();
        YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>()?.CheckRecoveryCostPower();
    }

    /// <summary>
    /// 添加体力
    /// </summary>
    public void AddCostPower(int addCostPower)
    {
        YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>().AddCostPower(addCostPower);
    }

    /// <summary>
    /// 尝试消耗体力 
    /// </summary>
    public bool TryUseCostPower(int useCostPower)
    {
        //无限体力
        bool isInfiniteCostPower =
            YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>().IsInfiniteCostPower;
        if (isInfiniteCostPower) return true;
        //消耗体力成功
        bool result = YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>()
            .RemoveCostPower(useCostPower);
        if (result)
        {
            //if(useCostPower != 0)UIWindowTool.ShowPromptBox($"-{useCostPower}体力");
            return true;
        }

        //IWindowTool.ShowPromptBox("体力不足，请先获取体力");
        //UIWindowMgr.Instance.OpenWindow<AddCostPowerWindow>();
        //体力不足
        return false;
    }

    /// <summary>
    /// 体力是否足够
    /// </summary>
    public bool PowerIsEnough(int useCostPower)
    {
        bool isInfiniteCostPower =
            YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>().IsInfiniteCostPower;
        if (isInfiniteCostPower) return true;

        var result = YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>().PowerIsEnough(useCostPower);
        if (result == false)
        {
            //UIWindowTool.ShowPromptBox("体力不足，请先获取体力");
        }

        return result;
    }

    //获取当前日期字符串
    private static readonly string AddCostPowerDayStr =
        $"{PlatformMgr.UserId}_AddCostPower_{DateTime.Now:yyyy.MM.dd}_104";

    /// <summary>
    /// 获得今天恢复体力次数
    /// </summary>
    public int GetToDayAddCostPowerCount()
    {
#if UNITY_ANDROID
        if (!UnityEngine.PlayerPrefs.HasKey(AddCostPowerDayStr))
        {
            return 0;
        }

        int count = UnityEngine.PlayerPrefs.GetInt(AddCostPowerDayStr);
        return count;
#else
        if (!PlayerPrefs.HasKey(AddCostPowerDayStr))
        {
            return 0;
        }

        int count = PlayerPrefs.GetInt(AddCostPowerDayStr);
        return count;
#endif
    }

    /// <summary>
    /// 增加今天恢复体力次数
    /// </summary>
    public void TagAddCostPowerCount()
    {
        if (!PlayerPrefs.HasKey(AddCostPowerDayStr))
        {
            PlayerPrefs.SetInt(AddCostPowerDayStr, 1);
            PlayerPrefs.Save();
            return;
        }

        int count = PlayerPrefs.GetInt(AddCostPowerDayStr);
        PlayerPrefs.SetInt(AddCostPowerDayStr, count + 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 通用关卡消耗体力和道具
    /// </summary>
    public void CommonLevelCostPower(Transform btnTransform, Action<bool> result)
    {
        //消耗物品
        Dictionary<int, int> temp = new Dictionary<int, int>();
        var allItemIsEnough = true;
        foreach (KeyValuePair<int, int> item in temp)
        {
            if (item.Key == 1001)
            {
                allItemIsEnough &= CostPowerManager.Instance.PowerIsEnough(item.Value);
            }
            else
            {
                allItemIsEnough &= BagMgr.Instance.BagPropEnough(item.Key, item.Value);
            }
        }

        //道具充足
        if (allItemIsEnough)
        {
            Action costPowerCallBack = () => { result?.Invoke(true); };

            bool haveCostPower = false;
            foreach (KeyValuePair<int, int> item in temp)
            {
                if (item.Key == 1001)
                {
                    TryUseCostPower(item.Value);
                    //UIWindowTool.ShowMaskWindow(0);
                    haveCostPower = true;
                    ShowCostPowerAniTips(btnTransform.parent.transform,
                        btnTransform.localPosition, item.Value, () =>
                        {
                            //UIWindowTool.CloseMaskWindow();
                            costPowerCallBack?.Invoke();
                            costPowerCallBack = null;
                        });
                }
                else
                {
                    BagMgr.Instance.RemoveBagProp(item.Key, item.Value);
                }
            }

            if (!haveCostPower)
            {
                result?.Invoke(true);
            }
        }
        else
        {
            result?.Invoke(false);
        }
    }

    /// <summary>
    /// 显示飘分动画
    /// </summary>
    public async void ShowCostPowerAniTips(Transform parent, Vector2 pos, int num, Action overCallBack = null)
    {
        if (num == 0)
        {
            overCallBack?.Invoke();
            return;
        }

        //GameObject go = await ResourceManager.InstantiateGameObject("ShowTips", parent, false);
        //ShowTips script = go.GetComponent<ShowTips>();
        //script.InitShow(pos, num, overCallBack);
    }
}