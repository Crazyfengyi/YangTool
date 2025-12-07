using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain.BTree
{
    public class TestBT : MonoBehaviour
    {
        private TaskTree tree;

        void Start()
        {
            var rootNode = new RootNode();
            tree = new TaskTree(rootNode);

            var sequence = new SequenceNode();
            rootNode.AddChild(sequence);

            var delayNode = new DelayNode(1);
            sequence.AddChild(delayNode);

            var logNode = new LogNode
            {
                logLevel = LogLevel.Error,
                logString = "测试打印Info"
            };
            sequence.AddChild(logNode);

            delayNode = new DelayNode(2);
            sequence.AddChild(delayNode);

            logNode = new LogNode
            {
                logLevel = LogLevel.Error,
                logString = "测试打印Warning"
            };
            sequence.AddChild(logNode);

            delayNode = new DelayNode(3);
            sequence.AddChild(delayNode);

            logNode = new LogNode
            {
                logLevel = LogLevel.Error,
                logString = "测试打印Error"
            };
            sequence.AddChild(logNode);

            tree.OnFinishedEvent += OnTreeFinish;
            tree.StartRun();
        }

        [Button("重新开始行为树")]
        public void RestartTree()
        {
            tree.Restart();
        }

        private void OnDestroy()
        {
            tree.OnFinishedEvent -= OnTreeFinish;
        }

        private void OnTreeFinish(bool result)
        {
            Debug.Log($"行为树完成 result:{result}");
        }
    }
}