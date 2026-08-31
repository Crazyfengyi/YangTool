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
        [SerializeField] private List<ShopProductData> products = new();

        public IReadOnlyList<ShopProductData> Products => products;
    }
}
