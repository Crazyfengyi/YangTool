using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SignInSystem
{
    /// <summary>
    /// 签到奖励显示项
    /// </summary>
    public class ItemUI_BagProp : MonoBehaviour
    {
        public Image mImgPropBg;
        public TextMeshProUGUI mTxtPropCount;
        public Image mImgPropIcon;
        public TextMeshProUGUI mTextPropName;
        public Image debrisImage;
        public List<Sprite> bgList = new List<Sprite>();
        public Button clickBtn;
        public bool canClick;

        /// <summary>刷新奖励内容</summary>
        public void RefreshBagPropUI(SignInReward reward)
        {
            if (reward == null)
            {
                return;
            }

            if (mTxtPropCount != null)
            {
                mTxtPropCount.text = $"x{reward.amount}";
            }

            if (mTextPropName != null)
            {
                mTextPropName.text = reward.rewardId;
            }

            if (mImgPropIcon != null && reward.icon != null)
            {
                mImgPropIcon.sprite = reward.icon;
            }
        }
    }
}
