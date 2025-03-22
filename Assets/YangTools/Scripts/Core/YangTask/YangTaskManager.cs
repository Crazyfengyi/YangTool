/*
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-18 
*/

using System;
using System.Collections.Generic;
using YangTools.Scripts.Core.YangToolsManager;

namespace YangTools.Scripts.Core.YangTask
{
    /// <summary>
    /// 任务管理器。
    /// </summary>
    public sealed class YangTaskManager : GameModuleBase, ITaskManager
    {
        #region 属性
        private readonly LinkedList<TaskBase> allTasksList;//所有任务列表
        private int serial;//序列号
        /// <summary>
        /// 初始化任务管理器的新实例。
        /// </summary>
        public YangTaskManager()
        {
            allTasksList = new LinkedList<TaskBase>();
            serial = 0;
        }
        /// <summary>
        /// 获取任务数量。
        /// </summary>
        public int Count => allTasksList.Count;

        #endregion

        #region  生命周期
        internal override void InitModule()
        {
        }
        /// <summary>
        /// 任务管理器轮询
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间,以秒为单位</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间,以秒为单位</param>
        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            LinkedListNode<TaskBase> current = allTasksList.First;
            while (current != null)
            {
                TaskBase taskBase = current.Value;
                if (taskBase.Status == TaskStatus.None)
                {
                    throw new Exception("Task status is invalid. this None");
                }
                if (taskBase.Status == TaskStatus.Waiting)
                {
                    taskBase.OnStart();
                }
                if (taskBase.Status == TaskStatus.Running)
                {
                    taskBase.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
                    current = current.Next;
                }
                else
                {
                    LinkedListNode<TaskBase> next = current.Next;
                    allTasksList.Remove(current);
                    current = next;
                }
            }
        }
        /// <summary>
        /// 关闭并清理任务管理器
        /// </summary>
        internal override void CloseModule()
        {
            CancelAllTasks(null);
            allTasksList.Clear();
        }
        #endregion

        #region 生成任务
        /// <summary>
        /// 生成任务
        /// </summary>
        /// <typeparam name="T">任务的类型</typeparam>
        /// <returns>生成的指定类型的任务</returns>
        public T CreateTask<T>() where T : TaskBase, new()
        {
            return CreateTask<T>(TaskBase.DefaultPriority);
        }
        /// <summary>
        /// 生成任务
        /// </summary>
        /// <typeparam name="T">任务的类型</typeparam>
        /// <param name="priority">任务的优先级</param>
        /// <returns>生成的指定类型的任务</returns>
        public T CreateTask<T>(int priority) where T : TaskBase, new()
        {
            T task = (T)Activator.CreateInstance(typeof(T));
            task.Initialize(++serial, priority);
            task.OnCreate();
            LinkedListNode<TaskBase> current = allTasksList.First;
            while (current != null)
            {
                if (task.Priority > current.Value.Priority)
                {
                    break;
                }

                current = current.Next;
            }
            if (current != null)
            {
                allTasksList.AddBefore(current, task);
            }
            else
            {
                allTasksList.AddLast(task);
            }
            return task;
        }
        #endregion

        #region 取消任务
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="taskBase">要取消的任务</param>
        /// <returns>是否取消任务成功</returns>
        public bool CancelTask(TaskBase taskBase)
        {
            if (taskBase == null)
            {
                throw new Exception("Task is invalid.");
            }
            return CancelTask(taskBase.SerialId, null);
        }
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="taskBase">要取消的任务</param>
        /// <param name="reason">任务取消的原因</param>
        /// <returns>是否取消任务成功</returns>
        public bool CancelTask(TaskBase taskBase, string reason)
        {
            if (taskBase == null)
            {
                throw new Exception("Task is invalid.");
            }
            return CancelTask(taskBase.SerialId, reason);
        }
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号</param>
        /// <returns>是否取消任务成功</returns>
        public bool CancelTask(int serialId)
        {
            return CancelTask(serialId, null);
        }
        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号</param>
        /// <param name="reason">任务取消的原因</param>
        /// <returns>是否取消任务成功</returns>
        public bool CancelTask(int serialId, string reason)
        {
            foreach (TaskBase task in allTasksList)
            {
                if (task.SerialId != serialId)
                {
                    continue;
                }
                if (task.Status != TaskStatus.Waiting && task.Status != TaskStatus.Running)
                {
                    return false;
                }
                task.OnCancel(reason);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 取消所有任务
        /// </summary>
        /// <param name="reason">任务取消的原因</param>
        public void CancelAllTasks(string reason)
        {
            foreach (TaskBase task in allTasksList)
            {
                if (task.Status != TaskStatus.Waiting && task.Status != TaskStatus.Running)
                {
                    continue;
                }
                task.OnCancel(reason);
            }
        }
        #endregion
    }
}