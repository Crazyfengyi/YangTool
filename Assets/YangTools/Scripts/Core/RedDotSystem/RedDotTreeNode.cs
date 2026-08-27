using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Scripts.Core.RedDotSystem
{
    /// <summary>
    /// 表示红点树中的一个节点
    /// </summary>
    public class RedDotTreeNode
    {
        private Dictionary<RangeString, RedDotTreeNode> children; //直接子节点
        private Action<RedDotTreeNode> changeCallBack; //节点值变化回调
        private int callBackCount; //监听数量
        private string fullPath; //完整节点路径

        public int CallBackCount => callBackCount;
        
        public string Name { get; private set; }

        public int Value { get; private set; }

        public RedDotTreeNode Parent { get; private set; }

        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(fullPath))
                {
                    if (Parent == null || Parent == RedDotMgr.Instance.RootNode)
                    {
                        fullPath = Name;
                    }
                    else
                    {
                        fullPath = Parent.FullPath + RedDotMgr.Instance.SplitChar + Name;
                    }
                }

                return fullPath;
            }
        }

        public Dictionary<RangeString, RedDotTreeNode>.ValueCollection Children => children?.Values;

        public int ChildrenCount
        {
            get
            {
                if (children == null)
                {
                    return 0;
                }

                int sum = children.Count;

                foreach (var node in children.Values)
                {
                    sum += node.ChildrenCount;
                }

                return sum;
            }
        }

        /// <summary>
        /// 创建根节点或无父节点的节点
        /// </summary>
        /// <param name="name">节点名称</param>
        public RedDotTreeNode(string name)
        {
            Name = name;

            Value = 0;

            changeCallBack = null;

            callBackCount = 0;
        }

        /// <summary>
        /// 创建指定父节点下的节点
        /// </summary>
        /// <param name="name">节点名称</param>
        /// <param name="parent">父节点</param>
        public RedDotTreeNode(string name, RedDotTreeNode parent) : this(name)
        {
            Parent = parent;
        }

        #region 监听管理

        /// <summary>
        /// 添加节点值变化监听
        /// </summary>
        /// <param name="callback">变化回调</param>
        public void AddListener(Action<RedDotTreeNode> callback)
        {
            if (callback == null || ContainsListener(callback))
            {
                return;
            }

            changeCallBack += callback;
            callBackCount++;
            RedDotMgr.Instance.NotifyCallbackCountChanged();
        }

        /// <summary>
        /// 移除节点值变化监听
        /// </summary>
        /// <param name="callback">变化回调</param>
        public void RemoveListener(Action<RedDotTreeNode> callback)
        {
            if (callback == null || !ContainsListener(callback))
            {
                return;
            }

            changeCallBack -= callback;
            callBackCount--;
            RedDotMgr.Instance.NotifyCallbackCountChanged();
        }

        /// <summary>
        /// 移除节点的全部监听
        /// </summary>
        public void RemoveAllListener()
        {
            if (changeCallBack == null)
            {
                return;
            }

            changeCallBack = null;
            callBackCount = 0;
            RedDotMgr.Instance.NotifyCallbackCountChanged();
        }

        /// <summary>
        /// 判断是否已经添加指定监听
        /// </summary>
        /// <param name="callback">变化回调</param>
        /// <returns>监听是否存在</returns>
        private bool ContainsListener(Action<RedDotTreeNode> callback)
        {
            if (changeCallBack == null)
            {
                return false;
            }

            foreach (Action<RedDotTreeNode> listener in changeCallBack.GetInvocationList())
            {
                if (listener == callback)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 节点值

        /// <summary>
        /// 修改叶子节点值
        /// </summary>
        /// <param name="newValue">新值</param>
        public void ChangeValue(int newValue)
        {
            if (children != null && children.Count > 0)
            {
                throw new Exception("不允许直接改变非叶子节点的数据");
            }

            InternalChangeValue(newValue);
        }

        /// <summary>
        /// 根据直接子节点重新计算当前节点值
        /// </summary>
        public void ChangeValue()
        {
            long sum = 0;

            if (children != null && children.Count > 0)
            {
                foreach (RedDotTreeNode child in children.Values)
                {
                    sum += child.Value;
                }
            }

            int aggregateValue = sum > int.MaxValue ? int.MaxValue : sum < int.MinValue ? int.MinValue : (int)sum;
            InternalChangeValue(aggregateValue);
        }

        /// <summary>
        /// 更新当前节点并通知监听方
        /// </summary>
        /// <param name="newValue">新值</param>
        private void InternalChangeValue(int newValue)
        {
            if (Value == newValue)
            {
                return;
            }

            Value = newValue;
            InvokeChangeCallbacks();
            RedDotMgr.Instance.NotifyNodeValueChanged(this, Value);
            RedDotMgr.Instance.MarkDirtyNode(Parent);
        }

        /// <summary>
        /// 独立调用监听以避免单个异常中断红点传播
        /// </summary>
        private void InvokeChangeCallbacks()
        {
            if (changeCallBack == null)
            {
                return;
            }

            foreach (Action<RedDotTreeNode> callback in changeCallBack.GetInvocationList())
            {
                try
                {
                    callback(this);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        #endregion

        #region 子节点管理

        /// <summary>
        /// 获得或创建直接子节点
        /// </summary>
        /// <param name="key">子节点名称范围</param>
        /// <returns>直接子节点</returns>
        public RedDotTreeNode GetOrAddChild(RangeString key)
        {
            var child = GetChild(key);

            if (child == null)
            {
                child = AddChild(key);
            }

            return child;
        }

        /// <summary>
        /// 获得直接子节点
        /// </summary>
        /// <param name="key">子节点名称范围</param>
        /// <returns>不存在时返回空</returns>
        public RedDotTreeNode GetChild(RangeString key)
        {
            if (children == null)
            {
                return null;
            }

            children.TryGetValue(key, out RedDotTreeNode child);

            return child;
        }

        /// <summary>
        /// 创建直接子节点
        /// </summary>
        /// <param name="key">子节点名称范围</param>
        /// <returns>新节点</returns>
        private RedDotTreeNode AddChild(RangeString key)
        {
            if (children == null)
            {
                children = new Dictionary<RangeString, RedDotTreeNode>();
            }
            else if (children.ContainsKey(key))
            {
                throw new Exception($"子节点添加失败,不允许重复添加 key={key}");
            }

            RedDotTreeNode child = new RedDotTreeNode(key.ToString(), this);
            children.Add(key, child);
            RedDotMgr.Instance.NotifyNodeCountChanged();

            return child;
        }

        /// <summary>
        /// 删除直接子节点及其子树
        /// </summary>
        /// <param name="key">子节点名称范围</param>
        /// <returns>是否成功删除</returns>
        public bool RemoveChild(RangeString key)
        {
            if (children == null || children.Count == 0)
            {
                return false;
            }

            RedDotTreeNode child = GetChild(key);

            if (child != null)
            {
                RedDotMgr.Instance.RemoveSubtreeState(child);
                children.Remove(key);
                child.Parent = null;
                RedDotMgr.Instance.MarkDirtyNode(this);
                RedDotMgr.Instance.NotifyNodeCountChanged();

                return true;
            }

            return false;
        }

        /// <summary>
        /// 删除全部直接子节点及其子树
        /// </summary>
        public void RemoveAllChild()
        {
            if (children == null || children.Count == 0)
            {
                return;
            }
            
            foreach (RedDotTreeNode child in children.Values)
            {
                RedDotMgr.Instance.RemoveSubtreeState(child);
                child.Parent = null;
            }

            children.Clear();
            RedDotMgr.Instance.MarkDirtyNode(this);
            RedDotMgr.Instance.NotifyNodeCountChanged();
        }

        #endregion

        /// <summary>
        /// 获得节点调试信息
        /// </summary>
        /// <returns>节点信息</returns>
        public override string ToString()
        {
            return $"节点名:{Name} 节点值:{Value} 节点路径:{FullPath} 子节点数量:{ChildrenCount} 监听数量:{callBackCount}";
        }
    }
}
