#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using YangTools.Scripts.Core.RedDotSystem;

namespace GameMain
{
    /// <summary>
    /// 红点树视图节点
    /// </summary>
    public class RedDotTreeViewItem : TreeViewItem<int>
    {
        private readonly RedDotTreeNode redDotNode; //红点数据节点
        public string Path => redDotNode.FullPath;
        public int Value => redDotNode.Value;

        /// <summary>
        /// 创建红点树视图节点
        /// </summary>
        /// <param name="id">视图编号</param>
        /// <param name="node">红点数据节点</param>
        public RedDotTreeViewItem(int id, RedDotTreeNode node)
        {
            base.id = id;
            redDotNode = node;
        }

        public override string displayName
        {
            get => $"{redDotNode.Name}-节点值:{redDotNode.Value} 子节点个数:{redDotNode.ChildrenCount} 监听数量:{redDotNode.CallBackCount}";
            set { }
        }
    }
}
#endif
