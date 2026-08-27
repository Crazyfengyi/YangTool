using Sirenix.OdinInspector;
using UnityEngine;

namespace YangTools.Scripts.Core.RedDotSystem
{
    /// <summary>
    /// 红点系统使用示例
    /// </summary>
    public class RedDotExample : MonoBehaviour
    {
        private const string Path1 = "First/Second/Test1"; //测试路径一
        private const string Path2 = "First/Second/Test2"; //测试路径二
        private const string Path3 = "First/Second/Third/Test3"; //测试路径三
        private const string Path4 = "First/Second/Third/Test4"; //测试路径四
        private const string ChangeablePath = "First/Second_{0}/Third/x"; //动态测试路径

        /// <summary>
        /// 组件启用时注册监听
        /// </summary>
        private void OnEnable()
        {
            SetListeners(true);
        }

        /// <summary>
        /// 组件禁用时取消监听
        /// </summary>
        private void OnDisable()
        {
            SetListeners(false);
        }

        /// <summary>
        /// 批量设置示例节点监听
        /// </summary>
        /// <param name="shouldAdd">是否添加监听</param>
        private void SetListeners(bool shouldAdd)
        {
            SetListener(Path1, shouldAdd);
            SetListener(Path2, shouldAdd);
            SetListener(Path3, shouldAdd);
            SetListener(Path4, shouldAdd);

            for (int i = 1; i < 10; i++)
            {
                SetListener(string.Format(ChangeablePath, i), shouldAdd);
            }
        }

        /// <summary>
        /// 设置单个节点监听
        /// </summary>
        /// <param name="path">节点路径</param>
        /// <param name="shouldAdd">是否添加监听</param>
        private void SetListener(string path, bool shouldAdd)
        {
            if (shouldAdd)
            {
                RedDotMgr.Instance.AddListener(path, OnTreeNodeValueChange);
                return;
            }

            RedDotMgr.Instance.RemoveListener(path, OnTreeNodeValueChange);
        }

        /// <summary>
        /// 输出变化节点信息
        /// </summary>
        /// <param name="node">变化节点</param>
        private void OnTreeNodeValueChange(RedDotTreeNode node)
        {
            Debug.Log($"节点改变:{node}");
        }

#if UNITY_EDITOR

        [LabelText("测试改变节点值")]
        public string testChangeValuePath; //测试修改路径

        [LabelText("修改值")] 
        public int testChangeValue; //测试修改值

        /// <summary>
        /// 修改测试节点值
        /// </summary>
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
