using System;

/// <summary>
/// 起始节点
/// </summary>
public class RootNode : DecoratorNode
{
    public Action<bool> OnFinishCallBack { get; set; }
    protected override void OnChildFinish(TaskTreeNodeBase childNode, bool success)
    {
        SetFinish(success);
        OnFinishCallBack?.Invoke(success);
    }
}