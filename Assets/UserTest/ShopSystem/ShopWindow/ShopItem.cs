using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShopSystem
{
    /// <summary>
    /// 单个商品 UI
    /// </summary>
    public sealed class ShopItem : MonoBehaviour
    {
        // 商品名称文本
        [SerializeField] private TMP_Text itemName;
        // 商品图标
        [SerializeField] private Image icon;
        // 奖励数量文本
        [SerializeField] private TMP_Text count;
        // 广告购买按钮
        [SerializeField] private Button adBtn;
        // 普通购买按钮
        [SerializeField] private Button btn;
        // 购买按钮上的消耗图标
        [SerializeField] private Image btnIcon;
        // 购买按钮上的消耗文本
        [SerializeField] private TMP_Text btnText;
        // 购买状态文本
        [SerializeField] private TMP_Text statusText;
        // 售罄状态显示节点
        [SerializeField] private GameObject sellOutShow;

        // 当前绑定的商品运行时数据
        private ItemData_ShopItem data;

        /// <summary>
        /// 禁用时移除按钮监听
        /// </summary>
        private void OnDisable()
        {
            if (adBtn != null)
            {
                adBtn.onClick.RemoveListener(OnAdBtnClick);
            }

            if (btn != null)
            {
                btn.onClick.RemoveListener(OnBtnClick);
            }
        }

        /// <summary>
        /// 绑定商品数据并刷新显示
        /// </summary>
        /// <param name="shopData">要显示的商品运行时数据</param>
        public void SetData(ItemData_ShopItem shopData)
        {
            data = shopData;
            if (data == null || data.Config == null)
            {
                return;
            }

            ShopProductData product = data.Config;
            if (itemName != null)
            {
                itemName.text = product.DisplayName;
            }

            if (icon != null)
            {
                icon.sprite = product.Icon;
                icon.enabled = product.Icon != null;
            }

            if (count != null)
            {
                count.text = FormatRewards(product);
            }

            if (btnIcon != null)
            {
                btnIcon.sprite = GetFirstCostIcon(product);
                btnIcon.enabled = btnIcon.sprite != null;
            }

            if (btnText != null)
            {
                btnText.text = FormatCosts(product);
            }

            bool soldOut = data.IsSellOut;
            bool adPurchase = product.PurchaseMethod == ShopPurchaseMethod.RewardedAd;
            if (adBtn != null)
            {
                adBtn.gameObject.SetActive(adPurchase);
                adBtn.interactable = adPurchase && !soldOut;
            }

            if (btn != null)
            {
                btn.gameObject.SetActive(!adPurchase);
                btn.interactable = !adPurchase && !soldOut && data.IsCanPurchase();
            }

            if (sellOutShow != null)
            {
                sellOutShow.SetActive(soldOut);
            }

            if (statusText != null)
            {
                statusText.text = adPurchase
                    ? $"Ads {data.LookAdCount}/{Mathf.Max(1, data.TotalLookAdCount)}"
                    : string.Empty;
            }

            if (adBtn != null)
            {
                adBtn.onClick.RemoveListener(OnAdBtnClick);
                adBtn.onClick.AddListener(OnAdBtnClick);
            }

            if (btn != null)
            {
                btn.onClick.RemoveListener(OnBtnClick);
                btn.onClick.AddListener(OnBtnClick);
            }
        }

        /// <summary>
        /// 响应广告购买按钮点击
        /// </summary>
        private void OnAdBtnClick()
        {
            TryPurchase();
        }

        /// <summary>
        /// 响应普通购买按钮点击
        /// </summary>
        private void OnBtnClick()
        {
            TryPurchase();
        }

        /// <summary>
        /// 发起当前商品购买并刷新按钮状态
        /// </summary>
        private void TryPurchase()
        {
            if (data == null)
            {
                return;
            }

            ShopPurchaseResult result = data.BuyItem();
            if (result.Status == ShopPurchaseStatus.Failed && statusText != null)
            {
                statusText.text = result.ErrorMessage;
            }

            SetData(data);
        }

        /// <summary>
        /// 将奖励列表格式化为 UI 文本
        /// </summary>
        /// <param name="product">商品配置</param>
        /// <returns>奖励展示文本</returns>
        private static string FormatRewards(ShopProductData product)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < product.Rewards.Count; i++)
            {
                ShopRewardData reward = product.Rewards[i];
                if (reward == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append("  ");
                }

                builder.Append(reward.ItemId).Append(" x").Append(reward.Amount);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 将消耗列表格式化为按钮文本
        /// </summary>
        /// <param name="product">商品配置</param>
        /// <returns>消耗展示文本</returns>
        private static string FormatCosts(ShopProductData product)
        {
            if (product.Costs == null || product.Costs.Count == 0)
            {
                return product.PurchaseMethod == ShopPurchaseMethod.RewardedAd ? "Ads" : "Free";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < product.Costs.Count; i++)
            {
                ShopCostData cost = product.Costs[i];
                if (cost == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append("  ");
                }

                builder.Append(cost.ItemId).Append(" x").Append(cost.Amount);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 获取商品第一个消耗项的图标
        /// </summary>
        /// <param name="product">商品配置</param>
        /// <returns>第一个消耗项图标 没有时返回空</returns>
        private static Sprite GetFirstCostIcon(ShopProductData product)
        {
            return product.Costs != null && product.Costs.Count > 0
                ? product.Costs[0]?.Icon
                : null;
        }
    }
}
