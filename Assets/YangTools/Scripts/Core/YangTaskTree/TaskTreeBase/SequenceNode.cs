/// <summary>
/// 队列节点
/// </summary>
public class SequenceNode : ComposeNodeBase
{
    private int currentRunChildIndex = -1;

    private void StartNextChild()
    {
        currentRunChildIndex++;
        if (currentRunChildIndex >= Childrens.Count)
        {
            SetFinish(true);
            return;
        }

        var currentNode = Childrens[currentRunChildIndex];
        currentNode.StartRun();
    }

    protected override void OnStart()
    {
        currentRunChildIndex = -1;
        StartNextChild();
    }

    protected override void OnCancel()
    {
        var currentChild = Childrens[currentRunChildIndex];
        currentChild.StartCancel();
    }

    protected override void OnChildFinish(TaskTreeNodeBase childNode, bool success)
    {
        if (success)
        {
            StartNextChild();
        }
        else
        {
            SetFinish(false);
        }
    }
}