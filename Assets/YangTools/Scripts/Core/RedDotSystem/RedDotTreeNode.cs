using System;
using System.Collections.Generic;
using UnityEngine;

namespace YangTools.Scripts.Core.RedDotSystem
{
    public class RedDotTreeNode
    {
        private Dictionary<RangeString, RedDotTreeNode> children;
        
        private Action<RedDotTreeNode> changeCallBack;
        private int callBackCount;
        public int CallBackCount => callBackCount;
        
        public string Name { get; private set; }

        public int Value { get; private set; }

        public RedDotTreeNode Parent { get; private set; }

        private string fullPath;
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

        public RedDotTreeNode(string name)
        {
            Name = name;

            Value = 0;

            changeCallBack = null;

            callBackCount = 0;
        }

        public RedDotTreeNode(string name, RedDotTreeNode parent):this(name)
        {
            Parent = parent;
        }

        public void AddListener(Action<RedDotTreeNode> callback)
        {
            if(callback == null) return;
            
            changeCallBack += callback;

            callBackCount++;
            
            RedDotMgr.Instance.CallBackNumberChange?.Invoke();
            
            Debug.Log($"监听个数改变: {this}");
        }

        public void RemoveListener(Action<RedDotTreeNode> callback)
        {
            if(callback == null) return;
            
            changeCallBack -= callback;

            callBackCount--;
            
            RedDotMgr.Instance.CallBackNumberChange?.Invoke();
            Debug.Log($"监听个数改变: {this}");
        }

        public void RemoveAllListener()
        {
            changeCallBack = null;
            
            callBackCount = 0;
            
            RedDotMgr.Instance.CallBackNumberChange?.Invoke();
            
            Debug.Log($"监听个数改变: {this}");
        }

        public void ChangeValue(int newValue)
        {
            if (children != null && children.Count > 0)
            {
                throw new Exception("不允许直接改变非叶子节点的数据");
            }

            InternalChangeValue(newValue);
        }

        public void ChangeValue()
        {
            int sum = 0;

            if (children != null && children.Count > 0)
            {
                foreach (var child in children)
                {
                    sum += child.Value.Value;
                }
            }
            
            InternalChangeValue(sum);
        }

        public RedDotTreeNode GetOrAddChild(RangeString key)
        {
            var child = GetChild(key);

            if (child == null)
            {
                child = AddChild(key);
            }

            return child;
        }

        public RedDotTreeNode GetChild(RangeString key)
        {
            if (children == null)
            {
                return null;
            }

            children.TryGetValue(key, out var child);

            return child;
        }

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

            var child = new RedDotTreeNode(key.ToString(), this);
            children.Add(key, child);
            RedDotMgr.Instance.NodeNumChangeCallback?.Invoke();

            return child;
        }

        public bool RemoveChild(RangeString key)
        {
            if (children == null || children.Count == 0)
            {
                return false;
            }

            var child = GetChild(key);

            if (child != null)
            {
                children.Remove(key);

                RedDotMgr.Instance.MarkDirtyNode(this);

                RedDotMgr.Instance.NodeNumChangeCallback?.Invoke();

                return true;
            }

            return false;
        }

        public void RemoveAllChild()
        {
            if (children == null || children.Count == 0)
            {
                return;
            }
            
            children.Clear();
            
            RedDotMgr.Instance.MarkDirtyNode(this);
            
            RedDotMgr.Instance.NodeNumChangeCallback?.Invoke();
        }

        private void InternalChangeValue(int newValue)
        {
            if (Value == newValue)
            {
                return;
            }

            Value = newValue;
            changeCallBack?.Invoke(this);
            
            RedDotMgr.Instance.NodeValueChangeCallback?.Invoke(this, Value);
            
            RedDotMgr.Instance.MarkDirtyNode(Parent);
        }

        public override string ToString()
        {
            return $"节点名:{Name} 节点值:{Value} 节点路径:{FullPath} 子节点数量:{ChildrenCount} 监听数量:{callBackCount}";
        }
    }
}
