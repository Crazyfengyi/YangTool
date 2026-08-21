using System;
using System.Collections;
using System.Collections.Generic;
using cfg;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using YangTools.Scripts.Core.YangSaveData;

namespace GameMain
{
    /// <summary>
    /// 体力
    /// </summary>
    [System.Serializable]
    public class Save_CostPower : SaveDataBase
    {
        [SerializeField] public int maxPower; //最大体力

        /// <summary>
        /// 最大体力
        /// </summary>
        public int MaxPower => maxPower;

        /// <summary>
        /// 当前体力
        /// </summary>
        [JsonIgnore]
        public int CurrentPower => (int) BagMgr.Instance.GetBagPropCount(CostPowerTableId);

        [SerializeField] public long lastGetPowerTime; //上次恢复体力时间

        /// <summary>
        /// 上次恢复体力时间
        /// </summary>
        [JsonIgnore]
        public long LastGetPowerTime => lastGetPowerTime;

        [SerializeField] public float recoveryInterval; //恢复间隔时间(秒)

        /// <summary>
        /// 恢复间隔时间(秒)
        /// </summary>
        [JsonIgnore]
        public float RecoveryInterval => recoveryInterval;

        [SerializeField] public long lastInfiniteCostPowerStartTime; //上次无限体力开始时间

        /// <summary>
        /// 上次无限体力开始时间
        /// </summary>
        [JsonIgnore]
        public long LastInfiniteCostPowerStartTime => lastInfiniteCostPowerStartTime;

        [SerializeField] public float infiniteCostPowerTime; //无限体力时间

        /// <summary>
        /// 无限体力时间
        /// </summary>
        [JsonIgnore]
        public float InfiniteCostPowerTime => infiniteCostPowerTime;

        /// <summary>
        /// 是否无限体力
        /// </summary>
        [JsonIgnore]
        public bool IsInfiniteCostPower => TimeTool.DataTimeConvertToTimeStampLocal(DateTimeOffset.Now.LocalDateTime) -
                                           LastInfiniteCostPowerStartTime <
                                           InfiniteCostPowerTime;

        private const int CostPowerTableId = 1001;

        [SerializeField] public float lookInfiniteCostPowerAdCount; //无限体力看广告次数

        /// <summary>
        /// 无限体力看广告次数
        /// </summary>
        [JsonIgnore]
        public float LookInfiniteCostPowerAdCount => lookInfiniteCostPowerAdCount;

        
        public override void SetDefaultData(string tableData)
        {
            //BagMgr.Instance.AddBagProp(CostPowerTableId, ConfigMgr.Instance.Tables.GlobalConfigCategory.InitCostPower);
            maxPower = 10;
            // maxPower = ConfigMgr.Instance.Tables.GlobalConfigCategory.MaxCostPower;
            // recoveryInterval = ConfigMgr.Instance.Tables.GlobalConfigCategory.SpeedCostPower;
            // lastGetPowerTime = CommonTool.DataTimeConvertToTimeStamp(DateTimeOffset.Now.LocalDateTime);
            // lastInfiniteCostPowerStartTime = CommonTool.DataTimeConvertToTimeStamp(DateTimeOffset.Now.LocalDateTime);
            infiniteCostPowerTime = 0;

            RecordLastTime(0);
        }

        /// <summary>
        /// 根据时间检查体力恢复
        /// </summary>
        public void CheckRecoveryCostPower()
        {
            if (IsMaxPower())
            {
                RecordLastTime(0);
                return;
            }

            DateTime time = DateTime.Now;
            DateTime lastTime = TimeTool.TimestampConvertToDate(LastGetPowerTime);
            TimeSpan ts = time - lastTime;

            if (ts.TotalSeconds >= recoveryInterval)
            {
                double remainCount = ts.TotalSeconds % recoveryInterval;
                int addPowerCount = (int) (ts.TotalSeconds / recoveryInterval);
                Debug.Log($"时间增加体力:{lastTime},{time}");
                AddCostPower(addPowerCount, true, (float) remainCount);
            }
        }

        /// <summary>
        /// 添加体力
        /// </summary>
        public void AddCostPower(int value, bool isTimeAdd = false, float remainCount = 0)
        {
            //时间恢复限制最大
            if (isTimeAdd)
            {
                float lastValue = BagMgr.Instance.GetBagPropCount(CostPowerTableId);
                float newPower = Mathf.Min(MaxPower, lastValue + value);
                if (newPower > lastValue)
                {
                    Debug.Log($"增加体力(时间):{newPower - lastValue}");
                    BagMgr.Instance.AddBagProp(CostPowerTableId, newPower - lastValue);
                    RecordLastTime(remainCount);
                }
            }
            else
            {
                Debug.Log($"增加体力:{value}");
                BagMgr.Instance.AddBagProp(CostPowerTableId, value);
            }
        }

        /// <summary>
        /// 记录上次计算时间
        /// </summary>
        public void RecordLastTime(float remainCount)
        {
            DateTimeOffset nowTime = DateTimeOffset.Now;
            nowTime = nowTime.AddSeconds(-remainCount); //减去剩余时间
            lastGetPowerTime = TimeTool.DataTimeConvertToTimeStampLocal(nowTime.LocalDateTime);
        }

        /// <summary>
        /// 消耗体力
        /// </summary>
        public bool RemoveCostPower(int value)
        {
            float currentValue = BagMgr.Instance.GetBagPropCount(CostPowerTableId);
            if (currentValue < value) return false;
            BagMgr.Instance.RemoveBagProp(CostPowerTableId, value);

            float remainValue = currentValue - value;
            Debug.Log($"消耗体力:{value},剩余:{remainValue}");
            return true;
        }

        public bool PowerIsEnough(int value)
        {
            float currentValue = BagMgr.Instance.GetBagPropCount(CostPowerTableId);
            return currentValue >= value;
        }

        /// <summary>
        /// 体力是否已经满
        /// </summary>
        public bool IsMaxPower()
        {
            float currentValue = BagMgr.Instance.GetBagPropCount(CostPowerTableId);
            return currentValue >= maxPower;
        }

        /// <summary>
        /// 设置无限体力
        /// </summary>
        public void SetInfiniteCostPowerTime(float time)
        {
            lastInfiniteCostPowerStartTime = TimeTool.DataTimeConvertToTimeStampLocal(DateTimeOffset.Now.LocalDateTime);
            infiniteCostPowerTime = time;
        }

        /// <summary>
        /// 增加无限体力看广告次数
        /// </summary>
        public void AddLookInfiniteCostPowerAdCount(int num = 1)
        {
            lookInfiniteCostPowerAdCount += num;
        }

        /// <summary>
        /// 清零无限体力看广告次数
        /// </summary>
        public void ResetLookInfiniteCostPowerAdCount()
        {
            lookInfiniteCostPowerAdCount = 0;
        }

    }
}