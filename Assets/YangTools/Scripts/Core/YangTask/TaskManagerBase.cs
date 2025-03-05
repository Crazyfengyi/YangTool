/*
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-18
*/

namespace YangTools.TaskExtend
{
    /// <summary>
    /// 任务管理器
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// 获取任务数量。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 生成任务。
        /// </summary>
        /// <typeparam name="T">任务的类型</typeparam>
        /// <returns>生成的指定类型的任务</returns>
        T CreateTask<T>() where T : TaskBase, new();

        /// <summary>
        /// 生成任务。
        /// </summary>
        /// <typeparam name="T">任务的类型</typeparam>
        /// <param name="priority">任务的优先级</param>
        /// <returns>生成的指定类型的任务</returns>
        T CreateTask<T>(int priority) where T : TaskBase, new();

        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="taskBase">要取消的任务</param>
        /// <returns>是否取消任务成功</returns>
        bool CancelTask(TaskBase taskBase);

        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="taskBase">要取消的任务</param>
        /// <param name="reason">任务取消的原因</param>
        /// <returns>是否取消任务成功</returns>
        bool CancelTask(TaskBase taskBase, string reason);

        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号</param>
        /// <returns>是否取消任务成功</returns>
        bool CancelTask(int serialId);

        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号</param>
        /// <param name="reason">任务取消的原因</param>
        /// <returns>是否取消任务成功</returns>
        bool CancelTask(int serialId, string reason);

        /// <summary>
        /// 取消所有任务。
        /// </summary>
        /// <param name="reason">任务取消的原因</param>
        void CancelAllTasks(string reason);
    }
    
    /// <summary>
    /// 任务基类
    /// </summary>
    public abstract class TaskBase
    {
        /// <summary>
        /// 任务默认优先级
        /// </summary>
        public const int DefaultPriority = 0;

        private int serialId; //序列编号
        private int priority; //优先级
        private TaskStatus status; //任务状态
        private object userData; //自定义数据

        /// <summary>
        /// 初始化任务基类的新实例
        /// </summary>
        public TaskBase()
        {
            serialId = 0;
            priority = DefaultPriority;
            status = TaskStatus.None;
            userData = null;
        }

        /// <summary>
        /// 获取任务的序列编号
        /// </summary>
        public int SerialId => serialId;

        /// <summary>
        /// 获取任务的优先级
        /// </summary>
        public int Priority => priority;

        /// <summary>
        /// 获取任务的状态
        /// </summary>
        public TaskStatus Status => status;

        /// <summary>
        /// 获取或设置用户自定义数据
        /// </summary>
        public object UserData
        {
            get => userData;
            set => userData = value;
        }

        /// <summary>
        /// 初始化任务基类。
        /// </summary>
        /// <param name="argSerialId">任务的序列编号</param>
        /// <param name="argPriority">任务的优先级</param>
        internal void Initialize(int argSerialId, int argPriority)
        {
            this.serialId = argSerialId;
            this.priority = argPriority;
        }

        /// <summary>
        /// 清理任务基类
        /// </summary>
        public virtual void Empty()
        {
            serialId = 0;
            priority = DefaultPriority;
            status = TaskStatus.None;
            userData = null;
        }

        /// <summary>
        /// 任务生成时调用
        /// </summary>
        protected internal virtual void OnCreate()
        {
            status = TaskStatus.Waiting;
        }

        /// <summary>
        /// 任务开始时调用
        /// </summary>
        protected internal virtual void OnStart()
        {
            status = TaskStatus.Running;
        }

        /// <summary>
        /// 任务轮询时调用
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        protected internal virtual void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
        }

        /// <summary>
        /// 任务完成时调用。
        /// </summary>
        /// <param name="reason">任务完成的原因</param>
        protected internal virtual void OnComplete(string reason)
        {
            status = TaskStatus.Completed;
        }

        /// <summary>
        /// 任务失败时调用
        /// </summary>
        /// <param name="reason">任务失败的原因</param>
        protected internal virtual void OnFailure(string reason)
        {
            status = TaskStatus.Failed;
        }

        /// <summary>
        /// 任务取消时调用
        /// </summary>
        /// <param name="reason">任务取消的原因</param>
        protected internal virtual void OnCancel(string reason)
        {
            status = TaskStatus.Canceled;
        }
    }

    /// <summary>
    /// 任务状态
    /// </summary>
    public enum TaskStatus : byte
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        None = 0,

        /// <summary>
        /// 等待中
        /// </summary>
        Waiting,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed,

        /// <summary>
        /// 已失败
        /// </summary>
        Failed,

        /// <summary>
        /// 已取消
        /// </summary>
        Canceled
    }
}