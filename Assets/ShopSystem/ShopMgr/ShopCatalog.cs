using System.Collections.Generic;
using UnityEngine;

namespace ShopSystem
{
    /// <summary>
    /// 商店配置资产
    /// </summary>
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Shop System/Shop Catalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        // 商品目录中序列化保存的商品列表
        [SerializeField] private List<ShopProductData> products = new();

        public IReadOnlyList<ShopProductData> Products => products;
    }
}
