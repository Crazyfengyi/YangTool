/** 
 *Copyright(C) 2020 by DefaultCompany 
 *All rights reserved. 
 *Author:       DESKTOP-AJS8G4U 
 *UnityVersion：2021.2.1f1c1 
 *创建时间:         2022-02-18 
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YangTools.Task
{
    /// <summary>
    /// 任务管理器。
    /// </summary>
    internal sealed class YangTaskManager : GameModuleBase, ITaskManager
    {
        #region 属性
        private readonly LinkedList<TaskBase> allTasksList;//所有任务列表
        private int m_Serial;//序列号
        /// <summary>
        /// 初始化任务管理器的新实例。
        /// </summary>
        public YangTaskManager()
        {
            allTasksList = new LinkedList<TaskBase>();
            m_Serial = 0;
        }
        /// <summary>
        /// 获取任务数量。
        /// </summary>
        public int Count
        {
            get
            {
                return allTasksList.Count;
            }
        }
        #endregion

        #region  生命周期
        internal override void InitModule()
        {
        }
        /// <summary>
        /// 任务管理器轮询。
        /// </summary>
        /// <param name="delaTimeSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="unscaledDeltaTimeSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float delaTimeSeconds, float unscaledDeltaTimeSeconds)
        {
            LinkedListNode<TaskBase> current = allTasksList.First;
            while (current != null)
            {
                TaskBase task = current.Value;
                if (task.Status == TaskStatus.None)
                {
                    throw new Exception("Task status is invalid. this None");
                }
                if (task.Status == TaskStatus.Waiting)
                {
                    task.OnStart();
                }
                if (task.Status == TaskStatus.Running)
                {
                    task.OnUpdate(delaTimeSeconds, unscaledDeltaTimeSeconds);
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
        /// 关闭并清理任务管理器。
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
        /// <typeparam name="T">任务的类型。</typeparam>
        /// <returns>生成的指定类型的任务。</returns>
        public T CreateTask<T>() where T : TaskBase, new()
        {
            return CreateTask<T>(TaskBase.DefaultPriority);
        }
        /// <summary>
        /// 生成任务。
        /// </summary>
        /// <typeparam name="T">任务的类型。</typeparam>
        /// <param name="priority">任务的优先级。</param>
        /// <returns>生成的指定类型的任务。</returns>
        public T CreateTask<T>(int priority) where T : TaskBase, new()
        {
            T task = (T)Activator.CreateInstance(typeof(T));
            task.Initialize(++m_Serial, priority);
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
        /// 取消任务。
        /// </summary>
        /// <param name="task">要取消的任务。</param>
        /// <returns>是否取消任务成功。</returns>
        public bool CancelTask(TaskBase task)
        {
            if (task == null)
            {
                throw new Exception("Task is invalid.");
            }
            return CancelTask(task.SerialId, null);
        }
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="task">要取消的任务。</param>
        /// <param name="reason">任务取消的原因。</param>
        /// <returns>是否取消任务成功。</returns>
        public bool CancelTask(TaskBase task, string reason)
        {
            if (task == null)
            {
                throw new Exception("Task is invalid.");
            }
            return CancelTask(task.SerialId, reason);
        }
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号。</param>
        /// <returns>是否取消任务成功。</returns>
        public bool CancelTask(int serialId)
        {
            return CancelTask(serialId, null);
        }
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号。</param>
        /// <param name="reason">任务取消的原因。</param>
        /// <returns>是否取消任务成功。</returns>
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
        /// 取消所有任务。
        /// </summary>
        /// <param name="reason">任务取消的原因。</param>
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
    /// <summary>
    /// 任务管理器。
    /// </summary>
    public interface ITaskManager
    {
        /// <summary>
        /// 获取任务数量。
        /// </summary>
        int Count
        {
            get;
        }
        /// <summary>
        /// 生成任务。
        /// </summary>
        /// <typeparam name="T">任务的类型。</typeparam>
        /// <returns>生成的指定类型的任务。</returns>
        T CreateTask<T>() where T : TaskBase, new();
        /// <summary>
        /// 生成任务。
        /// </summary>
        /// <typeparam name="T">任务的类型。</typeparam>
        /// <param name="priority">任务的优先级。</param>
        /// <returns>生成的指定类型的任务。</returns>
        T CreateTask<T>(int priority) where T : TaskBase, new();
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="task">要取消的任务。</param>
        /// <returns>是否取消任务成功。</returns>
        bool CancelTask(TaskBase task);
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="task">要取消的任务。</param>
        /// <param name="reason">任务取消的原因。</param>
        /// <returns>是否取消任务成功。</returns>
        bool CancelTask(TaskBase task, string reason);
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号。</param>
        /// <returns>是否取消任务成功。</returns>
        bool CancelTask(int serialId);
        /// <summary>
        /// 取消任务。
        /// </summary>
        /// <param name="serialId">要取消的任务的序列编号。</param>
        /// <param name="reason">任务取消的原因。</param>
        /// <returns>是否取消任务成功。</returns>
        bool CancelTask(int serialId, string reason);
        /// <summary>
        /// 取消所有任务。
        /// </summary>
        /// <param name="reason">任务取消的原因。</param>
        void CancelAllTasks(string reason);
    }
}