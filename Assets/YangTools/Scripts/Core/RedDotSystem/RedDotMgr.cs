using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YangTools.Scripts.Core.RedDotSystem
{
    /// <summary>
    /// 管理红点树节点和值传播
    /// </summary>
    public class RedDotMgr
    {
        private static RedDotMgr instance; //单例实例
        public static RedDotMgr Instance => instance ??= new RedDotMgr();

        private readonly Dictionary<string, RedDotTreeNode> allTreeNodes; //路径节点缓存
        private readonly HashSet<RedDotTreeNode> dirtyNodes; //等待更新的父节点
        private readonly List<RedDotTreeNode> tempDirtyNodes; //更新节点临时列表
        private readonly List<string> tempRemovedPaths; //删除路径临时列表
        private bool isRefreshingDirtyNodes; //是否正在刷新父节点

        public char SplitChar { get; private set; }

        public StringBuilder CachedStrBuilder { get; private set; }
        
        public RedDotTreeNode RootNode { get; private set; }

        public Action NodeNumChangeCallback { get; set; }
        public Action<RedDotTreeNode, int> NodeValueChangeCallback { get; set; }
        public Action CallBackNumberChange { get; set; }

        /// <summary>
        /// 初始化红点树
        /// </summary>
        private RedDotMgr()
        {
            SplitChar = '/';
            allTreeNodes = new Dictionary<string, RedDotTreeNode>();
            RootNode = new RedDotTreeNode("Root");
            CachedStrBuilder = new StringBuilder();
            dirtyNodes = new HashSet<RedDotTreeNode>();
            tempDirtyNodes = new List<RedDotTreeNode>();
            tempRemovedPaths = new List<string>();
        }

        #region 节点刷新

        /// <summary>
        /// 刷新等待聚合的父节点
        /// </summary>
        /// <param name="deltaTime">帧间隔</param>
        public void OnUpdate(float deltaTime)
        {
            RefreshDirtyNodes();
        }

        /// <summary>
        /// 标记并立即刷新父节点
        /// </summary>
        /// <param name="node">需要刷新的节点</param>
        public void MarkDirtyNode(RedDotTreeNode node)
        {
            if (node == null || node == RootNode)
            {
                return;
            }

            dirtyNodes.Add(node);
            RefreshDirtyNodes();
        }

        /// <summary>
        /// 更新所有脏节点直到整条父链稳定
        /// </summary>
        private void RefreshDirtyNodes()
        {
            if (isRefreshingDirtyNodes || dirtyNodes.Count == 0)
            {
                return;
            }

            isRefreshingDirtyNodes = true;
            try
            {
                while (dirtyNodes.Count > 0)
                {
                    tempDirtyNodes.Clear();
                    foreach (RedDotTreeNode node in dirtyNodes)
                    {
                        tempDirtyNodes.Add(node);
                    }

                    dirtyNodes.Clear();
                    for (int i = 0; i < tempDirtyNodes.Count; i++)
                    {
                        if (IsNodeAttachedToRoot(tempDirtyNodes[i]))
                        {
                            tempDirtyNodes[i].ChangeValue();
                        }
                    }
                }
            }
            finally
            {
                tempDirtyNodes.Clear();
                isRefreshingDirtyNodes = false;
            }
        }

        #endregion

        #region 路径和公开接口

        /// <summary>
        /// 检查路径是否合法
        /// </summary>
        /// <param name="path">节点路径</param>
        private void CheckPathIsValid(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("红点树路径不能为空", nameof(path));
            }

            if (path.StartsWith(SplitChar) || path.EndsWith(SplitChar))
            {
                throw new ArgumentException($"红点树路径不能以 {SplitChar} 开头或结尾 path={path}", nameof(path));
            }

            for (int i = 1; i < path.Length; i++)
            {
                if (path[i] == SplitChar && path[i - 1] == SplitChar)
                {
                    throw new ArgumentException($"红点树路径不能包含连续分隔符 path={path}", nameof(path));
                }
            }

            if (path.IndexOf("{0}", StringComparison.Ordinal) >= 0)
            {
                throw new ArgumentException($"红点树路径不能包含未格式化占位符 path={path}", nameof(path));
            }
        }

        /// <summary>
        /// 添加节点值监听
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="callback">节点值变化回调</param>
        /// <returns>监听节点</returns>
        public RedDotTreeNode AddListener(string path, Action<RedDotTreeNode> callback)
        {
            try
            {
                CheckPathIsValid(path);

                if (callback == null)
                {
                    return null;
                }

                RedDotTreeNode node = GetTreeNode(path);
                node.AddListener(callback);
                return node;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());

                return null;
            }
        }

        /// <summary>
        /// 移除节点值监听
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="callback">节点值变化回调</param>
        public void RemoveListener(string path, Action<RedDotTreeNode> callback)
        {
            try
            {
                CheckPathIsValid(path);

                if (callback != null && TryGetExistingNode(path, out RedDotTreeNode node))
                {
                    node.RemoveListener(callback);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 移除节点的全部监听
        /// </summary>
        /// <param name="path">节点路径</param>
        public void RemoveAllListener(string path)
        {
            try
            {
                CheckPathIsValid(path);
                if (TryGetExistingNode(path, out RedDotTreeNode node))
                {
                    node.RemoveAllListener();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 修改叶子节点值
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="newValue">新值</param>
        public void ChangeValue(string path, int newValue)
        {
            try
            {
                CheckPathIsValid(path);
                RedDotTreeNode node = GetTreeNode(path);
                node.ChangeValue(newValue);
            }
            catch (Exception e)
            {
                Debug.LogError($"path={path} newValue={newValue} {e}");
            }
        }

        /// <summary>
        /// 获得现有节点值
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <returns>节点不存在时返回零</returns>
        public int GetValue(string path)
        {
            CheckPathIsValid(path);
            return TryGetExistingNode(path, out RedDotTreeNode node) ? node.Value : 0;
        }

        /// <summary>
        /// 获得节点并在不存在时创建完整路径
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <returns>目标节点</returns>
        public RedDotTreeNode GetTreeNode(string path)
        {
            try
            {
                CheckPathIsValid(path);
                allTreeNodes.TryGetValue(path, out RedDotTreeNode node);
                if (node != null)
                {
                    return node;
                }

                RedDotTreeNode cur = RootNode;
                int length = path.Length;
                int startIndex = 0;
                for (var i = 0; i < length; i++)
                {
                    if (path[i] != SplitChar) continue;
                    if (i == length - 1)
                    {
                        throw new Exception($"路径不合法, 不能以路径分隔符结尾 path:{path}");
                    }

                    int endIndex = i - 1;
                    if (endIndex < startIndex)
                    {
                        throw new Exception($"路径不合法,不能存在连续的路径分隔符或以路径分隔符开头 path:{path}");
                    }

                    RedDotTreeNode child = cur.GetOrAddChild(new RangeString(path, startIndex, endIndex));
                    startIndex = i + 1;
                    cur = child;
                }

                RedDotTreeNode target = cur.GetOrAddChild(new RangeString(path, startIndex, length - 1));
                allTreeNodes[path] = target;
                return target;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// 创建指定路径节点
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <returns>目标节点</returns>
        public RedDotTreeNode AddTreeNode(string path)
        {
            CheckPathIsValid(path);
            return GetTreeNode(path);
        }

        /// <summary>
        /// 删除指定节点及其全部子节点
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <returns>是否成功删除</returns>
        public bool RemoveTreeNode(string path)
        {
            CheckPathIsValid(path);
            if (!TryGetExistingNode(path, out RedDotTreeNode node) || node.Parent == null)
            {
                return false;
            }

            return node.Parent.RemoveChild(new RangeString(node.Name, 0, node.Name.Length - 1));
        }

        /// <summary>
        /// 删除全部红点节点
        /// </summary>
        public void RemoveAllTreeNode()
        {
            dirtyNodes.Clear();
            tempDirtyNodes.Clear();
            RootNode.RemoveAllChild();
            allTreeNodes.Clear();
        }

        #endregion

        #region 节点查询和清理

        /// <summary>
        /// 查找已存在节点且不创建路径
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="node">目标节点</param>
        /// <returns>节点是否存在</returns>
        private bool TryGetExistingNode(string path, out RedDotTreeNode node)
        {
            if (allTreeNodes.TryGetValue(path, out node))
            {
                return true;
            }

            RedDotTreeNode currentNode = RootNode;
            int startIndex = 0;
            for (int i = 0; i <= path.Length; i++)
            {
                if (i < path.Length && path[i] != SplitChar)
                {
                    continue;
                }

                currentNode = currentNode.GetChild(new RangeString(path, startIndex, i - 1));
                if (currentNode == null)
                {
                    node = null;
                    return false;
                }

                startIndex = i + 1;
            }

            node = currentNode;
            return true;
        }

        /// <summary>
        /// 清理已移除子树的缓存和更新状态
        /// </summary>
        /// <param name="removedRoot">被移除的子树根节点</param>
        internal void RemoveSubtreeState(RedDotTreeNode removedRoot)
        {
            if (removedRoot == null)
            {
                return;
            }

            string removedPath = removedRoot.FullPath;
            string childPathPrefix = removedPath + SplitChar;
            tempRemovedPaths.Clear();
            foreach (KeyValuePair<string, RedDotTreeNode> pair in allTreeNodes)
            {
                if (pair.Key == removedPath || pair.Key.StartsWith(childPathPrefix, StringComparison.Ordinal))
                {
                    tempRemovedPaths.Add(pair.Key);
                }
            }

            for (int i = 0; i < tempRemovedPaths.Count; i++)
            {
                allTreeNodes.Remove(tempRemovedPaths[i]);
            }

            tempRemovedPaths.Clear();
            dirtyNodes.RemoveWhere(node => IsNodeInSubtree(node, removedRoot));
        }

        /// <summary>
        /// 判断节点是否属于指定子树
        /// </summary>
        /// <param name="node">待判断节点</param>
        /// <param name="subtreeRoot">子树根节点</param>
        /// <returns>是否属于子树</returns>
        private bool IsNodeInSubtree(RedDotTreeNode node, RedDotTreeNode subtreeRoot)
        {
            RedDotTreeNode currentNode = node;
            while (currentNode != null)
            {
                if (currentNode == subtreeRoot)
                {
                    return true;
                }

                currentNode = currentNode.Parent;
            }

            return false;
        }

        /// <summary>
        /// 判断节点是否仍然连接到当前根节点
        /// </summary>
        /// <param name="node">待判断节点</param>
        /// <returns>节点是否仍在树中</returns>
        private bool IsNodeAttachedToRoot(RedDotTreeNode node)
        {
            if (node == null)
            {
                return false;
            }

            RedDotTreeNode currentNode = node;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
            }

            return currentNode == RootNode;
        }

        #endregion

        #region 事件通知

        /// <summary>
        /// 安全通知节点数量变化
        /// </summary>
        internal void NotifyNodeCountChanged()
        {
            InvokeSafely(NodeNumChangeCallback);
        }

        /// <summary>
        /// 安全通知监听数量变化
        /// </summary>
        internal void NotifyCallbackCountChanged()
        {
            InvokeSafely(CallBackNumberChange);
        }

        /// <summary>
        /// 安全通知节点值变化
        /// </summary>
        /// <param name="node">变化节点</param>
        /// <param name="value">节点值</param>
        internal void NotifyNodeValueChanged(RedDotTreeNode node, int value)
        {
            Action<RedDotTreeNode, int> callbacks = NodeValueChangeCallback;
            if (callbacks == null)
            {
                return;
            }

            foreach (Action<RedDotTreeNode, int> callback in callbacks.GetInvocationList())
            {
                try
                {
                    callback(node, value);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        /// <summary>
        /// 安全调用无参数回调
        /// </summary>
        /// <param name="callbacks">回调集合</param>
        private void InvokeSafely(Action callbacks)
        {
            if (callbacks == null)
            {
                return;
            }

            foreach (Action callback in callbacks.GetInvocationList())
            {
                try
                {
                    callback();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        #endregion
    }
}
