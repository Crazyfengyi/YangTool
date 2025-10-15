#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YangTools.Scripts.Core.RedDotSystem;

namespace GameMain
{
    public class RedDotTreeViewItem : TreeViewItem
    {
        private readonly RedDotTreeNode redDotNode;
        public string Path { get; private set; }
        public int Value { get; private set; }

        public RedDotTreeViewItem(int id, RedDotTreeNode node)
        {
            base.id = id;
            redDotNode = node;
            Path = node.FullPath;
            Value = node.Value;
            Debug.Log($"Id:{id} node:{node}");
        }

        public override string displayName
        {
            get => $"{redDotNode.Name}-节点值:{redDotNode.Value} 子节点个数:{redDotNode.ChildrenCount} 监听数量:{redDotNode.CallBackCount}";
            set { }
        }
    }
}
#endif