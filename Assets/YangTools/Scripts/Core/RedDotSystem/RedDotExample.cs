using Sirenix.OdinInspector;
using UnityEngine;

namespace YangTools.Scripts.Core.RedDotSystem
{
    public class RedDotExample : MonoBehaviour
    {
        private const string Path1 = "First/Second/Test1";
        private const string Path2 = "First/Second/Test2";
        private const string Path3 = "First/Second/Third/Test3";
        private const string Path4 = "First/Second/Third/Test4";
        private const string ChangeablePath = "First/Second_{0}/Third/x";
        
        private void Start()
        {
            RedDotMgr.Instance.AddListener(Path1, OnTreeNodeValueChange);
            RedDotMgr.Instance.AddListener(Path2, OnTreeNodeValueChange);
            RedDotMgr.Instance.AddListener(Path3, OnTreeNodeValueChange);
            RedDotMgr.Instance.AddListener(Path4, OnTreeNodeValueChange);

            for (int i = 1; i < 10; i++)
            {
                RedDotMgr.Instance.AddListener(string.Format(ChangeablePath, i), OnTreeNodeValueChange);
            }
        }
        
        private void OnTreeNodeValueChange(RedDotTreeNode obj)
        {
            Debug.Log($"节点改变:{obj}");
        }

#if UNITY_EDITOR

        [LabelText("测试改变节点值")]
        public string testChangeValuePath;

        [LabelText("修改值")] 
        public int testChangeValue;
        
        [Button("改变红点树值")]
        private void SetValue1()
        {
            if (string.IsNullOrEmpty(testChangeValuePath))
            {
                return;
            }

            RedDotMgr.Instance.ChangeValue(testChangeValuePath, testChangeValue);
        }

#endif
    }
}
