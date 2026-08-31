namespace ShopSystem
{
    /// <summary>
    /// 商品运行时数据视图
    /// </summary>
    public sealed class ItemData_ShopItem
    {
        private readonly ShopMgr manager;

        public ShopProductData Config { get; }
        public int LookAdCount => manager.GetAdViewCount(Config.Id);
        public int TotalLookAdCount => Config.RequiredAdViews;
        public bool CanGetAward => TotalLookAdCount > 0 && LookAdCount >= TotalLookAdCount;
        public bool IsSellOut => manager.IsSellOut(Config.Id);

        internal ItemData_ShopItem(ShopMgr manager, ShopProductData config)
        {
            this.manager = manager;
            Config = config;
        }

        /// <summary>
        /// 购买当前商品
        /// </summary>
        public ShopPurchaseResult BuyItem()
        {
            return manager.TryPurchase(Config.Id);
        }

        /// <summary>
        /// 检查当前商品消耗是否充足
        /// </summary>
        public bool IsCanPurchase()
        {
            return manager.CanPurchase(Config.Id);
        }

        /// <summary>
        /// 增加广告进度
        /// </summary>
        public void AddLookAdCount()
        {
            manager.TryPurchase(Config.Id);
        }
    }
}
