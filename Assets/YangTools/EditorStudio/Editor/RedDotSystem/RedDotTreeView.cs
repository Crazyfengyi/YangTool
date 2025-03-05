#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using GameMain.RedDot;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace GameMain
{
    public class RedDotTreeView : TreeView
    {
        private RedDotTreeViewItem _root;

        private int _id;
        
        public RedDotTreeView(TreeViewState state) : base(state)
        {
            Reload();

            useScrollView = true;

            RedDotMgr.Instance.NodeNumChangeCallback += Reload;
            RedDotMgr.Instance.NodeValueChangeCallback += Repaint;
            RedDotMgr.Instance.CallBackNumberChange += Repaint;
        }

        private void Repaint(RedDotTreeNode arg1, int arg2)
        {
            Repaint();
        }

        public RedDotTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            _id = 0;

            _root = PreOrder(RedDotMgr.Instance.RootNode);
            _root.depth = -1;

            SetupDepthsFromParentsAndChildren(_root);

            return _root;
        }

        private RedDotTreeViewItem PreOrder(RedDotTreeNode root)
        {
            if (root == null) return null;

            var item = new RedDotTreeViewItem(_id++, root);

            if (root.ChildrenCount > 0)
            {
                foreach (var child in root.Children)
                {
                    item.AddChild(PreOrder(child));
                }
            }

            return item;
        }

        public void OnDestroy()
        {
            RedDotMgr.Instance.NodeNumChangeCallback -= Reload;
            RedDotMgr.Instance.NodeValueChangeCallback -= Repaint;
            RedDotMgr.Instance.CallBackNumberChange -= Repaint;
        }
    }
}
#endif