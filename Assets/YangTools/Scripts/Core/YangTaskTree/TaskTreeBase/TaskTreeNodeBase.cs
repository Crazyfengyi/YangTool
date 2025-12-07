using System;
using UnityEngine;

/// <summary>
/// 任务树基类
/// </summary>
public abstract class TaskTreeNodeBase
{
    //当前状态
    public TaskTreeNodeState CurrentState { get; protected set; }
    //父节点
    public TaskTreeNodeBase ParentNode { get; set; }
    /// <summary>
    /// 添加字节点
    /// </summary>
    public abstract void AddChild(TaskTreeNodeBase childNode);

    public abstract void RemoveChild(TaskTreeNodeBase childNode);

    public abstract void RemoveAllChildren();

    public abstract void ForeachChild(Action<TaskTreeNodeBase> action);

    public void StartRun()
    {
        if (CurrentState == TaskTreeNodeState.Run)
        {
            Debug.LogError($"当前节点已经运行CurrentStatus:{CurrentState}");
            return;
        }
        Debug.Log($"开始运行节点:{this}");
        CurrentState = TaskTreeNodeState.Run;
        OnStart();
    }
    public void StartCancel()
    {
        if (CurrentState != TaskTreeNodeState.Run)
        {
            Debug.LogError($"当前节点不是已经运行 CurrentStatus:{CurrentState}");
            return;
        }

        Debug.Log($"取消运行节点:{this}");
        CurrentState = TaskTreeNodeState.None;
        OnCancel();
    }
    public void StartComplete()
    {
        OnComplete();
    }
    /// <summary>
    /// 设置完成
    /// </summary>
    /// <param name="success">是否成功</param>
    protected virtual void SetFinish(bool success)
    {
        Debug.Log($"完成运行节点:{this} success={success}");
        CurrentState = success ? TaskTreeNodeState.Success : TaskTreeNodeState.Failed;
        ParentNode?.OnChildFinish(this, success);
        StartComplete();
    }
    /// <summary>
    /// 开始时
    /// </summary>
    protected abstract void OnStart();
    /// <summary>
    /// 取消时
    /// </summary>
    protected abstract void OnCancel();
    /// <summary>
    /// 完成时
    /// </summary>
    protected abstract void OnComplete();
    protected abstract void OnChildFinish(TaskTreeNodeBase childNode, bool success);

    public override string ToString() => $"{GetType().Name}";
}

/// <summary>
/// 节点状态
/// </summary>
public enum TaskTreeNodeState
{ 
    None,
    Run,
    Success,
    Failed
}