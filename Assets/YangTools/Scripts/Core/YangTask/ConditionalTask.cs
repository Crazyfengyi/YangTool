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
    public sealed class ConditionalTaskBase : TaskBase
    {
        private Predicate<ConditionalTaskBase> condition;//条件-返回bool
        private Action<ConditionalTaskBase, string> completeAction;//完成回调
        private Action<ConditionalTaskBase, string> failureAction;//失败回调
        private Action<ConditionalTaskBase, string> cancelAction;//关闭回调
        /// <summary>
        /// 初始化条件任务的新实例。
        /// </summary>
        public ConditionalTaskBase()
        {
            condition = null;
            completeAction = null;
            failureAction = null;
            cancelAction = null;
        }
        /// <summary>
        /// 设置任务完成的条件
        /// </summary>
        /// <param name="condition">任务完成的条件</param>
        public void SetCondition(Predicate<ConditionalTaskBase> condition)
        {
            this.condition = condition;
        }
        /// <summary>
        /// 设置任务完成时的行为
        /// </summary>
        /// <param name="completeAction">任务完成时的行为</param>
        public void SetCompleteAction(Action<ConditionalTaskBase, string> completeAction)
        {
            this.completeAction = completeAction;
        }
        /// <summary>
        /// 设置任务失败时的行为
        /// </summary>
        /// <param name="failureAction">任务失败时的行为</param>
        public void SetFailureAction(Action<ConditionalTaskBase, string> failureAction)
        {
            this.failureAction = failureAction;
        }
        /// <summary>
        /// 设置任务取消时的行为
        /// </summary>
        /// <param name="cancelAction">任务取消时的行为</param>
        public void SetCancelAction(Action<ConditionalTaskBase, string> cancelAction)
        {
            this.cancelAction = cancelAction;
        }
        /// <summary>
        /// 任务开始时调用
        /// </summary>
        protected internal override void OnStart()
        {
            base.OnStart();

            if (condition == null)
            {
                OnFailure("Condition is invalid.");
                return;
            }

            if (completeAction == null)
            {
                OnFailure("Complete action is invalid.");
                return;
            }
        }
        /// <summary>
        /// 任务轮询时调用
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        protected internal override void OnUpdate(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            base.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
            if (!condition(this))
            {
                return;
            }
            OnComplete("Conditional reach");
        }
        /// <summary>
        /// 任务完成时调用
        /// </summary>
        /// <param name="reason">任务完成的原因</param>
        protected internal override void OnComplete(string reason)
        {
            base.OnComplete(reason);
            completeAction?.Invoke(this, reason);
        }
        /// <summary>
        /// 任务失败时调用
        /// </summary>
        /// <param name="reason">任务失败的原因</param>
        protected internal override void OnFailure(string reason)
        {
            base.OnFailure(reason);
            failureAction?.Invoke(this, reason);
        }
        /// <summary>
        /// 任务取消时调用
        /// </summary>
        /// <param name="reason">任务取消的原因</param>
        protected internal override void OnCancel(string reason)
        {
            base.OnCancel(reason);
            cancelAction?.Invoke(this, reason);
        }
    }
}
