using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SignInSystem
{
    /// <summary>
    /// 单日签到项
    /// </summary>
    public class ItemUI_DailySign : MonoBehaviour
    {
        public GameObject mStatus_NoGet;
        public GameObject mStatus_CanGet;
        public GameObject mStatus_CanAdGet;
        public GameObject mStatus_IsGet;
        public Button mBtnLookAd;
        public List<TextMeshProUGUI> mTxtDay;
        public UIPropList mUIPropList;
        public Button mBtnGetAward;
        public List<GameObject> adsShowList;
        public List<GameObject> completeActiveList;

        /// <summary>
        /// 当前签到项可领取状态提示动画
        /// </summary>
        private DoRotateAniScript rotateAniScript;

        private int dayIndex = -1;

        /// <summary>
        /// 缓存签到项提示动画组件
        /// </summary>
        private void Awake()
        {
            rotateAniScript = GetComponent<DoRotateAniScript>();
        }

        /// <summary>绑定单日签到按钮事件</summary>
        private void Start()
        {
            mBtnGetAward?.onClick.AddListener(ClaimDay);
            mBtnLookAd?.onClick.AddListener(ClaimDayWithDoubleReward);
        }

        /// <summary>刷新签到项显示</summary>
        public void RefreshItemUI(SignInDayData data, SignInDayState state, int index)
        {
            dayIndex = index;
            bool canClaim = state == SignInDayState.Available;
            SetActive(mStatus_NoGet, state == SignInDayState.Locked);
            SetActive(mStatus_CanGet, canClaim);
            SetActive(mStatus_CanAdGet, false);
            SetActive(mStatus_IsGet, state == SignInDayState.Claimed);
            SetActiveList(adsShowList, false);
            SetActiveList(completeActiveList, state == SignInDayState.Claimed);
            string dayText = string.IsNullOrEmpty(data.title) ? $"第{index + 1}天" : data.title;
            if (mTxtDay != null)
            {
                for (int i = 0; i < mTxtDay.Count; i++)
                {
                    if (mTxtDay[i] != null)
                    {
                        mTxtDay[i].text = dayText;
                    }
                }
            }

            mUIPropList?.RefreshBagProp(data.rewards);
            SetButtonActive(mBtnGetAward, canClaim);
            SetButtonActive(mBtnLookAd, canClaim);
        }

        /// <summary>
        /// 设置当前签到项的可签到提示动画
        /// </summary>
        public void SetSignAnimation(bool active)
        {
            if (active)
            {
                rotateAniScript?.StartAni();
            }
            else
            {
                rotateAniScript?.EndAni();
            }
        }

        /// <summary>领取单日奖励</summary>
        private void ClaimDay()
        {
            if (SignInManager.Instance.ClaimDay(dayIndex))
            {
                FindFirstObjectByType<SevenSignWindow>()?.Refresh();
            }
        }

        /// <summary>领取双倍单日奖励</summary>
        private void ClaimDayWithDoubleReward()
        {
            if (SignInManager.Instance.ClaimDay(dayIndex, 2))
            {
                FindFirstObjectByType<SevenSignWindow>()?.Refresh();
            }
        }

        /// <summary>安全设置对象显隐</summary>
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        /// <summary>批量设置对象显隐</summary>
        private static void SetActiveList(List<GameObject> targets, bool active)
        {
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Count; i++)
            {
                SetActive(targets[i], active);
            }
        }

        /// <summary>设置按钮显隐</summary>
        private static void SetButtonActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }
    }
}
