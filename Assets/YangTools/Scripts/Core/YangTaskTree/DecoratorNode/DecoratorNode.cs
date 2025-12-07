using System;
using UnityEngine;

/// <summary>
/// 装饰节点
/// </summary>
public class DecoratorNode : TaskTreeNodeBase
{
    public TaskTreeNodeBase childNode;
    
    public override void AddChild(TaskTreeNodeBase _childNode)
    {
        if (_childNode == null)
        {
            Debug.LogError("AddChild childNode=null");
            return;
        }

        if (_childNode.ParentNode != null)
        {
            _childNode.ParentNode.RemoveChild(childNode);
        }

        _childNode.ParentNode = this;
        childNode = _childNode;
    }

    public override void RemoveChild(TaskTreeNodeBase _childNode)
    {
        if (childNode == null)
        {
            Debug.LogError("RemoveChild childNode=null");
            return;
        }
        _childNode.ParentNode = null;
        childNode = null;
    }

    public override void RemoveAllChildren()
    {
        RemoveChild(childNode);
    }

    public override void ForeachChild(Action<TaskTreeNodeBase> action)
    {
        action?.Invoke(childNode);
    }

    protected override void OnStart()
    {
        childNode.StartRun();
    }

    protected override void OnCancel()
    {
        childNode.StartCancel();
    }

    protected override void OnComplete()
    {
        childNode.StartComplete();
    }

    protected override void OnChildFinish(TaskTreeNodeBase childNode, bool success)
    {
        SetFinish(success);
    }
}