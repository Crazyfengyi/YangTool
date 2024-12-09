using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain.RedDot
{
    public class RedDotTreeNode
    {
        private Dictionary<RangeString, RedDotTreeNode> _children;
        
        private Action<RedDotTreeNode> _changeCallBack;
        private int _callBackCount;
        public int CallBackCount => _callBackCount;
        
        public string Name { get; private set; }

        public int Value { get; private set; }

        public RedDotTreeNode Parent { get; private set; }

        private string _fullPath;
        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(_fullPath))
                {
                    if (Parent == null || Parent == RedDotMgr.Instance.Root)
                    {
                        _fullPath = Name;
                    }
                    else
                    {
                        _fullPath = Parent.FullPath + RedDotMgr.Instance.SplitChar + Name;
                    }
                }

                return _fullPath;
            }
        }

        public Dictionary<RangeString, RedDotTreeNode>.ValueCollection Children => _children?.Values;

        public int ChildrenCount
        {
            get
            {
                if (_children == null)
                {
                    return 0;
                }

                int sum = _children.Count;

                foreach (var node in _children.Values)
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

            _changeCallBack = null;

            _callBackCount = 0;
        }

        public RedDotTreeNode(string name, RedDotTreeNode parent):this(name)
        {
            Parent = parent;
        }

        public void AddListener(Action<RedDotTreeNode> callback)
        {
            if(callback == null) return;
            
            _changeCallBack += callback;

            _callBackCount++;
            
            RedDotMgr.Instance.CallBackNumberChange?.Invoke();
            
            Debug.Log($"监听个数改变: {this}");
        }

        public void RemoveListener(Action<RedDotTreeNode> callback)
        {
            if(callback == null) return;
            
            _changeCallBack -= callback;

            _callBackCount--;
            
            RedDotMgr.Instance.CallBackNumberChange?.Invoke();
            Debug.Log($"监听个数改变: {this}");
        }

        public void RemoveAllListener()
        {
            _changeCallBack = null;
            
            _callBackCount = 0;
            
            RedDotMgr.Instance.CallBackNumberChange?.Invoke();
            
            Debug.Log($"监听个数改变: {this}");
        }

        public void ChangeValue(int newValue)
        {
            if (_children != null && _children.Count > 0)
            {
                throw new Exception("不允许直接改变非叶子节点的数据");
            }

            InternalChangeValue(newValue);
        }

        public void ChangeValue()
        {
            int sum = 0;

            if (_children != null && _children.Count > 0)
            {
                foreach (var child in _children)
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
            if (_children == null)
            {
                return null;
            }

            _children.TryGetValue(key, out var child);

            return child;
        }

        private RedDotTreeNode AddChild(RangeString key)
        {
            if (_children == null)
            {
                _children = new Dictionary<RangeString, RedDotTreeNode>();
            }
            else if (_children.ContainsKey(key))
            {
                throw new Exception($"子节点添加失败,不允许重复添加 key={key}");
            }

            var child = new RedDotTreeNode(key.ToString(), this);
            _children.Add(key, child);
            RedDotMgr.Instance.NodeNumChangeCallback?.Invoke();

            return child;
        }

        public bool RemoveChild(RangeString key)
        {
            if (_children == null || _children.Count == 0)
            {
                return false;
            }

            var child = GetChild(key);

            if (child != null)
            {
                _children.Remove(key);

                RedDotMgr.Instance.MarkDirtyNode(this);

                RedDotMgr.Instance.NodeNumChangeCallback?.Invoke();

                return true;
            }

            return false;
        }

        public void RemoveAllChild()
        {
            if (_children == null || _children.Count == 0)
            {
                return;
            }
            
            _children.Clear();
            
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
            _changeCallBack?.Invoke(this);
            
            RedDotMgr.Instance.NodeValueChangeCallback?.Invoke(this, Value);
            
            RedDotMgr.Instance.MarkDirtyNode(Parent);
        }

        public override string ToString()
        {
            return $"节点名:{Name} 节点值:{Value} 节点路径:{FullPath} 子节点数量:{ChildrenCount} 监听数量:{_callBackCount}";
        }
    }
}
