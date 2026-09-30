/* 
 *Copyright(C) 2020 by Yang 
 *All rights reserved. 
 *脚本功能:     #FUNCTION#
 *Author:       陈春洋 
 *UnityVersion：2019.3.3f1 
 *创建时间:         2020-06-14 
*/

using System;
using System.Collections.Generic;
using UnityEngine;

/*
 *  YangExtend.AddEventListener<DefaultEventMsg>(gameObject, (msg) =>
    {
        Debug.LogError($"收到事件:{msg.Name}--{msg.Args}");
    });
    Debug.LogError("测试1");
    yield return new WaitForSeconds(1);
    Debug.LogError("测试2");
    YangExtend.SendEvent<DefaultEventMsg>(new DefaultEventMsg());
    
    DefaultEventMsg temp = new DefaultEventMsg();
    temp.SendEvent();
 */
namespace YangTools
{
    /// <summary>
    /// 主线程同步事件管理器
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
        private readonly Dictionary<string, EventBucket> eventDic = new Dictionary<string, EventBucket>();

        /// <summary>
        /// 每次进入运行模式清理监听 兼容关闭域重载
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetListeners()
        {
            instance?.Clear();
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        public void Add(EventInfo eventInfo)
        {
            if (eventInfo == null)
            {
                throw new ArgumentNullException(nameof(eventInfo));
            }

            if (eventInfo.RegisteredManager != null)
            {
                return;
            }

            if (!eventDic.TryGetValue(eventInfo.EventName, out EventBucket bucket))
            {
                bucket = new EventBucket();
                eventDic.Add(eventInfo.EventName, bucket);
            }

            if (!bucket.Listeners.TryGetValue(eventInfo.SortId, out List<EventInfo> listeners))
            {
                listeners = new List<EventInfo>();
                bucket.Listeners.Add(eventInfo.SortId, listeners);
            }

            eventInfo.RegisteredManager = this;
            eventInfo.RegistrationVersion++;
            listeners.Add(eventInfo);
            bucket.Snapshot = null;
        }

        /// <summary>
        /// 移除Key对应所有事件
        /// </summary>
        public void RemoveForKey(string eventName)
        {
            if (eventName == null || !eventDic.TryGetValue(eventName, out EventBucket bucket)) return;
            bucket.UnregisterAll();
            eventDic.Remove(eventName);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void Remove(EventInfo eventInfo)
        {
            if (eventInfo == null || eventInfo.RegisteredManager != this) return;
            if (!eventDic.TryGetValue(eventInfo.EventName, out EventBucket bucket)) return;
            if (!bucket.Listeners.TryGetValue(eventInfo.SortId, out List<EventInfo> listeners)) return;
            if (!listeners.Remove(eventInfo)) return;

            eventInfo.RegisteredManager = null;
            bucket.Snapshot = null;
            if (listeners.Count == 0)
            {
                bucket.Listeners.Remove(eventInfo.SortId);
            }

            if (bucket.Listeners.Count == 0) eventDic.Remove(eventInfo.EventName);
        }

        /// <summary>
        /// 移除对象绑定的事件
        /// </summary>
        public void Remove(UnityEngine.Object target)
        {
            if (ReferenceEquals(target, null)) return;

            List<string> emptyKeys = null;
            foreach (var pair in eventDic)
            {
                EventBucket bucket = pair.Value;
                for (int i = bucket.Listeners.Count - 1; i >= 0; i--)
                {
                    List<EventInfo> listeners = bucket.Listeners.Values[i];
                    for (int k = listeners.Count - 1; k >= 0; k--)
                    {
                        EventInfo eventInfo = listeners[k];
                        if (!ReferenceEquals(eventInfo.Holder, target)) continue;
                        eventInfo.RegisteredManager = null;
                        listeners.RemoveAt(k);
                        bucket.Snapshot = null;
                    }

                    if (listeners.Count == 0) bucket.Listeners.RemoveAt(i);
                }

                if (bucket.Listeners.Count > 0) continue;
                emptyKeys ??= new List<string>();
                emptyKeys.Add(pair.Key);
            }

            if (emptyKeys == null) return;
            for (int i = 0; i < emptyKeys.Count; i++) eventDic.Remove(emptyKeys[i]);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="eventArgs">参数列表</param>
        public void Send(string eventName, EventMessageBase eventArgs)
        {
            Send(eventName, (object)eventArgs);
        }

        /// <summary>
        /// 发送任意任务或项目事件参数
        /// </summary>
        public void Send(string eventName, object eventArgs)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentException("事件名称不能为空", nameof(eventName));
            }

            if (!eventDic.TryGetValue(eventName, out EventBucket bucket)) return;

            ListenerSnapshot[] snapshot = bucket.GetSnapshot();
            EventData eventData = new EventData(eventName, eventArgs);
            for (int i = 0; i < snapshot.Length; i++)
            {
                EventInfo listener = snapshot[i].Listener;
                if (listener.RegisteredManager != this || listener.RegistrationVersion != snapshot[i].Version) continue;
                if (!listener.CanUse)
                {
                    Remove(listener);
                    continue;
                }

                if (!listener.isEnabled) continue;
                try
                {
                    listener.Invoke(eventData);
                }
                catch (Exception exception)
                {
                    Debug.LogError($"事件回调异常 [{eventName}]\n{exception}", listener.Holder);
                }
            }
        }

        /// <summary>
        /// 清理所有事件
        /// </summary>
        public void Clear()
        {
            foreach (EventBucket bucket in eventDic.Values) bucket.UnregisterAll();
            eventDic.Clear();
        }

        #endregion

        #region 派发快照

        /// <summary>
        /// 同名事件的有序监听和缓存快照
        /// </summary>
        private sealed class EventBucket
        {
            /// <summary>
            /// 按优先级保存监听
            /// </summary>
            private readonly SortedList<int, List<EventInfo>> listeners = new SortedList<int, List<EventInfo>>();
            /// <summary>
            /// 监听变化前复用的派发快照
            /// </summary>
            private ListenerSnapshot[] snapshot;

            internal SortedList<int, List<EventInfo>> Listeners => listeners;
            internal ListenerSnapshot[] Snapshot { get => snapshot; set => snapshot = value; }

            /// <summary>
            /// 只在监听变化后构建新快照 嵌套派发不会改写外层快照
            /// </summary>
            internal ListenerSnapshot[] GetSnapshot()
            {
                if (snapshot != null) return snapshot;
                int count = 0;
                for (int i = 0; i < listeners.Count; i++) count += listeners.Values[i].Count;
                snapshot = new ListenerSnapshot[count];
                int index = 0;
                for (int i = 0; i < listeners.Count; i++)
                {
                    List<EventInfo> sameSortListeners = listeners.Values[i];
                    for (int k = 0; k < sameSortListeners.Count; k++)
                    {
                        snapshot[index++] = new ListenerSnapshot(sameSortListeners[k]);
                    }
                }

                return snapshot;
            }

            /// <summary>
            /// 让已有派发快照中的监听立即失效
            /// </summary>
            internal void UnregisterAll()
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    List<EventInfo> sameSortListeners = listeners.Values[i];
                    for (int k = 0; k < sameSortListeners.Count; k++)
                    {
                        sameSortListeners[k].RegisteredManager = null;
                    }
                }

                snapshot = null;
            }
        }

        /// <summary>
        /// 监听与注册版本 防止注销后重新注册触发旧快照
        /// </summary>
        private readonly struct ListenerSnapshot
        {
            /// <summary>
            /// 本轮派发的监听句柄
            /// </summary>
            internal readonly EventInfo Listener;
            /// <summary>
            /// 本轮捕获的注册版本
            /// </summary>
            internal readonly long Version;

            /// <summary>
            /// 捕获当前注册版本
            /// </summary>
            internal ListenerSnapshot(EventInfo listener)
            {
                Listener = listener;
                Version = listener.RegistrationVersion;
            }
        }

        #endregion
    }

    #region 事件相关类
    /// <summary>
    /// 事件信息
    /// </summary>
    [System.Serializable]
    public class EventInfo
    {
        /// <summary>
        /// 排序ID
        /// </summary>
        public readonly int SortId;
        /// <summary>
        /// 事件名称
        /// </summary>
        public readonly string EventName;
        /// <summary>
        /// 事件
        /// </summary>
        private readonly Action<EventData> callback;
        /// <summary>
        /// 弱引用绑定的物体
        /// </summary>
        private readonly System.WeakReference<UnityEngine.Object> weakObjectTarget;
        /// <summary>
        /// 真正的空引用表示全局监听 与 Unity 已销毁对象区分
        /// </summary>
        private readonly bool isGlobal;

        internal YangEventManager RegisteredManager { get; set; }
        internal long RegistrationVersion { get; set; }
        /// <summary>
        /// 是否可以使用---弱引用目标是否存活(存活--可以使用,不存活--不可使用)
        /// </summary>
        public bool CanUse
        {
            get
            {
                if (isGlobal) return true;
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
        public bool isEnabled = true;

        /// <summary>
        /// 创建事件检查者
        /// </summary>
        /// <param name="holder">(谁持有检查器)检查器绑定某个物体上(只要继承自UnityObject就行了),null表示全局监听</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="action">事件回调</param>
        /// <param name="sortId">触发优先级排序</param>
        public EventInfo(UnityEngine.Object holder, string eventName, Action<EventData> action,int sortId = 0)
        {
            if (string.IsNullOrWhiteSpace(eventName)) throw new ArgumentException("事件名称不能为空", nameof(eventName));
            if (action == null) throw new ArgumentNullException(nameof(action));
            SortId = sortId;
            EventName = eventName;
            isGlobal = ReferenceEquals(holder, null);
            weakObjectTarget = new System.WeakReference<UnityEngine.Object>(holder);
            callback = action;
        }
        /// <summary>
        /// 调用事件--返回是否可用
        /// </summary>
        public bool Invoke(EventData data)
        {
            if (!CanUse)
            {
                return false;
            }
            callback?.Invoke(data);
            return true;
        }

        /// <summary>
        /// 比较委托 用于同一分组的监听去重
        /// </summary>
        internal bool HasCallback(Action<EventData> listener)
        {
            return callback == listener;
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
        public object Args { get; private set; }
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="name">事件名称</param>
        /// <param name="args">事件参数</param>
        public EventData(string name, object args)
        {
            Name = name;
            Args = args;
        }
    }
    /// <summary>
    /// 事件参数
    /// </summary>
    public class EventMessageBase
    {
        /// <summary>
        /// 按实际事件类型发送消息
        /// </summary>
        public void SendEvent()
        {
            YangExtend.SendEvent(GetType(),this);
        }
    }
    
    /// <summary>
    /// 默认事件参数
    /// </summary>
    public class DefaultEventMsg : EventMessageBase
    {
    
    }
    #endregion

    #region 事件分组

    /// <summary>
    /// 管理一组全局监听 由持有者负责在生命周期结束时释放
    /// </summary>
    public class YangEventGroup : IDisposable
    {
        /// <summary>
        /// 本组持有的监听句柄
        /// </summary>
        private readonly Dictionary<string, List<EventInfo>> groupCachedListener = new Dictionary<string, List<EventInfo>>();
        /// <summary>
        /// 添加一个监听
        /// </summary>
        public void AddListener<T>(Action<EventData> listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            string key = typeof(T).FullName;
            if (key == null) throw new ArgumentException("事件类型必须有完整名称");
            if (!groupCachedListener.TryGetValue(key, out List<EventInfo> listeners))
            {
                listeners = new List<EventInfo>();
                groupCachedListener.Add(key, listeners);
            }

            for (int i = 0; i < listeners.Count; i++)
            {
                if (!listeners[i].HasCallback(listener)) continue;
                YangEventManager.Instance.Add(listeners[i]);
                return;
            }

            EventInfo eventInfo = new EventInfo(null, key, listener);
            listeners.Add(eventInfo);
            YangEventManager.Instance.Add(eventInfo);
        }

        /// <summary>
        /// 移除所有缓存的监听
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (List<EventInfo> listeners in groupCachedListener.Values)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    YangEventManager.Instance.Remove(listeners[i]);
                }
            }
            groupCachedListener.Clear();
        }

        /// <summary>
        /// 释放本组监听 重复调用安全 之后仍可重新添加监听
        /// </summary>
        public void Dispose()
        {
            RemoveAllListener();
        }
    }
    #endregion
}
