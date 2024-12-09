using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameMain.RedDot
{
    public class RedDotMgr
    {
        private static RedDotMgr _instance;

        public static RedDotMgr Instance => _instance ??= new RedDotMgr();

        private readonly Dictionary<string, RedDotTreeNode> _allTreeNodes;

        public char SplitChar { get; private set; }

        public StringBuilder CachedSb { get; private set; }
        
        public RedDotTreeNode Root { get; private set; }

        private readonly HashSet<RedDotTreeNode> _dirtyNodes;

        private readonly List<RedDotTreeNode> _tempDirtyNodes;

        public Action NodeNumChangeCallback { get; set; }

        public Action<RedDotTreeNode, int> NodeValueChangeCallback { get; set; }

        public Action CallBackNumberChange { get; set; }

        private RedDotMgr()
        {
            SplitChar = '/';

            _allTreeNodes = new Dictionary<string, RedDotTreeNode>();

            Root = new RedDotTreeNode("Root");

            CachedSb = new StringBuilder();

            _dirtyNodes = new HashSet<RedDotTreeNode>();

            _tempDirtyNodes = new List<RedDotTreeNode>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_dirtyNodes.Count == 0)
            {
                return;
            }
            
            _tempDirtyNodes.Clear();

            foreach (var node in _dirtyNodes)
            {
                _tempDirtyNodes.Add(node);
            }
            
            _dirtyNodes.Clear();

            for (int i = 0; i < _tempDirtyNodes.Count; i++)
            {
                _tempDirtyNodes[i].ChangeValue();
            }
        }

        public void MarkDirtyNode(RedDotTreeNode node)
        {
            if (node == null || node.Name == Root.Name)
            {
                return;
            }

            _dirtyNodes.Add(node);
        }

        private void CheckPathIsValid(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new Exception("红点树,路径不合法 path=null");
            }

            if (path.StartsWith(SplitChar) || path.EndsWith(SplitChar))
            {
                throw new Exception($"红点树,路径不合法 path:{path} {SplitChar} 开头或结尾");
            }

            if (path.Contains("{0}"))
            {
                throw new Exception($"红点树,路径不合法 path:{path} 包含:{0}");
            }
        }

        public RedDotTreeNode AddListener(string path, Action<RedDotTreeNode> callback)
        {
            try
            {
                CheckPathIsValid(path);

                var node = GetTreeNode(path);

                node.AddListener(callback);

                return node;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());

                return null;
            }
        }

        public void RemoveListener(string path, Action<RedDotTreeNode> callback)
        {
            try
            {
                CheckPathIsValid(path);

                var node = GetTreeNode(path);

                node.RemoveListener(callback);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void RemoveAllListener(string path)
        {
            try
            {
                CheckPathIsValid(path);
            
                var node = GetTreeNode(path);
            
                node.RemoveAllListener();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void ChangeValue(string path, int newValue)
        {
            try
            {
                CheckPathIsValid(path);

                var node = GetTreeNode(path);

                node.ChangeValue(newValue);
            }
            catch (Exception e)
            {
                //  throw;
                Debug.LogError( $"path={path} newValue={newValue} {e}");
            }
        }

        public int GetValue(string path)
        {
            CheckPathIsValid(path);
            
            var node = GetTreeNode(path);

            if (node == null)
            {
                return 0;
            }

            return node.Value;
        }

        public RedDotTreeNode GetTreeNode(string path)
        {
            try
            {
                CheckPathIsValid(path);

                var node = _allTreeNodes.GetValueOrDefault(path);

                if (node != null)
                {
                    return node;
                }

                var cur = Root;
                int length = path.Length;

                int startIndex = 0;

                for (int i = 0; i < length; i++)
                {
                    if (path[i] == SplitChar)
                    {
                        if (i == length - 1)
                        {
                            throw new Exception($"路径不合法, 不能以路径分隔符结尾 path:{path}");
                        }

                        int endIndex = i - 1;

                        if (endIndex < startIndex)
                        {
                            throw new Exception($"路径不合法,不能存在连续的路径分隔符或以路径分隔符开头 path:{path}");
                        }

                        var child = cur.GetOrAddChild(new RangeString(path, startIndex, endIndex));

                        startIndex = i + 1;

                        cur = child;
                    }
                }

                var target = cur.GetOrAddChild(new RangeString(path, startIndex, length - 1));

                _allTreeNodes.Add(path, target);

                return target;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                throw;
            }
        }

        public RedDotTreeNode AddTreeNode(string path)
        {
            CheckPathIsValid(path);
            
            var node = GetTreeNode(path);

            return node;
        }
        
        public bool RemoveTreeNode(string path)
        {
            CheckPathIsValid(path);
            
            if (_allTreeNodes.ContainsKey(path) == false)
            {
                return false;
            }

            var node = GetTreeNode(path);

            _allTreeNodes.Remove(path);

            var flag = node.Parent.RemoveChild(new RangeString(node.Name, 0, node.Name.Length - 1));

            return flag;
        }

        public void RemoveAllTreeNode()
        {
            Root.RemoveAllChild();
            
            _allTreeNodes.Clear();
        }
    }
}
