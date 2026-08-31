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
        public static ShopMgr Instance { get; private set; }

        [SerializeField] private ShopCatalog catalog;

        private readonly List<ShopProductData> products = new();
        private readonly List<ItemData_ShopItem> shopList = new();
        private readonly Dictionary<string, ShopSaveItem> saveItems = new();
        private IShopSaveStore saveStore;
        private IShopInventoryService inventoryService;
        private IShopRewardService rewardService;
        private IShopAdService adService;
        private bool initialized;
        private bool runtimeDataActive;
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

        public event Action<ShopPurchaseResult> PurchaseCompleted;
        public event Action ShopChanged;
        #endregion

        #region LifecycleAndPublicApi

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
        public void ConfigureSaveStore(IShopSaveStore saveStore)
        {
            ConfigureServices(saveStore: saveStore);
        }

        /// <summary>
        /// 注入运行时商品数据
        /// </summary>
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
        public int GetPurchaseCount(string productId)
        {
            EnsureInitialized();
            return GetSaveItem(productId).purchaseCount;
        }

        /// <summary>
        /// 获取商品广告进度
        /// </summary>
        public int GetAdViewCount(string productId)
        {
            EnsureInitialized();
            return GetSaveItem(productId).adViewCount;
        }

        /// <summary>
        /// 判断商品是否售罄
        /// </summary>
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

        private void EnsureServices()
        {
            saveStore ??= new PlayerPrefsShopSaveStore();
            inventoryService ??= new InMemoryShopInventoryService();
            rewardService ??= new InMemoryShopRewardService();
            adService ??= new UnavailableShopAdService();
        }

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

        private ShopPurchaseResult Complete(ShopPurchaseResult result)
        {
            if (result.Status != ShopPurchaseStatus.Pending)
            {
                PurchaseCompleted?.Invoke(result);
            }

            return result;
        }

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

        private ShopSaveItem GetSaveItem(string productId)
        {
            if (!saveItems.TryGetValue(productId ?? string.Empty, out ShopSaveItem item))
            {
                item = new ShopSaveItem { productId = productId ?? string.Empty };
                saveItems[item.productId] = item;
            }

            return item;
        }

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
