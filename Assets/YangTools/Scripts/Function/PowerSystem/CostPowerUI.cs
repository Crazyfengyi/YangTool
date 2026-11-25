using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using TMPro;
using UnityEngine;
using YangTools.Scripts.Core.YangSaveData;

/// <summary>
/// 按非缩放时间间隔刷新体力数量和恢复倒计时
/// </summary>
public class CostPowerUI : MonoBehaviour
{
    // 体力数量显示文本
    public TextMeshProUGUI powerCount;
    // 体力恢复倒计时文本
    public TextMeshProUGUI powerTime;

    // 累计的非缩放刷新时间
    private float time;
    // 体力界面刷新间隔秒数
    private float timeInterval = 0.02f;

    /// <summary>
    /// 重置刷新计时器
    /// </summary>
    public void OnEnable()
    {
        time = 0;
    }

    /// <summary>
    /// 按刷新间隔检查体力恢复并更新数量与倒计时显示
    /// </summary>
    public void Update()
    {
        time += Time.unscaledDeltaTime;
        if (time >= timeInterval)
        {
            time = 0;
            Save_CostPower temp = YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>();
            bool isMaxPower = temp.IsMaxPower();
            if (!isMaxPower)
            {
                temp.CheckRecoveryCostPower();
            }
            
            DateTime nowTime = DateTime.Now;
            DateTime lastTime = TimeTool.TimestampConvertToDate(temp.LastGetPowerTime);
            TimeSpan ts = nowTime - lastTime;
            powerCount.text = $"{temp.CurrentPower}";
            int remnantSeconds = (int)(temp.RecoveryInterval - ts.TotalSeconds);
            TimeSpan tsRemnant = TimeSpan.FromSeconds(remnantSeconds);
            if (CostPowerManager.Instance.IsInfiniteCostPower)
            {
                powerCount.text = "无限";
                Save_CostPower data = YangSaveDataManager.Instance.DataCenter.GetLocalSave<Save_CostPower>();
                DateTime lastInfiniteCostPowerStartTime = TimeTool.TimestampConvertToDate((long)data.LastInfiniteCostPowerStartTime);
                lastInfiniteCostPowerStartTime = lastInfiniteCostPowerStartTime.AddSeconds(data.InfiniteCostPowerTime);
                
                TimeSpan downTime = lastInfiniteCostPowerStartTime - DateTimeOffset.Now.LocalDateTime;
                powerTime.text = downTime.ToString(@"mm\:ss");//倒计时
            }
            else
            {
                powerTime.text = isMaxPower ? "已满" : tsRemnant.ToString(@"mm\:ss");
            }
        }
    }
}