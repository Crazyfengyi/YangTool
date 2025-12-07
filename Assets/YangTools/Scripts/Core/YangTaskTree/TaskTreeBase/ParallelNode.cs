/// <summary>
/// 并行节点
/// </summary>
public class ParallelNode : ComposeNodeBase
{
    public ParallelCondition Condition { get; set; }

    private int successCount;
    private int failedCount;

    protected override void OnStart()
    {
        successCount = 0;
        failedCount = 0;
        foreach (var child in Childrens)
        {
            child.StartRun();
        }
    }

    protected override void OnCancel()
    {
        foreach (var child in Childrens)
        {
            child.StartCancel();
        }
    }

    protected override void OnChildFinish(TaskTreeNodeBase childNode, bool success)
    {
        if (success)
        {
            successCount++;
        }
        else
        {
            failedCount++;
        }

        switch (Condition)
        {
            case ParallelCondition.FirstSuccess:
                if (successCount > 0)
                {
                    StartCancel();
                    SetFinish(true);
                }
                else if (failedCount == Childrens.Count)
                {
                    StartCancel();
                    SetFinish(false);
                }

                break;
            case ParallelCondition.FirstFailed:

                if (failedCount > 0)
                {
                    StartCancel();
                    SetFinish(true);
                }
                else if (successCount == Childrens.Count)
                {
                    StartCancel();
                    SetFinish(false);
                }

                break;
            default:
                break;
        }
    }
}

/// <summary>
/// 并行条件
/// </summary>
public enum ParallelCondition
{
    FirstSuccess,
    FirstFailed,
}