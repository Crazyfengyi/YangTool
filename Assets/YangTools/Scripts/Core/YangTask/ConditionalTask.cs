/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-18 
*/
using System;

namespace YangTools.TaskExtend
{
    /// <summary>
    /// 条件任务。
    /// </summary>
    public sealed class ConditionalTask : TaskBase
    {
        private Predicate<ConditionalTask> m_Condition;//条件-返回bool
        private Action<ConditionalTask, string> m_CompleteAction;//完成回调
        private Action<ConditionalTask, string> m_FailureAction;//失败回调
        private Action<ConditionalTask, string> m_CancelAction;//关闭回调
        /// <summary>
        /// 初始化条件任务的新实例。
        /// </summary>
        public ConditionalTask()
        {
            m_Condition = null;
            m_CompleteAction = null;
            m_FailureAction = null;
            m_CancelAction = null;
        }
        /// <summary>
        /// 设置任务完成的条件。
        /// </summary>
        /// <param name="condition">任务完成的条件。</param>
        public void SetCondition(Predicate<ConditionalTask> condition)
        {
            m_Condition = condition;
        }
        /// <summary>
        /// 设置任务完成时的行为。
        /// </summary>
        /// <param name="completeAction">任务完成时的行为。</param>
        public void SetCompleteAction(Action<ConditionalTask, string> completeAction)
        {
            m_CompleteAction = completeAction;
        }
        /// <summary>
        /// 设置任务失败时的行为。
        /// </summary>
        /// <param name="failureAction">任务失败时的行为。</param>
        public void SetFailureAction(Action<ConditionalTask, string> failureAction)
        {
            m_FailureAction = failureAction;
        }
        /// <summary>
        /// 设置任务取消时的行为。
        /// </summary>
        /// <param name="cancelAction">任务取消时的行为。</param>
        public void SetCancelAction(Action<ConditionalTask, string> cancelAction)
        {
            m_CancelAction = cancelAction;
        }
        /// <summary>
        /// 任务开始时调用。
        /// </summary>
        protected internal override void OnStart()
        {
            base.OnStart();

            if (m_Condition == null)
            {
                OnFailure("Condition is invalid.");
                return;
            }

            if (m_CompleteAction == null)
            {
                OnFailure("Complete action is invalid.");
                return;
            }
        }
        /// <summary>
        /// 任务轮询时调用。
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间，以秒为单位。</param>
        protected internal override void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            base.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
            if (!m_Condition(this))
            {
                return;
            }
            OnComplete("Conditional reach");
        }
        /// <summary>
        /// 任务完成时调用。
        /// </summary>
        /// <param name="reason">任务完成的原因。</param>
        protected internal override void OnComplete(string reason)
        {
            base.OnComplete(reason);
            m_CompleteAction?.Invoke(this, reason);
        }
        /// <summary>
        /// 任务失败时调用。
        /// </summary>
        /// <param name="reason">任务失败的原因。</param>
        protected internal override void OnFailure(string reason)
        {
            base.OnFailure(reason);
            m_FailureAction?.Invoke(this, reason);
        }
        /// <summary>
        /// 任务取消时调用。
        /// </summary>
        /// <param name="reason">任务取消的原因。</param>
        protected internal override void OnCancel(string reason)
        {
            base.OnCancel(reason);
            m_CancelAction?.Invoke(this, reason);
        }
    }
}
