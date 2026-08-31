using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShopSystem
{
    #region DataTypes
    /// <summary>
    /// 商品购买方式
    /// </summary>
    public enum ShopPurchaseMethod
    {
        /// <summary>
        /// 使用消耗道具购买
        /// </summary>
        Currency,

        /// <summary>
        /// 通过激励广告购买
        /// </summary>
        RewardedAd
    }

    /// <summary>
    /// 商品配置数据
    /// </summary>
    [Serializable]
    public sealed class ShopProductData
    {
        // 商品唯一标识
        [SerializeField] private string id;
        // 商品显示名称
        [SerializeField] private string displayName;
        // 商品图标
        [SerializeField] private Sprite icon;
        // 商品奖励列表
        [SerializeField] private List<ShopRewardData> rewards = new();
        // 商品消耗列表
        [SerializeField] private List<ShopCostData> costs = new();
        // 商品购买方式
        [SerializeField] private ShopPurchaseMethod purchaseMethod;
        // 最大购买次数 0 表示不限次数
        [SerializeField] private int maxPurchaseCount;
        // 完成一次广告购买所需的广告次数
        [SerializeField] private int requiredAdViews;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public IReadOnlyList<ShopRewardData> Rewards => rewards;
        public IReadOnlyList<ShopCostData> Costs => costs;
        public ShopPurchaseMethod PurchaseMethod => purchaseMethod;
        public int MaxPurchaseCount => maxPurchaseCount;
        public int RequiredAdViews => requiredAdViews;

        /// <summary>
        /// 创建空商品数据
        /// </summary>
        public ShopProductData()
        {
        }

        /// <summary>
        /// 使用指定参数创建商品数据
        /// </summary>
        /// <param name="id">商品唯一标识</param>
        /// <param name="displayName">商品显示名称</param>
        /// <param name="icon">商品图标</param>
        /// <param name="rewards">商品奖励列表</param>
        /// <param name="costs">商品消耗列表</param>
        /// <param name="purchaseMethod">商品购买方式</param>
        /// <param name="maxPurchaseCount">最大购买次数 0 表示不限次数</param>
        /// <param name="requiredAdViews">完成一次广告购买所需的广告次数</param>
        public ShopProductData(
            string id,
            string displayName,
            Sprite icon,
            IReadOnlyList<ShopRewardData> rewards,
            IReadOnlyList<ShopCostData> costs,
            ShopPurchaseMethod purchaseMethod,
            int maxPurchaseCount,
            int requiredAdViews)
        {
            this.id = id;
            this.displayName = displayName;
            this.icon = icon;
            this.rewards = CloneRewards(rewards);
            this.costs = CloneCosts(costs);
            this.purchaseMethod = purchaseMethod;
            this.maxPurchaseCount = maxPurchaseCount;
            this.requiredAdViews = requiredAdViews;
        }

        /// <summary>
        /// 创建不受外部列表修改影响的副本
        /// </summary>
        public ShopProductData Clone()
        {
            return new ShopProductData(
                id,
                displayName,
                icon,
                rewards,
                costs,
                purchaseMethod,
                maxPurchaseCount,
                requiredAdViews);
        }

        /// <summary>
        /// 校验商品数据是否满足购买流程要求
        /// </summary>
        /// <param name="error">校验失败时返回错误描述</param>
        /// <returns>数据有效时返回 true</returns>
        internal bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                error = "商品 ID 为空";
                return false;
            }

            if (rewards == null || rewards.Count == 0)
            {
                error = "商品没有奖励";
                return false;
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewards[i] == null || string.IsNullOrWhiteSpace(rewards[i].ItemId) || rewards[i].Amount <= 0)
                {
                    error = "商品奖励数据无效";
                    return false;
                }
            }

            if (purchaseMethod == ShopPurchaseMethod.Currency && costs == null)
            {
                error = "商品消耗列表为空";
                return false;
            }

            if (costs != null)
            {
                for (int i = 0; i < costs.Count; i++)
                {
                    if (costs[i] == null || string.IsNullOrWhiteSpace(costs[i].ItemId) || costs[i].Amount <= 0)
                    {
                        error = "商品消耗数据无效";
                        return false;
                    }
                }
            }

            if (requiredAdViews < 0 || maxPurchaseCount < 0)
            {
                error = "商品次数不能小于零";
                return false;
            }

            error = null;
            return true;
        }

        private static List<ShopRewardData> CloneRewards(IReadOnlyList<ShopRewardData> source)
        {
            var result = new List<ShopRewardData>();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                result.Add(source[i]?.Clone());
            }

            return result;
        }

        private static List<ShopCostData> CloneCosts(IReadOnlyList<ShopCostData> source)
        {
            var result = new List<ShopCostData>();
            if (source == null)
            {
                return result;
            }

            for (int i = 0; i < source.Count; i++)
            {
                result.Add(source[i]?.Clone());
            }

            return result;
        }
    }

    /// <summary>
    /// 商品奖励数据
    /// </summary>
    [Serializable]
    public sealed class ShopRewardData
    {
        // 奖励道具唯一标识
        [SerializeField] private string itemId;
        // 奖励数量
        [SerializeField] private int amount;
        // 奖励图标
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public int Amount => amount;
        public Sprite Icon => icon;

        /// <summary>
        /// 创建空奖励数据
        /// </summary>
        public ShopRewardData()
        {
        }

        /// <summary>
        /// 使用指定参数创建奖励数据
        /// </summary>
        /// <param name="itemId">奖励道具唯一标识</param>
        /// <param name="amount">奖励数量</param>
        /// <param name="icon">奖励图标</param>
        public ShopRewardData(string itemId, int amount, Sprite icon = null)
        {
            this.itemId = itemId;
            this.amount = amount;
            this.icon = icon;
        }

        /// <summary>
        /// 创建奖励副本
        /// </summary>
        public ShopRewardData Clone()
        {
            return new ShopRewardData(itemId, amount, icon);
        }
    }

    /// <summary>
    /// 商品消耗数据
    /// </summary>
    [Serializable]
    public sealed class ShopCostData
    {
        // 消耗道具唯一标识
        [SerializeField] private string itemId;
        // 消耗数量
        [SerializeField] private int amount;
        // 消耗图标
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public int Amount => amount;
        public Sprite Icon => icon;

        /// <summary>
        /// 创建空消耗数据
        /// </summary>
        public ShopCostData()
        {
        }

        /// <summary>
        /// 使用指定参数创建消耗数据
        /// </summary>
        /// <param name="itemId">消耗道具唯一标识</param>
        /// <param name="amount">消耗数量</param>
        /// <param name="icon">消耗图标</param>
        public ShopCostData(string itemId, int amount, Sprite icon = null)
        {
            this.itemId = itemId;
            this.amount = amount;
            this.icon = icon;
        }

        /// <summary>
        /// 创建消耗副本
        /// </summary>
        public ShopCostData Clone()
        {
            return new ShopCostData(itemId, amount, icon);
        }
    }

    /// <summary>
    /// 商品存档项
    /// </summary>
    [Serializable]
    public sealed class ShopSaveItem
    {
        // 商品唯一标识
        public string productId;
        // 已完成购买次数
        public int purchaseCount;
        // 已完成广告观看次数
        public int adViewCount;
    }

    /// <summary>
    /// 商店存档数据
    /// </summary>
    [Serializable]
    public sealed class ShopSaveData
    {
        // 所有商品的存档项
        public List<ShopSaveItem> items = new();
    }

    /// <summary>
    /// 广告回调结果
    /// </summary>
    public sealed class ShopAdResult
    {
        // 广告是否完整播放成功
        public bool Success { get; }
        // 广告失败时的错误描述
        public string ErrorMessage { get; }

        /// <summary>
        /// 创建广告回调结果
        /// </summary>
        /// <param name="success">广告是否成功</param>
        /// <param name="errorMessage">失败时的错误描述</param>
        public ShopAdResult(bool success, string errorMessage = null)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// 购买结果状态
    /// </summary>
    public enum ShopPurchaseStatus
    {
        Pending,
        Success,
        Failed
    }

    /// <summary>
    /// 购买失败原因
    /// </summary>
    public enum ShopPurchaseFailureReason
    {
        None,
        ProductNotFound,
        InvalidData,
        SoldOut,
        InsufficientCost,
        AdUnavailable,
        AdFailed,
        RewardFailed,
        SaveFailed
    }

    /// <summary>
    /// 商店购买结果
    /// </summary>
    public sealed class ShopPurchaseResult
    {
        // 商品唯一标识
        public string ProductId { get; }
        // 本次购买的处理状态
        public ShopPurchaseStatus Status { get; }
        // 购买失败原因
        public ShopPurchaseFailureReason FailureReason { get; }
        // 当前累计购买次数
        public int PurchaseCount { get; }
        // 当前累计广告次数
        public int AdViewCount { get; }
        // 本次购买对应的奖励列表
        public IReadOnlyList<ShopRewardData> Rewards { get; }
        // 失败时的错误描述
        public string ErrorMessage { get; }
        public bool IsSuccess => Status == ShopPurchaseStatus.Success;

        /// <summary>
        /// 创建购买结果
        /// </summary>
        private ShopPurchaseResult(
            string productId,
            ShopPurchaseStatus status,
            ShopPurchaseFailureReason failureReason,
            int purchaseCount,
            int adViewCount,
            IReadOnlyList<ShopRewardData> rewards,
            string errorMessage)
        {
            ProductId = productId;
            Status = status;
            FailureReason = failureReason;
            PurchaseCount = purchaseCount;
            AdViewCount = adViewCount;
            Rewards = rewards ?? Array.Empty<ShopRewardData>();
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// 创建供商店管理器使用的购买结果
        /// </summary>
        internal static ShopPurchaseResult Create(
            string productId,
            ShopPurchaseStatus status,
            ShopPurchaseFailureReason failureReason,
            int purchaseCount,
            int adViewCount,
            IReadOnlyList<ShopRewardData> rewards,
            string errorMessage = null)
        {
            return new ShopPurchaseResult(
                productId,
                status,
                failureReason,
                purchaseCount,
                adViewCount,
                rewards,
                errorMessage);
        }
    }
    #endregion
}
