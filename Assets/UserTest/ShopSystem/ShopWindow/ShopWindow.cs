using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ShopSystem
{
    /// <summary>
    /// 商店窗口列表控制器
    /// </summary>
    public sealed class ShopWindow : MonoBehaviour
    {
        // 商品管理器引用 未配置时尝试使用全局实例
        [SerializeField] private ShopMgr shopManager;
        // 商品条目预制体
        [SerializeField] private ShopItem shopItem;
        // 商品条目容器
        [SerializeField] private RectTransform shopItemContent;

        // 关闭窗口按钮
        public Button closeButton;
        // 当前窗口生成的商品条目
        private readonly List<ShopItem> allShopItems = new();

        /// <summary>
        /// 初始化管理器引用和关闭按钮事件
        /// </summary>
        private void Awake()
        {
            shopManager ??= ShopMgr.Instance;
            closeButton.onClick.AddListener(OnCloseClick);
        }
        
        /// <summary>
        /// 启用窗口时订阅商品变化并刷新列表
        /// </summary>
        private void OnEnable()
        {
            shopManager ??= ShopMgr.Instance;
            if (shopManager == null)
            {
                Debug.LogWarning("ShopSystem ShopWindow 未找到 ShopMgr");
                return;
            }

            shopManager.ShopChanged += Refresh;
            Refresh();
        }

        /// <summary>
        /// 在管理器稍后初始化时补充刷新一次
        /// </summary>
        private void Start()
        {
            shopManager ??= ShopMgr.Instance;
            if (shopManager != null)
            {
                Refresh();
            }
        }

        /// <summary>
        /// 禁用窗口时取消商品变化事件订阅
        /// </summary>
        private void OnDisable()
        {
            if (shopManager != null)
            {
                shopManager.ShopChanged -= Refresh;
            }
        }

        /// <summary>
        /// 刷新商品列表
        /// </summary>
        public void Refresh()
        {
            if (shopManager == null || shopItemContent == null || shopItem == null)
            {
                return;
            }

            for (int i = shopItemContent.childCount - 1; i >= 0; i--)
            {
                Destroy(shopItemContent.GetChild(i).gameObject);
            }

            allShopItems.Clear();
            IReadOnlyList<ItemData_ShopItem> data = shopManager.ShopList;
            for (int i = 0; i < data.Count; i++)
            {
                ShopItem item = Instantiate(shopItem, shopItemContent);
                item.SetData(data[i]);
                allShopItems.Add(item);
            }
        }
        
        /// <summary>
        /// 响应关闭按钮并隐藏窗口
        /// </summary>
        private void OnCloseClick()
        {
            gameObject.SetActive(false);
        }
    }
}
