using System.Collections.Generic;

namespace ResizeSettingLib.Core.Config
{
    /// <summary>
    /// 配置数据
    /// </summary>
    public class ResizeConfigData
    {
        /// <summary>
        /// 拖动类配置节点数据
        /// </summary>
        public Dictionary<int,DragObjectConfigNode> buttonConfigNodes;

        /// <summary>
        /// 限制区域配置节点数据
        /// </summary>
        public List<LimitAreaConfigNode> limitAreaConfigNodes;
        
        /// <summary>
        /// 开关类配置节点数据
        /// </summary>
        // public List<ToggleConfigNode> toggleConfigNodes;

        public ResizeConfigData CopySelf()
        {
            ResizeConfigData resizeConfigData = new ResizeConfigData();
            resizeConfigData.buttonConfigNodes = new Dictionary<int, DragObjectConfigNode>();
            foreach (KeyValuePair<int,DragObjectConfigNode> kv in this.buttonConfigNodes)
            {
                DragObjectConfigNode node = kv.Value.CopySelf();
                resizeConfigData.buttonConfigNodes.Add(kv.Key,node);
            }

            resizeConfigData.limitAreaConfigNodes = new List<LimitAreaConfigNode>();
            foreach (LimitAreaConfigNode node in this.limitAreaConfigNodes)
            {
                LimitAreaConfigNode newNode = node.CopySelf();
                resizeConfigData.limitAreaConfigNodes.Add(newNode);
            }

            return resizeConfigData;
        }
    }
}