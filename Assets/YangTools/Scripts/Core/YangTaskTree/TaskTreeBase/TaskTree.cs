using System;
using UnityEngine;

/// <summary>
/// 任务树
/// </summary>
public class TaskTree
{
    public RootNode RootNode { get; private set; }

    public event Action<bool> OnFinishedEvent
    {
        add
        {
            if (RootNode == null)
            {
                Debug.LogError("RootNode = NULL");
                return;
            }
            RootNode.OnFinishCallBack += value;
        }
        remove
        {
            if (RootNode == null)
            {
                Debug.LogError("RootNode = NULL");
                return;
            }
            RootNode.OnFinishCallBack -= value;
        }
    }
    public Action OnCanceled { get; set; }

    public TaskTree(RootNode rootNode)
    {
        RootNode = rootNode;
    }
    
    /// <summary>
    /// 开始运行
    /// </summary>
    public void StartRun()
    {
        if (RootNode == null)
        {
            throw new Exception("行为树运行失败,根节点为空");
        }
        if (RootNode.CurrentState == TaskTreeNodeState.Run)
        {
            Debug.LogError("行为树已经运行");
            return;
        }
        RootNode.StartRun();
    }
    /// <summary>
    /// 取消任务树
    /// </summary>
    public void StartCancel()
    {
        if (RootNode.CurrentState != TaskTreeNodeState.Run)
        {
            Debug.LogError($"行为树未运行 CurrentStatus={RootNode.CurrentState}");
            return;
        }
        RootNode.StartCancel();
        OnCanceled?.Invoke();
    }
    /// <summary>
    /// 重新开始
    /// </summary>
    public void Restart()
    {
        StartCancel();
        RootNode.StartRun();
    }
}