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
        [SerializeField] private ShopMgr shopManager;

        private void Awake()
        {
            shopManager ??= ShopMgr.Instance;
        }
    }
}
