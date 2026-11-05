using System.Collections.Generic;

namespace ResizeSettingLib.Core.Config
{
    /// <summary>
    /// 限制区域节点
    /// </summary>
    public class LimitAreaConfigNode:ConfigNodeBase
    {
        /// <summary>
        /// 限制那些节点不能在该区域使用
        /// </summary>
        public List<int> limitIds;

        /// <summary>
        /// x定位（左下角开始，百分比定位） 
        /// </summary>
        public (float xMin,float xMax) limitX;
        
        /// <summary>
        /// y定位（左下角开始，百分比定位）
        /// </summary>
        public (float yMin,float yMax) limitY;

        public LimitAreaConfigNode CopySelf()
        {
            LimitAreaConfigNode node = new LimitAreaConfigNode();
            node.id = this.id;
            node.limitIds = new List<int>();
            node.limitIds.AddRange(this.limitIds);
            node.limitX = (this.limitX.xMin,this.limitX.xMax);
            node.limitY = (this.limitY.yMin,this.limitY.yMax);
            return node;
        }
    }
}