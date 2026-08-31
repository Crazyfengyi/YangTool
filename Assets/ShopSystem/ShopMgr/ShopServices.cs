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
        /// <summary>
        /// 读取商店存档
        /// </summary>
        /// <returns>读取到的存档数据</returns>
        ShopSaveData Load();

        /// <summary>
        /// 保存商店存档
        /// </summary>
        /// <param name="data">要保存的存档数据</param>
        void Save(ShopSaveData data);
    }

    /// <summary>
    /// 商店道具服务
    /// </summary>
    public interface IShopInventoryService
    {
        /// <summary>
        /// 获取指定道具数量
        /// </summary>
        /// <param name="itemId">道具 ID</param>
        /// <returns>道具数量</returns>
        int GetAmount(string itemId);

        /// <summary>
        /// 检查是否拥有足够的消耗道具
        /// </summary>
        /// <param name="costs">消耗列表</param>
        /// <returns>消耗充足时返回 true</returns>
        bool HasEnough(IReadOnlyList<ShopCostData> costs);

        /// <summary>
        /// 尝试扣除消耗道具
        /// </summary>
        /// <param name="costs">消耗列表</param>
        /// <returns>扣除成功时返回 true</returns>
        bool TryConsume(IReadOnlyList<ShopCostData> costs);

        /// <summary>
        /// 尝试退还消耗道具
        /// </summary>
        /// <param name="costs">要退还的消耗列表</param>
        /// <returns>退还成功时返回 true</returns>
        bool TryRefund(IReadOnlyList<ShopCostData> costs);
    }

    /// <summary>
    /// 商店奖励服务
    /// </summary>
    public interface IShopRewardService
    {
        /// <summary>
        /// 发放商品奖励
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <param name="rewards">奖励列表</param>
        /// <returns>发放成功时返回 true</returns>
        bool GrantRewards(string productId, IReadOnlyList<ShopRewardData> rewards);
    }

    /// <summary>
    /// 商店广告服务
    /// </summary>
    public interface IShopAdService
    {
        /// <summary>
        /// 当前是否存在可用的激励广告
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// 展示激励广告并异步回调结果
        /// </summary>
        /// <param name="completed">广告完成回调</param>
        void ShowRewardedAd(Action<ShopAdResult> completed);
    }

    /// <summary>
    /// 使用 PlayerPrefs 的默认商店存档
    /// </summary>
    public sealed class PlayerPrefsShopSaveStore : IShopSaveStore
    {
        private const string SaveKey = "ShopSystem.SaveData.v1";

        /// <summary>
        /// 从独立 PlayerPrefs 键读取商店存档
        /// </summary>
        /// <returns>存档不存在或读取失败时返回空存档</returns>
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

        /// <summary>
        /// 将商店存档序列化后写入 PlayerPrefs
        /// </summary>
        /// <param name="data">要保存的存档数据</param>
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

        /// <summary>
        /// 获取内存中的道具数量
        /// </summary>
        /// <param name="itemId">道具 ID</param>
        /// <returns>道具数量 不存在时返回零</returns>
        public int GetAmount(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && amounts.TryGetValue(itemId, out int amount)
                ? amount
                : 0;
        }

        /// <summary>
        /// 检查内存中的道具是否足够支付消耗
        /// </summary>
        /// <param name="costs">消耗列表</param>
        /// <returns>消耗充足时返回 true</returns>
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
        /// <param name="costs">要消耗的资源列表</param>
        /// <returns>成功消耗所有资源时返回 true</returns>
        public bool TryConsume(IReadOnlyList<ShopCostData> costs)
        {
            if (!HasEnough(costs))
            {
                return false;
            }

            if (costs == null)
            {
                return true;
            }

            for (int i = 0; i < costs.Count; i++)
            {
                AddAmount(costs[i].ItemId, -costs[i].Amount);
            }

            return true;
        }

        /// <summary>
        /// 尝试退还商品费用
        /// </summary>
        /// <param name="costs">要退还的商品费用数据</param>
        /// <returns>退还成功时返回 true</returns>
        public bool TryRefund(IReadOnlyList<ShopCostData> costs)
        {
            if (costs == null)
            {
                return true;
            }

            for (int i = 0; i < costs.Count; i++)
            {
                var cost = costs[i];
                if (cost == null || cost.Amount < 0)
                {
                    return false;
                }

                AddAmount(cost.ItemId, cost.Amount);
            }

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

        /// <summary>
        /// 将奖励累计到内存奖励表
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <param name="rewardList">奖励列表</param>
        /// <returns>奖励数据有效并发放成功时返回 true</returns>
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
        /// <summary>
        /// 未配置广告服务 因此始终不可用
        /// </summary>
        public bool IsAvailable => false;

        /// <summary>
        /// 返回广告服务未配置的失败结果
        /// </summary>
        /// <param name="completed">广告结果回调</param>
        public void ShowRewardedAd(Action<ShopAdResult> completed)
        {
            completed?.Invoke(new ShopAdResult(false, "广告服务未配置"));
            Debug.LogError("广告");
        }
    }
}
