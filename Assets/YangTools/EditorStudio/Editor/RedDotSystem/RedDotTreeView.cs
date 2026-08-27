#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using YangTools.Scripts.Core.RedDotSystem;

namespace GameMain
{
    /// <summary>
    /// 红点树编辑器视图
    /// </summary>
    public class RedDotTreeView : TreeView<int>
    {
        private RedDotTreeViewItem root; //根视图节点
        private int id; //视图节点编号
        private bool isSubscribed; //是否已订阅红点事件

        /// <summary>
        /// 创建单列红点树视图
        /// </summary>
        /// <param name="state">树视图状态</param>
        public RedDotTreeView(TreeViewState<int> state) : base(state)
        {
            Initialize();
        }

        /// <summary>
        /// 创建多列红点树视图
        /// </summary>
        /// <param name="state">树视图状态</param>
        /// <param name="multiColumnHeader">多列表头</param>
        public RedDotTreeView(TreeViewState<int> state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            Initialize();
        }

        /// <summary>
        /// 初始化视图并订阅红点事件
        /// </summary>
        private void Initialize()
        {
            useScrollView = true;
            SubscribeEvents();
            Reload();
        }

        /// <summary>
        /// 释放视图事件
        /// </summary>
        public void OnDestroy()
        {
            if (!isSubscribed)
            {
                return;
            }

            RedDotMgr.Instance.NodeNumChangeCallback -= Reload;
            RedDotMgr.Instance.NodeValueChangeCallback -= Repaint;
            RedDotMgr.Instance.CallBackNumberChange -= Repaint;
            isSubscribed = false;
        }

        /// <summary>
        /// 订阅红点事件
        /// </summary>
        private void SubscribeEvents()
        {
            if (isSubscribed)
            {
                return;
            }

            RedDotMgr.Instance.NodeNumChangeCallback += Reload;
            RedDotMgr.Instance.NodeValueChangeCallback += Repaint;
            RedDotMgr.Instance.CallBackNumberChange += Repaint;
            isSubscribed = true;
        }

        /// <summary>
        /// 重绘节点值
        /// </summary>
        /// <param name="node">变化节点</param>
        /// <param name="value">节点值</param>
        private void Repaint(RedDotTreeNode node, int value)
        {
            Repaint();
        }

        /// <summary>
        /// 构建节点
        /// </summary>
        /// <returns>根视图节点</returns>
        protected override TreeViewItem<int> BuildRoot()
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
        /// <param name="node">红点节点</param>
        /// <returns>视图节点</returns>
        private RedDotTreeViewItem PreOrder(RedDotTreeNode node)
        {
            if (node == null)
            {
                return null;
            }

            RedDotTreeViewItem item = new RedDotTreeViewItem(id++, node);
            if (node.Children != null)
            {
                foreach (RedDotTreeNode child in node.Children)
                {
                    item.AddChild(PreOrder(child));
                }
            }

            return item;
        }
    }
}
#endif
