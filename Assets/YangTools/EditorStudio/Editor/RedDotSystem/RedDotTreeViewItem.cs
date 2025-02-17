#if UNITY_EDITOR
using GameMain.RedDot;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GameMain
{
    public class RedDotTreeViewItem : TreeViewItem
    {
        private readonly RedDotTreeNode _redDotNode;

        public string Path { get; private set; }

        public int Value { get; private set; }

        public RedDotTreeViewItem(int id, RedDotTreeNode node)
        {
            base.id = id;

            _redDotNode = node;

            Path = node.FullPath;

            Value = node.Value;

            Debug.Log($"Id:{id} node:{node}");
        }

        public override string displayName
        {
            get =>
                $"{_redDotNode.Name}-节点值:{_redDotNode.Value} 子节点个数:{_redDotNode.ChildrenCount} 监听数量:{_redDotNode.CallBackCount}";
            set { }
        }
    }
}
#endif