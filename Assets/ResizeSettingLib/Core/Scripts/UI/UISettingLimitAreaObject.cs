using ResizeSettingLib.Core.Config;
using UnityEngine;

namespace ResizeSettingLib.Core.UI
{
    /// <summary>
    /// 限制区域对象
    /// </summary>
    public class UISettingLimitAreaObject:MonoBehaviour
    {
        [SerializeField] private RectTransform m_SelfTrans;
        
        private LimitAreaConfigNode m_ConfigNode;

        public LimitAreaConfigNode ConfigNode => m_ConfigNode;
        
        public void SetConfigData(LimitAreaConfigNode configNode)
        {
            m_ConfigNode = configNode;
            
            RefreshUI();
        }

        private void RefreshUI()
        {
            gameObject.name = m_ConfigNode.name;
            m_SelfTrans.anchorMin = new Vector2(m_ConfigNode.limitX.xMin, m_ConfigNode.limitY.yMin);
            m_SelfTrans.anchorMax = new Vector2(m_ConfigNode.limitX.xMax, m_ConfigNode.limitY.yMax);
        }
    }
}