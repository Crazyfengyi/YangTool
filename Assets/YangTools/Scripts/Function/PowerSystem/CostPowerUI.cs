using System;
using System.Collections;
using System.Collections.Generic;
using GameMain;
using TMPro;
using UnityEngine;
using YangTools.Scripts.Core.YangSaveData;

public class CostPowerUI : MonoBehaviour
{
    public TextMeshProUGUI powerCount;
    public TextMeshProUGUI powerTime;

    private float time;
    private float timeInterval = 0.02f;

    public void OnEnable()
    {
        time = 0;
    }

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