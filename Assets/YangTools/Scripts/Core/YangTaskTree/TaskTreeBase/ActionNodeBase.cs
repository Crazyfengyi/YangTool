  
using System;

/// <summary>
/// 行为节点父类
/// </summary>
public class ActionNodeBase : TaskTreeNodeBase
{
    public override void AddChild(TaskTreeNodeBase childNode)
    {
    }

    public override void RemoveChild(TaskTreeNodeBase childNode)
    {
    }

    public override void RemoveAllChildren()
    {
    }

    public override void ForeachChild(Action<TaskTreeNodeBase> action)
    {
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