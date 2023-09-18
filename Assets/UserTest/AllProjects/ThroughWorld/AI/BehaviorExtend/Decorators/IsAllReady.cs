//using UnityEngine;
//using BehaviorDesigner.Runtime;
//using BehaviorDesigner.Runtime.Tasks;

//[TaskIcon(BehaviorSetting.path + "allOK.png")]
//[TaskCategory("RoleAI")]
//[TaskDescription("�Ƿ�ȫ��Ϊtrue")]
///// <summary>
///// �Ƿ�ȫ��Ϊtrue
///// </summary>
//public class IsAllReady : Decorator
//{
//    // ��ǰ�ӽڵ��±�
//    private int currentChildIndex = 0;
//    // ��ǰ���е��ӽڵ�״̬
//    private TaskStatus executionStatus = TaskStatus.Inactive;
//    public override int MaxChildren()
//    {
//        return 100;
//    }
//    public override int CurrentChildIndex()
//    {
//        return currentChildIndex;
//    }
//    //����Ƿ���Լ�������
//    public override bool CanExecute()
//    {
//        return currentChildIndex < children.Count && executionStatus != TaskStatus.Failure;
//    }
//    /// <summary>
//    /// ���ӽڵ�start��--�����ӽڵ�
//    /// </summary>
//    public override void OnChildStarted(int childIndex)
//    {

//    }
//    /// <summary>
//    /// ���ӽڵ�ִ����Ϻ�--�����ӽڵ�
//    /// </summary>
//    public override void OnChildExecuted(TaskStatus childStatus)
//    {
//        currentChildIndex++;
//        executionStatus = childStatus;
//    }
//    //��������ֹʱ
//    public override void OnConditionalAbort(int childIndex)
//    {
//        currentChildIndex = childIndex;
//        executionStatus = TaskStatus.Inactive;
//    }
//    /// <summary>
//    /// װ��
//    /// </summary>
//    public override TaskStatus Decorate(TaskStatus status)
//    {
//        if (status == TaskStatus.Failure)
//        {
//            return TaskStatus.Failure;
//        }

//        //���һ���ڵ�
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