/// <summary>
/// 选择器节点
/// </summary>
public class SelectorNode : ComposeNodeBase
{
    private int currentChildIndex = -1;

    protected override void OnStart()
    {
        currentChildIndex = -1;
    }

    private void StartNextChild()
    {
        currentChildIndex++;
        if (currentChildIndex >= Childrens.Count)
        {
            SetFinish(false);
            return;
        }
        var currentChild = Childrens[currentChildIndex];
        currentChild.StartRun();
    }

    protected override void OnCancel()
    {
        var currentChild = Childrens[currentChildIndex];
        currentChild?.StartCancel();
    }

    protected override void OnChildFinish(TaskTreeNodeBase childNode, bool success)
    {
        if (success)
        {
            SetFinish(true);
        }
        else
        { 
            StartNextChild();
        }
    }
}