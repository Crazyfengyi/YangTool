using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 组合节点
/// </summary>
public class ComposeNodeBase : TaskTreeNodeBase
{
    public List<TaskTreeNodeBase> Childrens = new();

    public override void AddChild(TaskTreeNodeBase childNode)
    {
        if (childNode == null)
        {
            Debug.LogError("AddChild 参数:childNode=null");
            return;
        }

        if (Childrens.Contains(childNode))
        {
            Debug.LogError($"{childNode}已经加入集合");
            return;
        }

        if(childNode.ParentNode != null)
        {
            childNode.ParentNode.RemoveChild(childNode);
        }

        childNode.ParentNode = this;
        Childrens.Add(childNode);
    }

    public override void RemoveChild(TaskTreeNodeBase childNode)
    {
        if (childNode == null)
        {
            Debug.LogError("AddChild 参数:childNode=null");
            return;
        }

        childNode.ParentNode = null;
        Childrens.Remove(childNode);
    }

    public override void RemoveAllChildren()
    {
        for (int i = Childrens.Count - 1; i >= 0; i--)
        {
            RemoveChild(Childrens[i]);
        }
    }

    public override void ForeachChild(Action<TaskTreeNodeBase> action)
    {
        foreach (var child in Childrens)
        {
            action?.Invoke(child);
        }
    }

    protected override void OnStart()
    {
    }

    protected override void OnCancel()
    {
    }

    protected override void OnComplete()
    {
    }

    protected override void OnChildFinish(TaskTreeNodeBase childNode, bool success)
    {
    }
}