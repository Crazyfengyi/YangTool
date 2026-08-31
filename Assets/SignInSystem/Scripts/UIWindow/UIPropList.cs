using System.Collections.Generic;
using UnityEngine;

namespace SignInSystem
{
    /// <summary>签到奖励列表</summary>
    public class UIPropList : MonoBehaviour
    {
        public List<ItemUI_BagProp> uiBagProps;

        /// <summary>刷新奖励项</summary>
        public void RefreshBagProp(List<SignInReward> rewards)
        {
            if (uiBagProps == null)
            {
                return;
            }

            for (int i = 0; i < uiBagProps.Count; i++)
            {
                bool visible = rewards != null && i < rewards.Count;
                uiBagProps[i]?.gameObject.SetActive(visible);
                if (visible)
                {
                    uiBagProps[i].RefreshBagPropUI(rewards[i]);
                }
            }
        }
    }
}
