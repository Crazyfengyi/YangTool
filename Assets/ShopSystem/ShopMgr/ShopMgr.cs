using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShopSystem
{
    /// <summary>
    /// 独立商店管理器
    /// </summary>
    public sealed class ShopMgr : MonoBehaviour
    {
        #region FieldsAndProperties
        /// <summary>
        /// 当前场景中的商店管理器实例
        /// </summary>
        public static ShopMgr Instance { get; private set; }

        // Inspector 中配置的商品目录
        [SerializeField] private ShopCatalog catalog;

        // 当前生效的商品副本
        private readonly List<ShopProductData> products = new();
        // 面向 UI 的商品运行时视图
        private readonly List<ItemData_ShopItem> shopList = new();
        // 按商品 ID 索引的购买存档
        private readonly Dictionary<string, ShopSaveItem> saveItems = new();
        // 存档服务
        private IShopSaveStore saveStore;
        // 道具扣除与查询服务
        private IShopInventoryService inventoryService;
        // 奖励发放服务
        private IShopRewardService rewardService;
        // 激励广告服务
        private IShopAdService adService;
        // 是否已完成首次初始化
        private bool initialized;
        // 是否正在使用运行时注入目录
        private bool runtimeDataActive;
        // 是否已加载 Inspector 配置目录
        private bool loadedSerializedCatalog;

        public IReadOnlyList<ShopProductData> Products
        {
            get
            {
                EnsureInitialized();
                return products;
            }
        }

        public IReadOnlyList<ItemData_ShopItem> ShopList
        {
            get
            {
                EnsureInitialized();
                return shopList;
            }
        }

        /// <summary>
        /// 购买流程完成时触发 包含成功或失败结果
        /// </summary>
        public event Action<ShopPurchaseResult> PurchaseCompleted;

        /// <summary>
        /// 商品目录或购买进度变化时触发
        /// </summary>
        public event Action ShopChanged;
        #endregion

        #region LifecycleAndPublicApi

        /// <summary>
        /// 初始化场景中的唯一商店管理器实例
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureInitialized();
        }

        /// <summary>
        /// 销毁时清理全局实例引用
        /// </summary>
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 配置商店外部服务
        /// </summary>
        /// <param name="saveStore">存档服务 为空时保留已有服务 未配置时使用 PlayerPrefs 实现</param>
        /// <param name="inventoryService">道具服务 为空时保留已有服务 未配置时使用内存实现</param>
        /// <param name="rewardService">奖励服务 为空时保留已有服务 未配置时使用内存实现</param>
        /// <param name="adService">广告服务 为空时保留已有服务 未配置时使用不可用实现</param>
        public void ConfigureServices(
            IShopSaveStore saveStore = null,
            IShopInventoryService inventoryService = null,
            IShopRewardService rewardService = null,
            IShopAdService adService = null)
        {
            EnsureInitialized();
            this.saveStore = saveStore ?? this.saveStore ?? new PlayerPrefsShopSaveStore();
            this.inventoryService = inventoryService ?? this.inventoryService ?? new InMemoryShopInventoryService();
            this.rewardService = rewardService ?? this.rewardService ?? new InMemoryShopRewardService();
            this.adService = adService ?? this.adService ?? new UnavailableShopAdService();
            LoadSaveData();
        }

        /// <summary>
        /// 单独替换存档服务
        /// </summary>
        /// <param name="saveStore">新的存档服务</param>
        public void ConfigureSaveStore(IShopSaveStore saveStore)
        {
            ConfigureServices(saveStore: saveStore);
        }

        /// <summary>
        /// 注入运行时商品数据并刷新商品目录
        /// </summary>
        /// <param name="injectedProducts">外部商品列表 为空时清除注入并恢复配置目录</param>
        public void InjectShopData(IReadOnlyList<ShopProductData> injectedProducts)
        {
            EnsureInitialized();
            runtimeDataActive = injectedProducts != null && injectedProducts.Count > 0;
            ReloadProducts(runtimeDataActive ? injectedProducts : null);
            ShopChanged?.Invoke();
        }

        /// <summary>
        /// 尝试购买商品
        /// </summary>
        /// <param name="productId">要购买的商品 ID</param>
        /// <returns>同步购买结果或广告购买的 Pending 结果</returns>
        public ShopPurchaseResult TryPurchase(string productId)
        {
            EnsureInitialized();
            if (!TryGetProduct(productId, out ShopProductData product))
            {
                return Complete(CreateResult(productId, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.ProductNotFound, null, "商品不存在"));
            }

            if (!product.IsValid(out string validationError))
            {
                return Complete(CreateResult(productId, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.InvalidData, product, validationError));
            }

            ShopSaveItem saveItem = GetSaveItem(product.Id);
            if (product.MaxPurchaseCount > 0 && saveItem.purchaseCount >= product.MaxPurchaseCount)
            {
                return Complete(CreateResult(productId, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.SoldOut, product, "商品已售罄"));
            }

            if (product.PurchaseMethod == ShopPurchaseMethod.RewardedAd)
            {
                return BeginAdPurchase(product, saveItem);
            }

            return CompleteCurrencyPurchase(product, saveItem);
        }

        /// <summary>
        /// 获取商品购买次数
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <returns>已完成的购买次数</returns>
        public int GetPurchaseCount(string productId)
        {
            EnsureInitialized();
            return GetSaveItem(productId).purchaseCount;
        }

        /// <summary>
        /// 获取商品广告进度
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <returns>已完成的广告观看次数</returns>
        public int GetAdViewCount(string productId)
        {
            EnsureInitialized();
            return GetSaveItem(productId).adViewCount;
        }

        /// <summary>
        /// 判断商品是否售罄
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <returns>达到最大购买次数时返回 true</returns>
        public bool IsSellOut(string productId)
        {
            if (!TryGetProduct(productId, out ShopProductData product))
            {
                return false;
            }

            int maxCount = product.MaxPurchaseCount;
            return maxCount > 0 && GetPurchaseCount(productId) >= maxCount;
        }

        /// <summary>
        /// 判断普通购买的消耗是否充足
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <returns>商品存在且消耗充足时返回 true</returns>
        public bool CanPurchase(string productId)
        {
            if (!TryGetProduct(productId, out ShopProductData product) ||
                product.PurchaseMethod != ShopPurchaseMethod.Currency)
            {
                return false;
            }

            return inventoryService.HasEnough(product.Costs);
        }
        #endregion

        #region InternalLogic

        /// <summary>
        /// 确保商品目录 服务和存档已完成初始化
        /// </summary>
        private void EnsureInitialized()
        {
            if (initialized)
            {
                EnsureServices();
                if (!runtimeDataActive && catalog != null && !loadedSerializedCatalog)
                {
                    ReloadProducts(null);
                }

                return;
            }

            initialized = true;
            EnsureServices();
            LoadSaveData();
            ReloadProducts(null);
        }

        /// <summary>
        /// 为未注入的服务补充默认实现
        /// </summary>
        private void EnsureServices()
        {
            saveStore ??= new PlayerPrefsShopSaveStore();
            inventoryService ??= new InMemoryShopInventoryService();
            rewardService ??= new InMemoryShopRewardService();
            adService ??= new UnavailableShopAdService();
        }

        /// <summary>
        /// 按数据来源优先级重建商品运行时副本
        /// </summary>
        /// <param name="source">优先使用的商品列表 为空时从配置资产加载</param>
        private void ReloadProducts(IReadOnlyList<ShopProductData> source)
        {
            products.Clear();
            shopList.Clear();
            IReadOnlyList<ShopProductData> selectedSource = source;
            loadedSerializedCatalog = false;
            if (selectedSource == null)
            {
                if (catalog != null)
                {
                    selectedSource = catalog.Products;
                    loadedSerializedCatalog = true;
                }
                else
                {
                    selectedSource = Resources.Load<ShopCatalog>("ShopCatalog")?.Products;
                }
            }

            if (selectedSource != null)
            {
                var ids = new HashSet<string>();
                for (int i = 0; i < selectedSource.Count; i++)
                {
                    ShopProductData product = selectedSource[i];
                    if (product == null)
                    {
                        Debug.LogWarning("ShopSystem 已跳过空商品");
                        continue;
                    }

                    if (!product.IsValid(out string error) || !ids.Add(product.Id))
                    {
                        Debug.LogWarning($"ShopSystem 已跳过无效或重复商品 {error ?? product.Id}");
                        continue;
                    }

                    ShopProductData clone = product.Clone();
                    products.Add(clone);
                    shopList.Add(new ItemData_ShopItem(this, clone));
                }
            }

            if (products.Count == 0)
            {
                Debug.LogWarning("ShopSystem 没有可用商品 请配置 ShopCatalog 或注入商品数据");
            }
        }

        /// <summary>
        /// 执行普通消耗购买并处理奖励和存档回滚
        /// </summary>
        /// <param name="product">待购买商品</param>
        /// <param name="saveItem">商品购买进度</param>
        /// <returns>普通购买结果</returns>
        private ShopPurchaseResult CompleteCurrencyPurchase(ShopProductData product, ShopSaveItem saveItem)
        {
            bool consumed = false;
            try
            {
                if (!inventoryService.HasEnough(product.Costs) || !inventoryService.TryConsume(product.Costs))
                {
                    return Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                        ShopPurchaseFailureReason.InsufficientCost, product, "消耗不足"));
                }

                consumed = true;
                if (!rewardService.GrantRewards(product.Id, product.Rewards))
                {
                    inventoryService.TryRefund(product.Costs);
                    return Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                        ShopPurchaseFailureReason.RewardFailed, product, "奖励发放失败"));
                }

                saveItem.purchaseCount++;
                if (!TrySaveData())
                {
                    return Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                        ShopPurchaseFailureReason.SaveFailed, product, "存档失败"));
                }

                ShopChanged?.Invoke();
                return Complete(CreateResult(product.Id, ShopPurchaseStatus.Success,
                    ShopPurchaseFailureReason.None, product, null));
            }
            catch (Exception exception)
            {
                if (consumed)
                {
                    inventoryService.TryRefund(product.Costs);
                }

                return Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.RewardFailed, product, exception.Message));
            }
        }

        /// <summary>
        /// 发起激励广告购买流程
        /// </summary>
        /// <param name="product">待购买商品</param>
        /// <param name="saveItem">商品购买进度</param>
        /// <returns>广告尚未完成时返回 Pending</returns>
        private ShopPurchaseResult BeginAdPurchase(ShopProductData product, ShopSaveItem saveItem)
        {
            try
            {
                if (!adService.IsAvailable)
                {
                    Debug.LogError("广告服务未配置");
                    return Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                        ShopPurchaseFailureReason.AdUnavailable, product, "广告服务未配置"));
                }

                adService.ShowRewardedAd(result => OnAdCompleted(product, saveItem, result));
            }
            catch (Exception exception)
            {
                return Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.AdFailed, product, exception.Message));
            }

            return CreateResult(product.Id, ShopPurchaseStatus.Pending,
                ShopPurchaseFailureReason.None, product, null);
        }

        /// <summary>
        /// 处理激励广告服务回调并推进广告购买进度
        /// </summary>
        /// <param name="product">广告购买商品</param>
        /// <param name="saveItem">商品购买进度</param>
        /// <param name="adResult">广告播放结果</param>
        private void OnAdCompleted(ShopProductData product, ShopSaveItem saveItem, ShopAdResult adResult)
        {
            if (this == null)
            {
                return;
            }

            if (adResult == null || !adResult.Success)
            {
                Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.AdFailed, product, adResult?.ErrorMessage ?? "广告失败"));
                return;
            }

            saveItem.adViewCount++;
            if (saveItem.adViewCount < Mathf.Max(1, product.RequiredAdViews))
            {
                if (!TrySaveData())
                {
                    Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                        ShopPurchaseFailureReason.SaveFailed, product, "存档失败"));
                    return;
                }

                ShopChanged?.Invoke();
                return;
            }

            if (!rewardService.GrantRewards(product.Id, product.Rewards))
            {
                Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.RewardFailed, product, "奖励发放失败"));
                return;
            }

            saveItem.purchaseCount++;
            saveItem.adViewCount = 0;
            if (!TrySaveData())
            {
                Complete(CreateResult(product.Id, ShopPurchaseStatus.Failed,
                    ShopPurchaseFailureReason.SaveFailed, product, "存档失败"));
                return;
            }

            ShopChanged?.Invoke();
            Complete(CreateResult(product.Id, ShopPurchaseStatus.Success,
                ShopPurchaseFailureReason.None, product, null));
        }

        /// <summary>
        /// 触发最终购买事件并返回结果
        /// </summary>
        /// <param name="result">购买结果</param>
        /// <returns>传入的购买结果</returns>
        private ShopPurchaseResult Complete(ShopPurchaseResult result)
        {
            if (result.Status != ShopPurchaseStatus.Pending)
            {
                PurchaseCompleted?.Invoke(result);
            }

            return result;
        }

        /// <summary>
        /// 根据当前存档状态创建购买结果
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <param name="status">购买状态</param>
        /// <param name="reason">失败原因</param>
        /// <param name="product">商品数据</param>
        /// <param name="error">错误描述</param>
        /// <returns>封装后的购买结果</returns>
        private ShopPurchaseResult CreateResult(
            string productId,
            ShopPurchaseStatus status,
            ShopPurchaseFailureReason reason,
            ShopProductData product,
            string error)
        {
            int purchaseCount = product == null ? 0 : GetPurchaseCount(productId);
            int adViewCount = product == null ? 0 : GetAdViewCount(productId);
            return ShopPurchaseResult.Create(
                productId,
                status,
                reason,
                purchaseCount,
                adViewCount,
                product?.Rewards,
                error);
        }

        /// <summary>
        /// 按商品 ID 查找当前目录中的商品
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <param name="product">找到的商品数据</param>
        /// <returns>找到商品时返回 true</returns>
        private bool TryGetProduct(string productId, out ShopProductData product)
        {
            EnsureInitialized();
            for (int i = 0; i < products.Count; i++)
            {
                if (products[i].Id == productId)
                {
                    product = products[i];
                    return true;
                }
            }

            product = null;
            return false;
        }

        /// <summary>
        /// 获取商品存档项 不存在时创建空记录
        /// </summary>
        /// <param name="productId">商品 ID</param>
        /// <returns>商品存档项</returns>
        private ShopSaveItem GetSaveItem(string productId)
        {
            if (!saveItems.TryGetValue(productId ?? string.Empty, out ShopSaveItem item))
            {
                item = new ShopSaveItem { productId = productId ?? string.Empty };
                saveItems[item.productId] = item;
            }

            return item;
        }

        /// <summary>
        /// 从当前存档服务加载购买次数和广告进度
        /// </summary>
        private void LoadSaveData()
        {
            saveItems.Clear();
            ShopSaveData data;
            try
            {
                data = saveStore.Load() ?? new ShopSaveData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"ShopSystem 存档读取失败 将使用空存档 {exception.Message}");
                data = new ShopSaveData();
            }
            if (data.items == null)
            {
                return;
            }

            for (int i = 0; i < data.items.Count; i++)
            {
                ShopSaveItem item = data.items[i];
                if (item != null && !string.IsNullOrEmpty(item.productId))
                {
                    saveItems[item.productId] = item;
                }
            }
        }

        /// <summary>
        /// 将当前购买进度保存到存档服务
        /// </summary>
        /// <returns>保存成功时返回 true</returns>
        private bool TrySaveData()
        {
            try
            {
                saveStore.Save(new ShopSaveData
                {
                    items = new List<ShopSaveItem>(saveItems.Values)
                });
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"ShopSystem 存档保存失败 {exception.Message}");
                return false;
            }
        }
        #endregion
    }
}
