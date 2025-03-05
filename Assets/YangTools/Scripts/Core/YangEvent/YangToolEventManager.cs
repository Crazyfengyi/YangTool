/* 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION#
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-06-14 
*/
using System.Collections.Generic;

namespace YangTools
{
    /// <summary>
    /// 事件管理器
    /// </summary>
    public class YangEventManager
    {
        /// <summary>
        /// 单例
        /// </summary>
        private static YangEventManager instance;
        /// <summary>
        /// 线程锁
        /// </summary>
        private static readonly object EventLock = new object();
        /// <summary>
        /// 单例
        /// </summary>
        public static YangEventManager Instance
        {
            get
            {
                if (instance != null) return instance;
                lock (EventLock)
                {
                    instance ??= new YangEventManager();
                }
                return instance;
            }
        }

        #region 可以绑定对象的事件管理器
        /// <summary>
        /// 事件字典
        /// </summary>
        private readonly Dictionary<string, List<EventInfo>> eventDic = new Dictionary<string, List<EventInfo>>();

        /// <summary>
        /// 添加事件
        /// </summary>
        public void Add(EventInfo eventListener)
        {
            if (eventDic.TryGetValue(eventListener.EventName, out var toAdd))
            {
                toAdd.Add(eventListener);
            }
            else
            {
                toAdd = new List<EventInfo> {eventListener};
                eventDic.Add(eventListener.EventName, toAdd);
            }
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void Remove(string eventName)
        {
            eventDic.Remove(eventName, out List<EventInfo> list);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void Remove(EventInfo eventInfo)
        {
            foreach (var i in eventDic)
            {
                i.Value.Remove(eventInfo);
            }
        }

        /// <summary>
        /// 移除对象绑定的事件
        /// </summary>
        public void Remove(UnityEngine.Object target)
        {
            foreach (var i in eventDic)
            {
                for (int k = i.Value.Count - 1; k >= 0; k--)
                {
                    EventInfo v = i.Value[k];
                    if (!ReferenceEquals(v.Holder, target)) continue;
                    i.Value.RemoveAt(k);
                    return;
                }
            }
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventArgs">参数列表</param>
        public void Send(string eventName, params object[] eventArgs)
        {
            if (eventDic.TryGetValue(eventName, out var list))
            {
                EventData e = new EventData(eventName, eventArgs);
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    EventInfo item = list[i];
                    if (!item.IsEnabled) continue;
                    if (!item.Invoke(e))
                    {
                        list.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// 清理所有事件
        /// </summary>
        public void Clear()
        {
            eventDic.Clear();
        }

        #endregion
    }

    #region 事件相关类
    /// <summary>
    /// 委托
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    /// <param name="arg">参数</param>
    public delegate void EventCallback<in T>(T arg);

    /// <summary>
    /// 事件信息
    /// </summary>
    [System.Serializable]
    public class EventInfo
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public readonly string EventName;
        /// <summary>
        /// 事件
        /// </summary>
        private readonly EventCallback<EventData> callback;
        /// <summary>
        /// 弱引用绑定的物体
        /// </summary>
        private readonly System.WeakReference<UnityEngine.Object> weakObjectTarget;
        /// <summary>
        /// 是否可以使用---弱引用目标是否存活(存活--可以使用,不存活--不可使用)
        /// </summary>
        public bool CanUse
        {
            get
            {
                weakObjectTarget.TryGetTarget(out var nowObj);
                return nowObj != null;
            }
        }
        /// <summary>
        /// 绑定的物体
        /// </summary>
        public UnityEngine.Object Holder
        {
            get
            {
                weakObjectTarget.TryGetTarget(out var nowObj);
                return nowObj;
            }
        }
        /// <summary>
        /// 可用状态
        /// </summary>
        public readonly bool IsEnabled = true;
        /// <summary>
        /// 创建事件检查者
        /// </summary>
        /// <param name="holder">(谁持有检查器)检查器绑定某个物体上(只要继承自UnityObject就行了),null表示全局监听</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventCallback">事件回调</param>
        public EventInfo(UnityEngine.Object holder, string eventName, EventCallback<EventData> eventCallback)
        {
            EventName = eventName;
            weakObjectTarget = new System.WeakReference<UnityEngine.Object>(holder);
            callback = eventCallback;
        }
        /// <summary>
        /// 调用事件
        /// </summary>
        public bool Invoke(EventData data)
        {
            if (!CanUse) return false;
            callback?.Invoke(data);
            return true;
        }
    }

    /// <summary>
    /// 事件数据
    /// </summary>
    public class EventData
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 事件参数
        /// </summary>
        public object[] Args { get; private set; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="args">事件参数</param>
        public EventData(string name, params object[] args)
        {
            Name = name;
            Args = args;
        }
    }
    #endregion
}