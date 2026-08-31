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
        Currency,
        RewardedAd
    }

    /// <summary>
    /// 商品配置数据
    /// </summary>
    [Serializable]
    public sealed class ShopProductData
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<ShopRewardData> rewards = new();
        [SerializeField] private List<ShopCostData> costs = new();
        [SerializeField] private ShopPurchaseMethod purchaseMethod;
        [SerializeField] private int maxPurchaseCount;
        [SerializeField] private int requiredAdViews;

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public IReadOnlyList<ShopRewardData> Rewards => rewards;
        public IReadOnlyList<ShopCostData> Costs => costs;
        public ShopPurchaseMethod PurchaseMethod => purchaseMethod;
        public int MaxPurchaseCount => maxPurchaseCount;
        public int RequiredAdViews => requiredAdViews;

        public ShopProductData()
        {
        }

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
        [SerializeField] private string itemId;
        [SerializeField] private int amount;
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public int Amount => amount;
        public Sprite Icon => icon;

        public ShopRewardData()
        {
        }

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
        [SerializeField] private string itemId;
        [SerializeField] private int amount;
        [SerializeField] private Sprite icon;

        public string ItemId => itemId;
        public int Amount => amount;
        public Sprite Icon => icon;

        public ShopCostData()
        {
        }

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
        public string productId;
        public int purchaseCount;
        public int adViewCount;
    }

    /// <summary>
    /// 商店存档数据
    /// </summary>
    [Serializable]
    public sealed class ShopSaveData
    {
        public List<ShopSaveItem> items = new();
    }

    /// <summary>
    /// 广告回调结果
    /// </summary>
    public sealed class ShopAdResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }

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
        public string ProductId { get; }
        public ShopPurchaseStatus Status { get; }
        public ShopPurchaseFailureReason FailureReason { get; }
        public int PurchaseCount { get; }
        public int AdViewCount { get; }
        public IReadOnlyList<ShopRewardData> Rewards { get; }
        public string ErrorMessage { get; }
        public bool IsSuccess => Status == ShopPurchaseStatus.Success;

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
