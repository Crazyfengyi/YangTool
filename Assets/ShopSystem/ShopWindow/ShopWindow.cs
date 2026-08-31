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
        [SerializeField] private ShopMgr shopManager;
        [SerializeField] private ShopItem shopItem;
        [SerializeField] private RectTransform shopItemContent;

        public Button closeButton;
        private readonly List<ShopItem> allShopItems = new();

        private void Awake()
        {
            shopManager ??= ShopMgr.Instance;
            closeButton.onClick.AddListener(OnCloseClick);
        }
        
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

        private void Start()
        {
            //确保管理器在其他对象稍后初始化时仍能刷新一次列表
            shopManager ??= ShopMgr.Instance;
            if (shopManager != null)
            {
                Refresh();
            }
        }

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
        
        private void OnCloseClick()
        {
            gameObject.SetActive(false);
        }
    }
}
