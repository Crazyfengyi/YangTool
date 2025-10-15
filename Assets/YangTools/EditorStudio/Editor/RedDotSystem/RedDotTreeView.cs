#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YangTools.Scripts.Core.RedDotSystem;

namespace GameMain
{
    public class RedDotTreeView : TreeView
    {
        private RedDotTreeViewItem root;
        private int id;
        
        public RedDotTreeView(TreeViewState state) : base(state)
        {
            Reload();
            useScrollView = true;
            
            RedDotMgr.Instance.NodeNumChangeCallback += Reload;
            RedDotMgr.Instance.NodeValueChangeCallback += Repaint;
            RedDotMgr.Instance.CallBackNumberChange += Repaint;
        }

        public RedDotTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
        }

        public void OnDestroy()
        {
            RedDotMgr.Instance.NodeNumChangeCallback -= Reload;
            RedDotMgr.Instance.NodeValueChangeCallback -= Repaint;
            RedDotMgr.Instance.CallBackNumberChange -= Repaint;
        }
        
        private void Repaint(RedDotTreeNode arg1, int arg2)
        {
            Repaint();
        }
        /// <summary>
        /// 构建节点
        /// </summary>
        /// <returns></returns>
        protected override TreeViewItem BuildRoot()
        {
            id = 0;
            root = PreOrder(RedDotMgr.Instance.RootNode);
            root.depth = -1;
            //更新子节点深度
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
        /// <summary>
        /// 预先排序
        /// </summary>
        private RedDotTreeViewItem PreOrder(RedDotTreeNode root)
        {
            if (root == null) return null;

            RedDotTreeViewItem item = new RedDotTreeViewItem(id++, root);
            if (root.ChildrenCount > 0)
            {
                foreach (var child in root.Children)
                {
                    item.AddChild(PreOrder(child));
                }
            }

            return item;
        }
    }
}
#endif