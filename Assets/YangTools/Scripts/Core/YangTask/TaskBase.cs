/**
 *Copyright(C) 2020 by DefaultCompany
 *All rights reserved.
 *Author:       DESKTOP-AJS8G4U
 *UnityVersion：2021.2.1f1c1
 *创建时间:         2022-02-18
*/

namespace YangTools.TaskExtend
{
    /// <summary>
    /// 任务基类。
    /// </summary>
    public abstract class TaskBase
    {
        /// <summary>
        /// 任务默认优先级。
        /// </summary>
        public const int DefaultPriority = 0;

        private int m_SerialId;//序列编号
        private int m_Priority;//优先级
        private TaskStatus m_Status;//任务状态
        private object m_UserData;//自定义数据

        /// <summary>
        /// 初始化任务基类的新实例。
        /// </summary>
        public TaskBase()
        {
            m_SerialId = 0;
            m_Priority = DefaultPriority;
            m_Status = TaskStatus.None;
            m_UserData = null;
        }

        /// <summary>
        /// 获取任务的序列编号。
        /// </summary>
        public int SerialId
        {
            get
            {
                return m_SerialId;
            }
        }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority
        {
            get
            {
                return m_Priority;
            }
        }

        /// <summary>
        /// 获取任务的状态。
        /// </summary>
        public TaskStatus Status
        {
            get
            {
                return m_Status;
            }
        }

        /// <summary>
        /// 获取或设置用户自定义数据。
        /// </summary>
        public object UserData
        {
            get
            {
                return m_UserData;
            }
            set
            {
                m_UserData = value;
            }
        }

        /// <summary>
        /// 初始化任务基类。
        /// </summary>
        /// <param name="serialId">任务的序列编号。</param>
        /// <param name="priority">任务的优先级。</param>
        internal void Initialize(int serialId, int priority)
        {
            m_SerialId = serialId;
            m_Priority = priority;
        }

        /// <summary>
        /// 清理任务基类
        /// </summary>
        public virtual void Empty()
        {
            m_SerialId = 0;
            m_Priority = DefaultPriority;
            m_Status = TaskStatus.None;
            m_UserData = null;
        }

        /// <summary>
        /// 任务生成时调用
        /// </summary>
        protected internal virtual void OnCreate()
        {
            m_Status = TaskStatus.Waiting;
        }

        /// <summary>
        /// 任务开始时调用
        /// </summary>
        protected internal virtual void OnStart()
        {
            m_Status = TaskStatus.Running;
        }

        /// <summary>
        /// 任务轮询时调用
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
        }

        /// <summary>
        /// 任务完成时调用。
        /// </summary>
        /// <param name="reason">任务完成的原因。</param>
        protected internal virtual void OnComplete(string reason)
        {
            m_Status = TaskStatus.Completed;
        }

        /// <summary>
        /// 任务失败时调用。
        /// </summary>
        /// <param name="reason">任务失败的原因。</param>
        protected internal virtual void OnFailure(string reason)
        {
            m_Status = TaskStatus.Failed;
        }

        /// <summary>
        /// 任务取消时调用。
        /// </summary>
        /// <param name="reason">任务取消的原因。</param>
        protected internal virtual void OnCancel(string reason)
        {
            m_Status = TaskStatus.Canceled;
        }
    }

    /// <summary>
    /// 任务状态。
    /// </summary>
    public enum TaskStatus : byte
    {
        /// <summary>
        /// 未初始化
        /// </summary>
        None = 0,

        /// <summary>
        /// 等待中。
        /// </summary>
        Waiting,

        /// <summary>
        /// 运行中。
        /// </summary>
        Running,

        /// <summary>
        /// 已完成。
        /// </summary>
        Completed,

        /// <summary>
        /// 已失败。
        /// </summary>
        Failed,

        /// <summary>
        /// 已取消。
        /// </summary>
        Canceled
    }
}