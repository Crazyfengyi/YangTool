using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShopSystem
{
    /// <summary>
    /// 商店存档服务
    /// </summary>
    public interface IShopSaveStore
    {
        ShopSaveData Load();
        void Save(ShopSaveData data);
    }

    /// <summary>
    /// 商店道具服务
    /// </summary>
    public interface IShopInventoryService
    {
        int GetAmount(string itemId);
        bool HasEnough(IReadOnlyList<ShopCostData> costs);
        bool TryConsume(IReadOnlyList<ShopCostData> costs);
        bool TryRefund(IReadOnlyList<ShopCostData> costs);
    }

    /// <summary>
    /// 商店奖励服务
    /// </summary>
    public interface IShopRewardService
    {
        bool GrantRewards(string productId, IReadOnlyList<ShopRewardData> rewards);
    }

    /// <summary>
    /// 商店广告服务
    /// </summary>
    public interface IShopAdService
    {
        bool IsAvailable { get; }
        void ShowRewardedAd(Action<ShopAdResult> completed);
    }

    /// <summary>
    /// 使用 PlayerPrefs 的默认商店存档
    /// </summary>
    public sealed class PlayerPrefsShopSaveStore : IShopSaveStore
    {
        private const string SaveKey = "ShopSystem.SaveData.v1";

        public ShopSaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return new ShopSaveData();
            }

            try
            {
                var data = JsonUtility.FromJson<ShopSaveData>(PlayerPrefs.GetString(SaveKey));
                return data ?? new ShopSaveData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ShopSystem 存档读取失败 将使用空存档 {exception.Message}");
                return new ShopSaveData();
            }
        }

        public void Save(ShopSaveData data)
        {
            var json = JsonUtility.ToJson(data ?? new ShopSaveData());
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 仅用于独立运行和测试的内存道具服务
    /// </summary>
    public sealed class InMemoryShopInventoryService : IShopInventoryService
    {
        private readonly Dictionary<string, int> amounts = new();

        public int GetAmount(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && amounts.TryGetValue(itemId, out int amount)
                ? amount
                : 0;
        }

        public bool HasEnough(IReadOnlyList<ShopCostData> costs)
        {
            if (costs == null)
            {
                return true;
            }

            for (int i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                if (cost == null || cost.Amount < 0 || GetAmount(cost.ItemId) < cost.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 尝试消耗指定的资源列表
        /// </summary>
        /// <param name="costs">要消耗的资源列表，每个元素包含物品ID和数量</param>
        /// <returns>如果成功消耗所有资源返回true，否则返回false</returns>
        public bool TryConsume(IReadOnlyList<ShopCostData> costs)
        {
            // 首先检查是否有足够的资源
            if (!HasEnough(costs))
            {
                return false;
            }

            // 如果成本列表为空，直接返回true（无需消耗任何资源）
            if (costs == null)
            {
                return true;
            }

            // 遍历成本列表，逐个扣除对应的物品数量
            for (int i = 0; i < costs.Count; i++)
            {
                AddAmount(costs[i].ItemId, -costs[i].Amount);
            }

            return true;
        }

        /// <summary>
        /// 尝试退还商品费用
        /// </summary>
        /// <param name="costs">只读列表，包含要退还的商品费用数据</param>
        /// <returns>如果退还成功返回true，否则返回false</returns>
        public bool TryRefund(IReadOnlyList<ShopCostData> costs)
        {
            // 如果传入的costs列表为null，直接返回true（视为成功）
            if (costs == null)
            {
                return true;
            }

            // 遍历costs列表中的每个费用项
            for (int i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                // 如果费用项为null或金额小于0，则退还失败，返回false
                if (cost == null || cost.Amount < 0)
                {
                    return false;
                }

                // 调用AddAmount方法增加对应物品的数量，实现退还
                AddAmount(cost.ItemId, cost.Amount);
            }

            // 所有费用项都成功退还，返回true
            return true;
        }

        /// <summary>
        /// 设置内存服务中的道具数量
        /// </summary>
        public void SetAmount(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            amounts[itemId] = Mathf.Max(0, amount);
        }

        private void AddAmount(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            amounts[itemId] = Mathf.Max(0, GetAmount(itemId) + amount);
        }
    }

    /// <summary>
    /// 仅用于独立运行和测试的内存奖励服务
    /// </summary>
    public sealed class InMemoryShopRewardService : IShopRewardService
    {
        private readonly Dictionary<string, int> rewards = new();

        public bool GrantRewards(string productId, IReadOnlyList<ShopRewardData> rewardList)
        {
            if (rewardList == null || rewardList.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < rewardList.Count; i++)
            {
                var reward = rewardList[i];
                if (reward == null || string.IsNullOrEmpty(reward.ItemId) || reward.Amount <= 0)
                {
                    return false;
                }
            }

            for (int i = 0; i < rewardList.Count; i++)
            {
                var reward = rewardList[i];
                rewards[reward.ItemId] = GetAmount(reward.ItemId) + reward.Amount;
            }

            return true;
        }

        /// <summary>
        /// 获取内存服务已发放的奖励数量
        /// </summary>
        public int GetAmount(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && rewards.TryGetValue(itemId, out int amount)
                ? amount
                : 0;
        }
    }

    /// <summary>
    /// 未配置广告服务时使用的默认实现
    /// </summary>
    public sealed class UnavailableShopAdService : IShopAdService
    {
        public bool IsAvailable => false;

        public void ShowRewardedAd(Action<ShopAdResult> completed)
        {
            completed?.Invoke(new ShopAdResult(false, "广告服务未配置"));
            Debug.LogError("广告");
        }
    }
}