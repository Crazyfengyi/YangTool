//using UnityEngine;
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;

//[TaskIcon(BehaviorSetting.path + "allOK.png")]
//[TaskCategory("RoleAI")]
//[TaskDescription("是否全部为true")]
///// <summary>
///// 是否全部为true
///// </summary>
//public class IsAllReady : Decorator
//{
//    // 当前子节点下标
//    private int currentChildIndex = 0;
//    // 当前运行的子节点状态
//    private TaskStatus executionStatus = TaskStatus.Inactive;
//    public override int MaxChildren()
//    {
//        return 100;
//    }
//    public override int CurrentChildIndex()
//    {
//        return currentChildIndex;
//    }
//    //检测是否可以继续运行
//    public override bool CanExecute()
//    {
//        return currentChildIndex < children.Count && executionStatus != TaskStatus.Failure;
//    }
//    /// <summary>
//    /// 在子节点start后--单个子节点
//    /// </summary>
//    public override void OnChildStarted(int childIndex)
//    {

//    }
//    /// <summary>
//    /// 在子节点执行完毕后--单个子节点
//    /// </summary>
//    public override void OnChildExecuted(TaskStatus childStatus)
//    {
//        currentChildIndex++;
//        executionStatus = childStatus;
//    }
//    //在条件终止时
//    public override void OnConditionalAbort(int childIndex)
//    {
//        currentChildIndex = childIndex;
//        executionStatus = TaskStatus.Inactive;
//    }
//    /// <summary>
//    /// 装饰
//    /// </summary>
//    public override TaskStatus Decorate(TaskStatus status)
//    {
//        if (status == TaskStatus.Failure)
//        {
//            return TaskStatus.Failure;
//        }

//        //最后一个节点
//        if (currentChildIndex == children.Count)
//        {
//            return status;
//        }

//        return TaskStatus.Running;
//    }
//    public override void OnEnd()
//    {
//        executionStatus = TaskStatus.Inactive;
//    }
//}