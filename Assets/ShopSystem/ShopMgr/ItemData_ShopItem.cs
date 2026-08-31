namespace ShopSystem
{
    /// <summary>
    /// 商品运行时数据视图
    /// </summary>
    public sealed class ItemData_ShopItem
    {
        // 提供购买状态查询和购买操作的商店管理器
        private readonly ShopMgr manager;

        public ShopProductData Config { get; }
        public int LookAdCount => manager.GetAdViewCount(Config.Id);
        public int TotalLookAdCount => Config.RequiredAdViews;
        public bool CanGetAward => TotalLookAdCount > 0 && LookAdCount >= TotalLookAdCount;
        public bool IsSellOut => manager.IsSellOut(Config.Id);

        /// <summary>
        /// 创建商品运行时数据视图
        /// </summary>
        /// <param name="manager">所属商店管理器</param>
        /// <param name="config">商品配置数据</param>
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
        /// 发起一次当前商品的购买请求
        /// </summary>
        public void AddLookAdCount()
        {
            manager.TryPurchase(Config.Id);
        }
    }
}
