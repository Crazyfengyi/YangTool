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
        [SerializeField] private TMP_Text itemName;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text count;
        [SerializeField] private Button adBtn;
        [SerializeField] private Button btn;
        [SerializeField] private Image btnIcon;
        [SerializeField] private TMP_Text btnText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject sellOutShow;

        private ItemData_ShopItem data;

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

        private void OnAdBtnClick()
        {
            TryPurchase();
        }

        private void OnBtnClick()
        {
            TryPurchase();
        }

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

        private static Sprite GetFirstCostIcon(ShopProductData product)
        {
            return product.Costs != null && product.Costs.Count > 0
                ? product.Costs[0]?.Icon
                : null;
        }
    }
}
