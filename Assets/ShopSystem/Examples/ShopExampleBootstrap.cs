using System.Collections.Generic;
using UnityEngine;

namespace ShopSystem
{
    /// <summary>
    /// 示例场景运行时商品注入器
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class ShopExampleBootstrap : MonoBehaviour
    {
        // 示例场景中的商店管理器
        [SerializeField] private ShopMgr shopManager;

        /// <summary>
        /// 初始化示例所需的商店管理器引用
        /// </summary>
        private void Awake()
        {
            shopManager ??= ShopMgr.Instance;
        }
    }
}
